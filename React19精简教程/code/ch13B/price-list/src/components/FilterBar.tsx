// src/components/FilterBar.tsx
// 搜索框。它自己不存关键字：值从父组件传进来，用户输入时调用 onKeywordChange 通知父组件
type FilterBarProps = {
  keyword: string
  onKeywordChange: (keyword: string) => void
}

export function FilterBar({ keyword, onKeywordChange }: FilterBarProps) {
  return (
    <label className="filter-bar">
      搜索 <input value={keyword} placeholder="编号或名称" onChange={(e) => onKeywordChange(e.target.value)} />
    </label>
  )
}
