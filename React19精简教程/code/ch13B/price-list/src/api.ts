// src/api.ts
// 所有「写」接口集中放在这里。页面和 Hook 只调用这些函数，不直接拼地址、写请求头
import type { Product } from './types.ts'

// 新增、修改时提交的内容：Product 去掉 id。id 由服务器分配，前端不能自己定
export type ProductInput = Omit<Product, 'id'>

const BASE = import.meta.env.VITE_API_BASE

// 失败时从响应里取出给用户看的文字。模拟接口出错时返回 { message, errors? }
async function failMessage(response: Response): Promise<string> {
  try {
    const body: { message?: string; errors?: Record<string, string> } = await response.json()
    // 有逐个字段的错误时（例如编号重复），把它们连起来显示，比「校验失败」更具体
    if (body.errors) {
      return Object.values(body.errors).join('；')
    }
    return body.message ?? `服务器返回 ${response.status}`
  } catch {
    // 响应内容不是 JSON（例如代理返回的 502 页面），只能显示状态码
    return `服务器返回 ${response.status}`
  }
}

export async function createProduct(input: ProductInput): Promise<Product> {
  const response = await fetch(`${BASE}/products`, {
    // 默认是 GET，新增要写明 POST
    method: 'POST',
    // 告诉服务器：请求体是 JSON。不写的话很多后端不知道怎么解析，会拒绝
    headers: { 'Content-Type': 'application/json' },
    // 请求体只能是字符串，把对象转成 JSON 文本
    body: JSON.stringify(input),
  })
  if (!response.ok) {
    throw new Error(await failMessage(response))
  }
  // 服务器返回保存后的产品，带上了它分配的 id
  const saved: Product = await response.json()
  return saved
}

export async function updateProduct(id: number, input: ProductInput): Promise<Product> {
  const response = await fetch(`${BASE}/products/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(input),
  })
  if (!response.ok) {
    throw new Error(await failMessage(response))
  }
  const saved: Product = await response.json()
  return saved
}

export async function deleteProduct(id: number): Promise<void> {
  const response = await fetch(`${BASE}/products/${id}`, { method: 'DELETE' })
  if (!response.ok) {
    throw new Error(await failMessage(response))
  }
  // 删除成功返回 204，没有内容，不要调用 response.json()
}
