// src/components/Badge.tsx
// 练习 1：children 声明成 string，组件里就能把它当字符串用（这里读 length）
type BadgeProps = {
  children: string
}

export function Badge({ children }: BadgeProps) {
  return (
    <span>
      {children}（{children.length} 字）
    </span>
  )
}
