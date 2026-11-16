// src/pages/WorkLogPage/WorkLogPage.tsx
// 工作记录页：上、中、下三个 panel 的父组件。
// 「生效的查询条件」和「表格数据」放在这里：第 16B、16C 章的删除和录入也要让表格重新查询
import { useEffect, useState } from 'react'
import { QueryPanel } from './QueryPanel.tsx'
// 🟨 第 16C 章：下面的录入表单
import { EntryForm } from './EntryForm.tsx'
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
    setDeletingId(row.id)
    try {
      await deleteWorkLog(row.id)
    } catch (err) {
      setActionError(`删除失败：${err instanceof Error ? err.message : String(err)}`)
    } finally {
      setDeletingId(null)
    }
    // 成功、失败都重新查询：失败可能是因为记录已经被别人删了，表格里那一行本来就该消失
    setLoading(true)
    setReloadKey((k) => k + 1)
  }

  // 🟨 第 16C 章：录入表单保存成功后调用。和删除一样，按生效的条件重新查询
  function handleSaved() {
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
        {error ? (
          <p className="error">查询失败：{error}</p>
        ) : (
          <ResultTable rows={rows} deletingId={deletingId} onDelete={handleDelete} />
        )}
      </div>
      {/* 🟨 第 16C 章：下面的 panel */}
      <EntryForm onSaved={handleSaved} />
    </section>
  )
}
