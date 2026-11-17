// src/components/NodeDemo.tsx
// 练习 2：在 3D.2 的基础上多放四个值，先猜，再看页面
export function NodeDemo() {
  return (
    <ul>
      <li>文字：{'价格含税'}</li>
      <li>数字：{12800}</li>
      <li>零：{0}</li>
      <li>JSX：{<b>主机</b>}</li>
      <li>数组：{['探头', '、', '耗材']}</li>
      <li>true：{true}</li>
      <li>false：{false}</li>
      <li>null：{null}</li>
      <li>undefined：{undefined}</li>
      {/* 练习 2 */}
      <li>空字符串：{''}</li>
      <li>字符串 0：{'0'}</li>
      <li>NaN：{NaN}</li>
      <li>空数组：{[]}</li>
    </ul>
  )
}
