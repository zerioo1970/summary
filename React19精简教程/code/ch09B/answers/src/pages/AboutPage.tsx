// src/pages/AboutPage.tsx
// 练习 1 做完后的版本
import { Link } from 'react-router'

export function AboutPage() {
  return (
    <section>
      <h2>关于</h2>
      <p>本页面列出超声事业部的产品价格，仅供内部报价参考。</p>
      <p>
        {/* 本应用内部的页面用 Link，不重新加载 */}
        <Link to="/">查看产品清单</Link>
      </p>
      <p>
        {/* 本应用以外的网站用普通的 a。target="_blank" 在新标签页打开 */}
        <a href="https://www.example.com" target="_blank" rel="noreferrer">
          公司官网
        </a>
      </p>
    </section>
  )
}
