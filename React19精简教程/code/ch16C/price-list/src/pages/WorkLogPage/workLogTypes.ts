// src/pages/WorkLogPage/workLogTypes.ts
// 工作记录页用到的类型。只有这个页面用，所以放在页面自己的文件夹里，不放进全书共用的 src/types.ts

// 工作种类只有三种。写成字面量联合类型，代码里写错一个字 TypeScript 就会报错
export type WorkType = '安装' | '维修' | '其它'

// 下拉框的选项从这个数组生成。标注成 WorkType[]，数组里也不能写错字
export const WORK_TYPES: WorkType[] = ['安装', '维修', '其它']

// 一条工作记录，字段和后端返回的 JSON 一一对应。日期是 'YYYY-MM-DD' 格式的字符串
export type WorkLog = {
  id: number
  startDate: string
  endDate: string
  workType: WorkType
  hospitalName: string
}

// 🟨 第 16C 章：新增时提交的内容：WorkLog 去掉 id。id 由服务器分配
export type WorkLogInput = Omit<WorkLog, 'id'>

// 查询条件。每一项都是字符串，空字符串表示「不限」。工作种类多一个 ''，代表下拉框里的「全部」
export type WorkLogQuery = {
  start: string
  end: string
  workType: WorkType | ''
  hospital: string
}

// 四个条件都不限。页面刚打开时用它查询，所以一打开就列出全部记录
export const EMPTY_QUERY: WorkLogQuery = { start: '', end: '', workType: '', hospital: '' }
