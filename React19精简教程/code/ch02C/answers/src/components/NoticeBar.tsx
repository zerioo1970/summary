// src/components/NoticeBar.tsx
// 练习 1：从 App.tsx 搬过来，前面加 export 才能被 App.tsx 导入
export function NoticeBar() {
  const validUntil = '2026-12-31'
  return (
    <>
      <hr />
      <p>价格含税，有效期至 {validUntil}</p>
    </>
  )
}
