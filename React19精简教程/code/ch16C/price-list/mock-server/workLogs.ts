// mock-server/workLogs.ts
// 工作记录的三个模拟接口：查询、新增、删除。第 16A–16C 章都用它，第 16A 章一次写好。
// 这个文件只负责「收到什么请求 → 返回什么状态码和内容」，真正写回响应由 server.ts 统一做。

type WorkType = '安装' | '维修' | '其它'
type WorkLog = { id: number; startDate: string; endDate: string; workType: WorkType; hospitalName: string }

// 处理结果：状态码 + 要返回的内容。body 不写表示没有内容（删除成功的 204）
export type WorkLogResult = { status: number; body?: unknown }

const WORK_TYPES: WorkType[] = ['安装', '维修', '其它']

// 日期一律是 'YYYY-MM-DD' 格式的字符串，和 <input type="date"> 的 value 格式相同
let workLogs: WorkLog[] = [
  { id: 1, startDate: '2026-09-02', endDate: '2026-09-03', workType: '安装', hospitalName: '市人民医院' },
  { id: 2, startDate: '2026-09-15', endDate: '2026-09-15', workType: '维修', hospitalName: '市第二人民医院' },
  { id: 3, startDate: '2026-10-08', endDate: '2026-10-09', workType: '安装', hospitalName: '市中心医院' },
  { id: 4, startDate: '2026-10-12', endDate: '2026-10-12', workType: '其它', hospitalName: '市人民医院' },
  { id: 5, startDate: '2026-10-20', endDate: '2026-10-22', workType: '维修', hospitalName: '县中医院' },
  { id: 6, startDate: '2026-11-03', endDate: '2026-11-03', workType: '维修', hospitalName: '市妇幼保健院' },
]

// 判断是不是真实存在的日期：格式对，并且不是 2026-02-30 这种「格式对、日子不存在」的写法
function isValidDate(value: string): boolean {
  if (!/^\d{4}-\d{2}-\d{2}$/.test(value)) return false
  const d = new Date(`${value}T00:00:00Z`)
  return !Number.isNaN(d.getTime()) && d.toISOString().slice(0, 10) === value
}

function isWorkType(value: string): value is WorkType {
  return WORK_TYPES.some((t) => t === value)
}

// GET /api/work-logs?start=...&end=...&workType=...&hospital=...
// 四个条件都可以不传；传了空字符串也当作「不限」
function search(params: URLSearchParams): WorkLogResult {
  const start = params.get('start') ?? ''
  const end = params.get('end') ?? ''
  const workType = params.get('workType') ?? ''
  const hospital = (params.get('hospital') ?? '').trim()

  if (start !== '' && !isValidDate(start)) return { status: 400, body: { message: `开始日期格式不对：${start}` } }
  if (end !== '' && !isValidDate(end)) return { status: 400, body: { message: `结束日期格式不对：${end}` } }
  // 'YYYY-MM-DD' 的字符串按字典顺序比较，结果和按日期先后比较一样，所以可以直接用 > 比
  if (start !== '' && end !== '' && start > end) return { status: 400, body: { message: '开始日期不能晚于结束日期' } }
  if (workType !== '' && !isWorkType(workType)) return { status: 400, body: { message: `没有这个工作种类：${workType}` } }

  const rows = workLogs
    // 记录的开始日期不早于查询的开始日期
    .filter((w) => start === '' || w.startDate >= start)
    // 记录的结束日期不晚于查询的结束日期
    .filter((w) => end === '' || w.endDate <= end)
    .filter((w) => workType === '' || w.workType === workType)
    // 医院名称模糊匹配：名称里包含输入的文字就算
    .filter((w) => hospital === '' || w.hospitalName.includes(hospital))
    // 新的在前：按开始日期从晚到早，同一天按 id 从大到小
    .sort((a, b) => (a.startDate === b.startDate ? b.id - a.id : a.startDate < b.startDate ? 1 : -1))
  return { status: 200, body: rows }
}

// POST /api/work-logs。前端校验过也要再查一遍：请求可能不是从页面发来的
function create(body: Record<string, unknown>): WorkLogResult {
  const startDate = typeof body.startDate === 'string' ? body.startDate : ''
  const endDate = typeof body.endDate === 'string' ? body.endDate : ''
  const workType = typeof body.workType === 'string' ? body.workType : ''
  const hospitalName = typeof body.hospitalName === 'string' ? body.hospitalName.trim() : ''

  const errors: Record<string, string> = {}
  if (!isValidDate(startDate)) errors.startDate = '请选择开始日期'
  if (!isValidDate(endDate)) errors.endDate = '请选择结束日期'
  else if (isValidDate(startDate) && endDate < startDate) errors.endDate = '结束日期不能早于开始日期'
  if (!isWorkType(workType)) errors.workType = '工作种类必须是安装、维修或其它'
  if (hospitalName === '') errors.hospitalName = '请填写医院名称'
  else if (hospitalName.length > 50) errors.hospitalName = '医院名称不能超过 50 个字'
  // 只有后端能做的检查：要看全部已有数据才知道是否重复
  else if (workLogs.some((w) => w.startDate === startDate && w.workType === workType && w.hospitalName === hospitalName)) {
    errors.hospitalName = `${hospitalName} 在 ${startDate} 已经有一条「${workType}」记录`
  }
  // 后半个条件是写给 TypeScript 看的：过了这一关，它才确定 workType 是 WorkType 而不是任意字符串
  if (Object.keys(errors).length > 0 || !isWorkType(workType)) {
    return { status: 400, body: { message: '校验失败', errors } }
  }

  const saved: WorkLog = { id: Math.max(0, ...workLogs.map((w) => w.id)) + 1, startDate, endDate, workType, hospitalName }
  workLogs = [...workLogs, saved]
  return { status: 201, body: saved }
}

// DELETE /api/work-logs/3
function remove(id: number): WorkLogResult {
  if (!workLogs.some((w) => w.id === id)) {
    return { status: 404, body: { message: `工作记录 ${id} 不存在，可能已经被删除` } }
  }
  workLogs = workLogs.filter((w) => w.id !== id)
  return { status: 204 }
}

// 地址不是 /api/work-logs 开头时返回 null，交回 server.ts 按原来的规则处理
export async function handleWorkLogs(
  method: string,
  url: URL,
  readBody: () => Promise<Record<string, unknown>>,
): Promise<WorkLogResult | null> {
  if (url.pathname === '/api/work-logs') {
    if (method === 'GET') return search(url.searchParams)
    if (method === 'POST') return create(await readBody())
  }
  const match = url.pathname.match(/^\/api\/work-logs\/(\d+)$/)
  if (match && method === 'DELETE') return remove(Number(match[1]))
  return null
}
