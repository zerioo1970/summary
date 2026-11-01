// src/components/ProductForm.tsx
// 新增和编辑共用的表单。传了 initial 就是编辑：输入框先填好原来的值
import { useState } from 'react'
import type { ChangeEvent, SubmitEvent } from 'react'
import type { Product } from '../types.ts'
import { ApiError } from '../api.ts'
import type { ProductInput } from '../api.ts'

type ProductFormProps = {
  // 标题，新增时是「录入新产品」，编辑时由编辑页传「编辑产品」
  title?: string
  // 编辑时的原始数据。不传就是新增
  initial?: Product
  // 保存时把填好的内容交给父组件，父组件决定调哪个接口。返回 Promise：表单要等它完成，才知道成功还是失败
  onSubmit: (input: ProductInput) => Promise<Product>
}

export function ProductForm({ title = '录入新产品', initial, onSubmit }: ProductFormProps) {
  // useState 的参数只在组件第一次显示时使用。编辑时用原来的值，新增时为空。数字要转成字符串，输入框只认字符串
  const [code, setCode] = useState(initial?.code ?? '')
  const [name, setName] = useState(initial?.name ?? '')
  const [category, setCategory] = useState(initial?.category ?? '探头')
  const [price, setPrice] = useState(initial ? String(initial.price) : '')
  // 保存成功后显示的提示
  const [message, setMessage] = useState('')
  // 点过一次「保存」才显示错误，免得一打开页面就满屏红字
  const [triedSubmit, setTriedSubmit] = useState(false)
  // 正在等服务器响应。这期间禁用保存按钮，防止重复提交
  const [submitting, setSubmitting] = useState(false)
  // 服务器返回的失败原因
  const [submitError, setSubmitError] = useState('')
  // 服务器返回的逐个字段的错误，例如 { code: '编号 P-001 已存在' }
  const [serverErrors, setServerErrors] = useState<Record<string, string>>({})

  // 派生值：每次渲染都从 state 现算，不另存 state，所以永远和输入框一致
  const priceNumber = Number(price)
  const codeError = code.trim() === '' ? '请填写编号' : ''
  const nameError = name.trim() === '' ? '请填写名称' : ''
  // Number('abc') 是 NaN，NaN > 0 不成立，所以「不是数字」和「小于等于 0」一起拦住
  const priceError = price.trim() === '' ? '请填写单价' : priceNumber > 0 ? '' : '单价必须是大于 0 的数字'
  const isValid = codeError === '' && nameError === '' && priceError === ''

  // 每个字段最终显示哪条错误：前端检查到的优先，没有时显示服务器给的
  function errorOf(field: string, clientError: string): string {
    return triedSubmit && clientError ? clientError : (serverErrors[field] ?? '')
  }

  // 用户改了某个字段，就把服务器对这个字段的旧错误清掉：那条错误针对的是改之前的值
  function clearServerError(field: string) {
    setServerErrors((prev) => {
      const next = { ...prev }
      delete next[field]
      return next
    })
  }

  // 单独写成函数时，参数类型要写明：input 元素上的变化事件
  function handlePriceChange(e: ChangeEvent<HTMLInputElement>) {
    setPrice(e.target.value)
    clearServerError('price')
  }

  async function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    // 阻止浏览器默认的「提交表单 = 刷新整页」，改由我们自己处理
    e.preventDefault()
    setTriedSubmit(true)
    if (!isValid) {
      setMessage('')
      return
    }
    // 新增时库存为 0；编辑时这个表单不改库存，沿用原来的值
    const input: ProductInput = {
      code: code.trim(),
      name: name.trim(),
      category,
      price: priceNumber,
      stock: initial?.stock ?? 0,
    }
    setSubmitting(true)
    setSubmitError('')
    setServerErrors({})
    setMessage('')
    try {
      const saved = await onSubmit(input)
      setMessage(`已保存：${saved.name}（编号 ${saved.code}，id ${saved.id}）`)
      // 新增成功后清空，方便录下一个；编辑时保留，父组件会跳转走
      if (!initial) {
        setCode('')
        setName('')
        setPrice('')
        setTriedSubmit(false)
      }
    } catch (err) {
      if (err instanceof ApiError) {
        // instanceof 判断成立后，TypeScript 知道 err 有 fieldErrors
        setServerErrors(err.fieldErrors)
        setSubmitError(Object.keys(err.fieldErrors).length > 0 ? '请修改标红的项' : err.message)
      } else {
        // 网络断了等情况，fetch 抛出的是普通错误，没有 fieldErrors
        setSubmitError(err instanceof Error ? err.message : String(err))
      }
    } finally {
      // 不管成功还是失败，都要恢复按钮。写在 finally 里就不会漏
      setSubmitting(false)
    }
  }

  const codeMessage = errorOf('code', codeError)
  const nameMessage = errorOf('name', nameError)
  const priceMessage = errorOf('price', priceError)

  return (
    <form className="product-form" onSubmit={handleSubmit}>
      <h2>{title}</h2>
      {/* value 来自 state，onChange 把用户输入写回 state：输入框完全由 React 控制 */}
      <label>
        编号{' '}
        <input
          value={code}
          onChange={(e) => {
            setCode(e.target.value)
            clearServerError('code')
          }}
        />
      </label>
      {codeMessage && <span className="error">{codeMessage}</span>}
      <label>
        名称{' '}
        <input
          value={name}
          onChange={(e) => {
            setName(e.target.value)
            clearServerError('name')
          }}
        />
      </label>
      {nameMessage && <span className="error">{nameMessage}</span>}
      <label>
        分类{' '}
        <select value={category} onChange={(e) => setCategory(e.target.value)}>
          <option value="探头">探头</option>
          <option value="主机">主机</option>
          <option value="耗材">耗材</option>
        </select>
      </label>
      <label>单价 <input value={price} onChange={handlePriceChange} /></label>
      {priceMessage && <span className="error">{priceMessage}</span>}
      {/* type="submit"：点击或在输入框里按回车都会触发表单的 onSubmit */}
      <button type="submit" disabled={submitting}>
        {submitting ? '保存中…' : '保存'}
      </button>
      {message && <p className="success">{message}</p>}
      {submitError && <p className="error">保存失败：{submitError}</p>}
    </form>
  )
}
