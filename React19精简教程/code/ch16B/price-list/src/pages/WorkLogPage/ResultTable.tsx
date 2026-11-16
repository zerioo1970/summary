// src/pages/WorkLogPage/ResultTable.tsx
// 中间的「查询结果」表格。只负责显示：数据从 props 来，自己没有 state
// 🟨 第 16B 章：点「删除」时不自己调接口，而是通知父组件，由父组件确认、删除、重新查询
import type { WorkLog } from './workLogTypes.ts'
import styles from './WorkLogPage.module.css'

type ResultTableProps = {
  rows: WorkLog[]
  // 🟨 第 16B 章：正在删除的那条记录的 id，没有在删除时是 null
  deletingId: number | null
  // 🟨 第 16B 章：传整条记录而不只是 id：父组件弹确认框时要显示医院、日期
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
          {/* 🟨 第 16B 章：多一列放删除按钮 */}
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
            {/* 🟨 第 16B 章：有一条正在删除时，所有删除按钮都禁用，一次只删一条；正在删的那条显示「删除中…」 */}
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
