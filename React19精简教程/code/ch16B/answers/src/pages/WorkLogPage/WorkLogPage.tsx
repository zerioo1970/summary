// src/pages/WorkLogPage/WorkLogPage.tsx
// 练习做完后的版本，🟨 标出练习改动的地方
// 工作记录页：上、中、下三个 panel 的父组件（第 16A、16B 章先做上、中两个）。
// 「生效的查询条件」和「表格数据」放在这里：第 16B、16C 章的删除和录入也要让表格重新查询
import { useEffect, useState } from 'react'
import { QueryPanel } from './QueryPanel.tsx'
import { ResultTable } from './ResultTable.tsx'
import { deleteWorkLog, searchWorkLogs } from './workLogApi.ts'
import { EMPTY_QUERY } from './workLogTypes.ts'
import type { WorkLog, WorkLogQuery } from './workLogTypes.ts'

export function WorkLogPage() {
  // 最后一次点「查询」时的条件。输入框里正在改、还没点查询的内容不在这里，在 QueryPanel 里
  const [criteria, setCriteria] = useState<WorkLogQuery>(EMPTY_QUERY)
  const [rows, setRows] = useState<WorkLog[]>([])
  // 页面一打开就要查询，所以初始值是 true
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [reloadKey, setReloadKey] = useState(0)
  const [deletingId, setDeletingId] = useState<number | null>(null)
  const [actionError, setActionError] = useState('')
  // 🟨 练习：删除成功的提示
  const [actionMessage, setActionMessage] = useState('')

  // criteria 变了就按新条件查询。组件第一次显示时也会执行一次，这就是「页面一打开就自动查询」
  useEffect(() => {
    // 连点两次「查询」时，先发的请求可能后回来。旧请求的结果用 ignore 丢掉（第 7C 章）
    let ignore = false

    async function load() {
      try {
        const data = await searchWorkLogs(criteria)
        if (!ignore) {
          setRows(data)
          setError('')
        }
      } catch (err) {
        if (!ignore) {
          setError(err instanceof Error ? err.message : String(err))
        }
      } finally {
        // finally：成功、失败都会执行，「查询中」在这里关掉
        if (!ignore) {
          setLoading(false)
        }
      }
    }

    load()
    return () => {
      ignore = true
    }
  }, [criteria, reloadKey])

  function handleSearch(next: WorkLogQuery) {
    setActionError('')
    // 🟨 练习：开始新的查询时，上一次的删除提示也清掉
    setActionMessage('')
    // 「查询中」在事件处理函数里打开，查询结果回来后在 load 里关掉
    setLoading(true)
    // next 每次都是新对象：条件和上次一模一样，React 也认为 criteria 变了，会再查一次（第 4D 章）
    setCriteria(next)
  }

  async function handleDelete(row: WorkLog) {
    // 浏览器自带的确认框。代码停在这一行，等用户点：「确定」返回 true，「取消」返回 false
    const ok = window.confirm(`确定删除这条记录吗？\n\n${row.hospitalName}，${row.workType}，${row.startDate} 至 ${row.endDate}`)
    if (!ok) {
      return
    }
    setActionError('')
    // 🟨 练习：开始新的删除时，清掉上一次的提示
    setActionMessage('')
    setDeletingId(row.id)
    try {
      await deleteWorkLog(row.id)
      // 🟨 练习：能走到这一行，说明 deleteWorkLog 没有抛错，删除成功了
      setActionMessage(`已删除：${row.hospitalName}，${row.workType}，${row.startDate} 至 ${row.endDate}`)
    } catch (err) {
      setActionError(`删除失败：${err instanceof Error ? err.message : String(err)}`)
    } finally {
      setDeletingId(null)
    }
    // 成功、失败都重新查询：失败可能是因为记录已经被别人删了，表格里那一行本来就该消失
    setLoading(true)
    setReloadKey((k) => k + 1)
  }

  return (
    <section>
      <QueryPanel onSearch={handleSearch} />
      <div className="panel">
        {/* 查询期间表格保留上一次的结果，只在标题上提示，避免表格闪一下变空 */}
        <h2>查询结果{loading ? '（查询中…）' : `（共 ${rows.length} 条）`}</h2>
        {actionError && <p className="error">{actionError}</p>}
        {/* 🟨 练习：删除成功的提示 */}
        {actionMessage && <p className="success">{actionMessage}</p>}
        {error ? (
          <p className="error">查询失败：{error}</p>
        ) : (
          <ResultTable rows={rows} deletingId={deletingId} onDelete={handleDelete} />
        )}
      </div>
    </section>
  )
}
