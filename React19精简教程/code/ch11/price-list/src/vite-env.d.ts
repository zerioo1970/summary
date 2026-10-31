// src/vite-env.d.ts
// 给 import.meta.env 补上类型：.env 里加了变量，就在这里声明一行

// 打开严格模式：没在下面声明的变量名，TypeScript 直接报错（不打开时，写错名字只会得到 undefined）
interface ViteTypeOptions {
  strictImportMetaEnv: unknown
}

interface ImportMetaEnv {
  // 接口地址前缀，例如 /api
  readonly VITE_API_BASE: string
}
