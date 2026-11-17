// src/components/NodeDemo.tsx（3D.2 版本：把各种值放进 {}，看页面上显示什么）
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
    </ul>
  )
}
