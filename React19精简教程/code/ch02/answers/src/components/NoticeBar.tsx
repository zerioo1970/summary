// src/components/NoticeBar.tsx
// 公告栏：返回两个并列元素，用 Fragment <>...</> 包起来，不多出一层 div
export function NoticeBar() {
  const validUntil = '2026-12-31'
  return (
    <>
      <hr />
      <p>价格含税，有效期至 {validUntil}</p>
    </>
  )
}
