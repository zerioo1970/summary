// src/components/ChildrenTypes.tsx（3D.3 版本：children 声明成不同类型，各能放什么）
// 这个文件只用来让 npx tsc -b 检查，不放到页面上
import type { PropsWithChildren, ReactElement, ReactNode } from 'react'

// ① ReactNode：能显示的东西都行
function AnyBox({ children }: { children: ReactNode }) {
  return <div>{children}</div>
}

// ② string：只能放文字。好处是组件里能把它当字符串用
function UpperText({ children }: { children: string }) {
  return <b>{children.toUpperCase()}</b>
}

// ③ ReactElement：只能放一个 JSX 元素
function OneElement({ children }: { children: ReactElement }) {
  return <div>{children}</div>
}

// ④ PropsWithChildren：React 自带的写法，自动加上可选的 children?: ReactNode
function Section({ title, children }: PropsWithChildren<{ title: string }>) {
  return (
    <section>
      <h2>{title}</h2>
      {children}
    </section>
  )
}

export function ChildrenTypesDemo() {
  return (
    <>
      <AnyBox>文字</AnyBox>
      <AnyBox>
        <i>一个元素</i>
        <i>两个元素</i>
      </AnyBox>
      <UpperText>abc</UpperText>
      <OneElement>
        <i>一个元素</i>
      </OneElement>
      <Section title="说明" />
    </>
  )
}
