// src/pages/WorkLogPage/QueryPanel.tsx
// 练习 1 做完后的版本，🟨 标出练习改动的地方
// 上面的「查询条件」panel。四个输入框的内容是它自己的 state（还没生效的草稿），
// 点「查询」时才把条件交给父组件，父组件据此查询
import { useState } from 'react'
import type { SubmitEvent } from 'react'
// 🟨 练习 1：多导入 EMPTY_QUERY
import { EMPTY_QUERY, WORK_TYPES } from './workLogTypes.ts'
import type { WorkLogQuery, WorkType } from './workLogTypes.ts'
import styles from './WorkLogPage.module.css'

type QueryPanelProps = {
  // 点「查询」时调用，参数是这一刻四个输入框的内容
  onSearch: (query: WorkLogQuery) => void
}

export function QueryPanel({ onSearch }: QueryPanelProps) {
  // 日期输入框的 value 是 'YYYY-MM-DD' 或 ''（没选），所以初始值用 ''
  const [start, setStart] = useState('')
  const [end, setEnd] = useState('')
  // '' 代表「全部」
  const [workType, setWorkType] = useState<WorkType | ''>('')
  const [hospital, setHospital] = useState('')
  const [error, setError] = useState('')

  // 用 form 的 onSubmit：点「查询」按钮、在医院名称框里按回车，都会查询
  function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault()
    // 两个日期字符串格式相同，可以直接比较先后。后端也会检查，这里先挡一下，不用等请求回来
    if (start !== '' && end !== '' && start > end) {
      setError('开始日期不能晚于结束日期')
      return
    }
    setError('')
    onSearch({ start, end, workType, hospital })
  }

  // 🟨 练习 1：清空四个输入框和提示，并马上按「不限」查询
  function handleReset() {
    setStart('')
    setEnd('')
    setWorkType('')
    setHospital('')
    setError('')
    // 必须复制一份新对象。直接传 EMPTY_QUERY 时，如果生效的条件本来就是它，
    // React 认为 criteria 没变，跳过更新，不会查询，「查询中…」也就一直关不掉
    onSearch({ ...EMPTY_QUERY })
  }

  return (
    <form className={`panel ${styles.fields}`} onSubmit={handleSubmit}>
      <h2>查询条件</h2>
      {/* type="date"：浏览器显示日期选择器，value 永远是 'YYYY-MM-DD'，和界面上显示成什么样无关 */}
      <label>
        开始日期 <input type="date" value={start} onChange={(e) => setStart(e.target.value)} />
      </label>
      <label>
        结束日期 <input type="date" value={end} onChange={(e) => setEnd(e.target.value)} />
      </label>
      <label>
        工作种类{' '}
        {/* e.target.value 是 string，要换成 WorkType | '' 才能存进 state：在选项里找到它，找不到就当「全部」 */}
        <select value={workType} onChange={(e) => setWorkType(WORK_TYPES.find((t) => t === e.target.value) ?? '')}>
          <option value="">全部</option>
          {WORK_TYPES.map((t) => (
            <option key={t} value={t}>
              {t}
            </option>
          ))}
        </select>
      </label>
      <label>
        医院名称 <input value={hospital} onChange={(e) => setHospital(e.target.value)} placeholder="可以只填一部分" />
      </label>
      <button type="submit">查询</button>{' '}
      {/* 🟨 练习 1：type="button"，点它不会触发表单提交 */}
      <button type="button" onClick={handleReset}>
        重置
      </button>
      {error && <span className="error">{error}</span>}
    </form>
  )
}
