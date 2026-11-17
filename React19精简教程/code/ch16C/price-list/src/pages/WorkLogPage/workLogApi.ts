// src/pages/WorkLogPage/workLogApi.ts
// 工作记录的接口函数。组件只调用这些函数，不自己拼地址、不自己判断状态码
import { toApiError } from '../../api.ts'
// 🟨 第 16C 章：多导入 WorkLogInput
import type { WorkLog, WorkLogInput, WorkLogQuery } from './workLogTypes.ts'

const BASE = import.meta.env.VITE_API_BASE

// 把查询条件变成查询字符串，例如 start=2026-10-01&workType=%E7%BB%B4%E4%BF%AE
function toQueryString(query: WorkLogQuery): string {
  const params = new URLSearchParams()
  // 只放有值的条件。空的不放，后端就按「不限」处理
  if (query.start !== '') params.set('start', query.start)
  if (query.end !== '') params.set('end', query.end)
  if (query.workType !== '') params.set('workType', query.workType)
  // 医院名称去掉首尾空格再发。中文、&、#、空格这些字符由 URLSearchParams 负责编码
  const hospital = query.hospital.trim()
  if (hospital !== '') params.set('hospital', hospital)
  return params.toString()
}

// GET /api/work-logs?...：按条件查询，返回符合条件的记录
export async function searchWorkLogs(query: WorkLogQuery): Promise<WorkLog[]> {
  const qs = toQueryString(query)
  // 一个条件都没有时，地址后面不留一个孤零零的 ?
  const url = qs === '' ? `${BASE}/work-logs` : `${BASE}/work-logs?${qs}`
  const response = await fetch(url)
  if (!response.ok) {
    throw await toApiError(response)
  }
  const rows: WorkLog[] = await response.json()
  return rows
}

export async function deleteWorkLog(id: number): Promise<void> {
  const response = await fetch(`${BASE}/work-logs/${id}`, { method: 'DELETE' })
  if (!response.ok) {
    // 404（记录已经不在了）等情况，抛出带服务器原因的 ApiError
    throw await toApiError(response)
  }
}

// 🟨 第 16C 章：POST /api/work-logs。成功返回 201 和保存后的记录（带上服务器分配的 id）
export async function createWorkLog(input: WorkLogInput): Promise<WorkLog> {
  const response = await fetch(`${BASE}/work-logs`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(input),
  })
  if (!response.ok) {
    // 校验失败时是 400，ApiError 的 fieldErrors 里是逐个字段的原因
    throw await toApiError(response)
  }
  const saved: WorkLog = await response.json()
  return saved
}
