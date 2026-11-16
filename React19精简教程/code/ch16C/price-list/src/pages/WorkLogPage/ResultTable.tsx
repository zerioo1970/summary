// src/pages/WorkLogPage/ResultTable.tsx
// 中间的「查询结果」表格。只负责显示：数据从 props 来，自己没有 state
import type { WorkLog } from './workLogTypes.ts'
import styles from './WorkLogPage.module.css'

type ResultTableProps = {
  rows: WorkLog[]
  deletingId: number | null
  onDelete: (row: WorkLog) => void
}

export function ResultTable({ rows, deletingId, onDelete }: ResultTableProps) {
  if (rows.length === 0) {
    return <p className="empty">没有符合条件的记录</p>
  }

  return (
    <table className={styles.table}>
      <thead>
        <tr>
          <th>开始日期</th>
          <th>结束日期</th>
          <th>工作种类</th>
          <th>医院名称</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        {/* 表格里不显示 id，但 key 要用它：只有 id 能唯一区分一条记录 */}
        {rows.map((w) => (
          <tr key={w.id}>
            <td>{w.startDate}</td>
            <td>{w.endDate}</td>
            <td>{w.workType}</td>
            <td>{w.hospitalName}</td>
            <td>
              <button type="button" disabled={deletingId !== null} onClick={() => onDelete(w)}>
                {deletingId === w.id ? '删除中…' : '删除'}
              </button>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
