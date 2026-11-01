// mock-server/server.ts
// 本地模拟接口：只用 Node.js 自带的 http 模块，不用安装任何东西。
// 数据放在内存里：增删改在服务器运行期间一直有效，关掉服务器再启动就恢复成初始的 5 个产品。
// 启动方法：在项目目录执行 npm run mock
import { createServer } from 'node:http'
import type { IncomingMessage, ServerResponse } from 'node:http'

type Product = { id: number; code: string; name: string; category: string; price: number; stock: number }
type ProductInput = Omit<Product, 'id'>
type User = { username: string; password: string; name: string; role: 'admin' | 'sales' }

const PORT = 3001
// 每个请求故意延迟 500 毫秒，让「加载中」「保存中」看得见。可以用环境变量 MOCK_DELAY 改
const DELAY = Number(process.env.MOCK_DELAY ?? 500)
const CATEGORIES = ['探头', '主机', '耗材']

let products: Product[] = [
  { id: 1, code: 'P-001', name: '凸阵探头 C5-2', category: '探头', price: 12800, stock: 6 },
  { id: 2, code: 'P-002', name: '线阵探头 L12-5', category: '探头', price: 15600, stock: 3 },
  { id: 3, code: 'H-001', name: '便携彩超主机', category: '主机', price: 168000, stock: 2 },
  { id: 4, code: 'C-001', name: '耦合剂 250ml', category: '耗材', price: 18, stock: 120 },
  { id: 5, code: 'C-002', name: '探头保护套', category: '耗材', price: 6.5, stock: 0 },
]

const users: User[] = [
  { username: 'admin', password: '123456', name: '系统管理员', role: 'admin' },
  { username: 'zhangsan', password: '123456', name: '张三', role: 'sales' },
]

// 返回 JSON。body 不传时（例如删除成功的 204）不带内容
function send(res: ServerResponse, status: number, body?: unknown) {
  res.writeHead(status, { 'Content-Type': 'application/json; charset=utf-8' })
  res.end(body === undefined ? '' : JSON.stringify(body))
}

// 读出请求体并按 JSON 解析。内容不是合法 JSON 时 JSON.parse 会抛错，由下面统一处理
async function readJson(req: IncomingMessage): Promise<Record<string, unknown>> {
  let text = ''
  for await (const chunk of req) {
    text += String(chunk)
  }
  const value: unknown = text === '' ? {} : JSON.parse(text)
  return typeof value === 'object' && value !== null ? (value as Record<string, unknown>) : {}
}

// 后端校验：前端校验过了也要再查一遍，前端的检查可以被绕过。selfId 是编辑时自己的 id，查重时排除自己
function validate(body: Record<string, unknown>, selfId: number | null) {
  const input: ProductInput = {
    code: typeof body.code === 'string' ? body.code.trim() : '',
    name: typeof body.name === 'string' ? body.name.trim() : '',
    category: typeof body.category === 'string' ? body.category : '',
    price: typeof body.price === 'number' ? body.price : NaN,
    stock: typeof body.stock === 'number' ? body.stock : NaN,
  }
  const errors: Record<string, string> = {}
  if (input.code === '') {
    errors.code = '请填写编号'
  } else if (products.some((p) => p.code === input.code && p.id !== selfId)) {
    errors.code = `编号 ${input.code} 已存在`
  }
  if (input.name === '') errors.name = '请填写名称'
  if (!CATEGORIES.includes(input.category)) errors.category = '分类必须是探头、主机或耗材'
  if (!(input.price > 0)) errors.price = '单价必须大于 0'
  if (!(Number.isInteger(input.stock) && input.stock >= 0)) errors.stock = '库存必须是不小于 0 的整数'
  return { input, errors }
}

async function handle(req: IncomingMessage, res: ServerResponse) {
  const method = req.method ?? 'GET'
  const path = new URL(req.url ?? '/', `http://localhost:${PORT}`).pathname
  // /api/products/3 这种地址，取出最后的数字
  const match = path.match(/^\/api\/products\/(\d+)$/)
  const id = match ? Number(match[1]) : null

  // 新增、修改、登录都要求请求体是 JSON，并在请求头里写明。真实后端（比如 Spring Boot）也是这样，不写会返回 415
  const isJson = (req.headers['content-type'] ?? '').includes('application/json')
  if ((method === 'POST' || method === 'PUT') && !isJson) {
    return send(res, 415, { message: '请求头 Content-Type 必须是 application/json' })
  }

  if (path === '/api/products' && method === 'GET') {
    return send(res, 200, products)
  }

  if (path === '/api/products' && method === 'POST') {
    const { input, errors } = validate(await readJson(req), null)
    if (Object.keys(errors).length > 0) {
      return send(res, 400, { message: '校验失败', errors })
    }
    const product: Product = { id: Math.max(0, ...products.map((p) => p.id)) + 1, ...input }
    products = [...products, product]
    return send(res, 201, product)
  }

  if (id !== null) {
    const existing = products.find((p) => p.id === id)
    if (!existing) {
      return send(res, 404, { message: `产品 ${id} 不存在` })
    }
    if (method === 'GET') {
      return send(res, 200, existing)
    }
    if (method === 'PUT') {
      const { input, errors } = validate(await readJson(req), id)
      if (Object.keys(errors).length > 0) {
        return send(res, 400, { message: '校验失败', errors })
      }
      const updated: Product = { id, ...input }
      products = products.map((p) => (p.id === id ? updated : p))
      return send(res, 200, updated)
    }
    if (method === 'DELETE') {
      // 业务规则：还有库存的产品不能删除。用来演示「后端拒绝了操作」
      if (existing.stock > 0) {
        return send(res, 409, { message: `${existing.name} 还有库存 ${existing.stock}，不能删除` })
      }
      products = products.filter((p) => p.id !== id)
      return send(res, 204)
    }
  }

  if (path === '/api/login' && method === 'POST') {
    const body = await readJson(req)
    const user = users.find((u) => u.username === body.username && u.password === body.password)
    if (!user) {
      return send(res, 401, { message: '用户名或密码错误' })
    }
    // 不返回密码
    return send(res, 200, { username: user.username, name: user.name, role: user.role })
  }

  send(res, 404, { message: `没有这个接口：${method} ${path}` })
}

const server = createServer(async (req, res) => {
  await new Promise((resolve) => setTimeout(resolve, DELAY))
  // 每个请求打印一行，方便在终端里看到前端到底发了什么
  console.log(`${new Date().toLocaleTimeString()} ${req.method} ${req.url}`)
  try {
    await handle(req, res)
  } catch {
    send(res, 400, { message: '请求内容不是合法的 JSON' })
  }
})

server.listen(PORT, () => {
  console.log(`模拟接口已启动：http://localhost:${PORT}/api/products`)
})
