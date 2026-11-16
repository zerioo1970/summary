// src/pages/WorkLogPage/EntryForm.tsx
// 下面的「录入工作记录」panel。自己负责校验和调用新增接口，保存成功后通知父组件重新查询
import { useState } from 'react'
import type { SubmitEvent } from 'react'
import { ApiError } from '../../api.ts'
import { createWorkLog } from './workLogApi.ts'
import { WORK_TYPES } from './workLogTypes.ts'
import type { WorkLog, WorkLogInput, WorkType } from './workLogTypes.ts'

type EntryFormProps = {
  // 保存成功后调用，参数是服务器返回的那条记录（带 id）
  onSaved: (saved: WorkLog) => void
}

export function EntryForm({ onSaved }: EntryFormProps) {
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')
  // 录入时必须选一种，没有「全部」，默认「安装」
  const [workType, setWorkType] = useState<WorkType>('安装')
  const [hospitalName, setHospitalName] = useState('')
  // 点过一次「保存」才显示前端校验的错误，免得一打开页面就满屏错误提示（第 6C 章）
  const [triedSubmit, setTriedSubmit] = useState(false)
  // 正在等服务器响应。这期间禁用保存按钮，防止重复提交
  const [saving, setSaving] = useState(false)
  // 服务器返回的逐个字段的错误，例如 { hospitalName: '市中心医院 在 2026-10-08 已经有一条「安装」记录' }
  const [serverErrors, setServerErrors] = useState<Record<string, string>>({})
  // 表单下方的一句话：保存成功的提示，或者保存失败的原因
  const [message, setMessage] = useState('')
  const [messageIsError, setMessageIsError] = useState(false)

  // 前端校验，都是派生值：每次渲染从 state 现算，永远和输入框一致
  const name = hospitalName.trim()
  const startError = startDate === '' ? '请选择开始日期' : ''
  // 两个日期都是 'YYYY-MM-DD'，字符串比较就是日期先后比较
  const endError =
    endDate === '' ? '请选择结束日期' : startDate !== '' && endDate < startDate ? '结束日期不能早于开始日期' : ''
  const hospitalError = name === '' ? '请填写医院名称' : name.length > 50 ? '医院名称不能超过 50 个字' : ''
  const isValid = startError === '' && endError === '' && hospitalError === ''

  // 每个字段最终显示哪条错误：前端检查到的优先，没有时显示服务器给的（第 13C 章）
  function errorOf(field: string, clientError: string): string {
    return triedSubmit && clientError ? clientError : (serverErrors[field] ?? '')
  }

  // 改了某个字段，就清掉服务器对这个字段的旧错误：那条错误针对的是改之前的值
  function clearServerError(field: string) {
    setServerErrors((prev) => {
      const next = { ...prev }
      delete next[field]
      return next
    })
  }

  async function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault()
    setTriedSubmit(true)
    if (!isValid) {
      setMessage('')
      return
    }
    const input: WorkLogInput = { startDate, endDate, workType, hospitalName: name }
    setSaving(true)
    setServerErrors({})
    setMessage('')
    try {
      const saved = await createWorkLog(input)
      // 清空表单，方便接着录下一条。工作种类保留：连续录同一种工作时不用每次重选
      setStartDate('')
      setEndDate('')
      setHospitalName('')
      setTriedSubmit(false)
      setMessageIsError(false)
      setMessage(`已保存：${saved.hospitalName}，${saved.workType}，${saved.startDate} 至 ${saved.endDate}`)
      // 通知父组件：数据变了。父组件按生效的条件重新查询，表格才会更新
      onSaved(saved)
    } catch (err) {
      setMessageIsError(true)
      if (err instanceof ApiError) {
        setServerErrors(err.fieldErrors)
        setMessage(Object.keys(err.fieldErrors).length > 0 ? '保存失败，请修改标红的项' : `保存失败：${err.message}`)
      } else {
        // 网络断了等情况，fetch 抛出的是普通错误，没有 fieldErrors
        setMessage(`保存失败：${err instanceof Error ? err.message : String(err)}`)
      }
    } finally {
      setSaving(false)
    }
  }

  const startMessage = errorOf('startDate', startError)
  const endMessage = errorOf('endDate', endError)
  const hospitalMessage = errorOf('hospitalName', hospitalError)

  return (
    // 借用全局的 product-form 样式：每个 label 占一行，错误提示紧跟在下面
    <form className="panel product-form" onSubmit={handleSubmit}>
      <h2>录入工作记录</h2>
      <label>
        开始日期{' '}
        <input
          type="date"
          value={startDate}
          onChange={(e) => {
            setStartDate(e.target.value)
            clearServerError('startDate')
          }}
        />
      </label>
      {startMessage && <span className="error">{startMessage}</span>}
      <label>
        结束日期{' '}
        <input
          type="date"
          value={endDate}
          onChange={(e) => {
            setEndDate(e.target.value)
            clearServerError('endDate')
          }}
        />
      </label>
      {endMessage && <span className="error">{endMessage}</span>}
      <label>
        工作种类{' '}
        {/* 和查询条件的下拉框一样，要把 string 换成 WorkType；这里找不到时退回「安装」 */}
        <select
          value={workType}
          onChange={(e) => {
            setWorkType(WORK_TYPES.find((t) => t === e.target.value) ?? '安装')
            clearServerError('workType')
          }}
        >
          {WORK_TYPES.map((t) => (
            <option key={t} value={t}>
              {t}
            </option>
          ))}
        </select>
      </label>
      {serverErrors.workType && <span className="error">{serverErrors.workType}</span>}
      <label>
        医院名称{' '}
        <input
          value={hospitalName}
          onChange={(e) => {
            setHospitalName(e.target.value)
            clearServerError('hospitalName')
          }}
        />
      </label>
      {hospitalMessage && <span className="error">{hospitalMessage}</span>}
      <button type="submit" disabled={saving}>
        {saving ? '保存中…' : '保存'}
      </button>
      {message && <p className={messageIsError ? 'error' : 'success'}>{message}</p>}
    </form>
  )
}
