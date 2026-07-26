"""第 3 章 - 综合实战：把业务人员口述的模糊需求，整理成开发能直接动手的任务单。

这是一个可以直接拿去用的小工具。重点看它的"待确认问题"部分 ——
那里列出来的，正是业务人员没说清、而开发必须问明白的东西。
"""

from dotenv import load_dotenv
from langchain.chat_models import init_chat_model
from langchain_core.prompts import ChatPromptTemplate

load_dotenv()
模型 = init_chat_model("deepseek:deepseek-chat", temperature=0, max_tokens=2000)

模板 = ChatPromptTemplate.from_messages([
    ("system", """【角色】
你是一名资深需求分析师，服务对象是企业内部管理系统的开发团队。

【任务】
把业务人员口述的模糊需求，整理成开发能直接动手的任务单。

【约束】
- 只整理用户说过的内容，不要自行扩展功能范围
- 用户没说清的地方，放进「待确认问题」，不要自己假设
- 不要写技术选型和实现方案，只写需求本身

【输出格式】
## 功能概述
（两三句话）

## 字段清单
| 字段名 | 类型 | 必填 | 说明 |
|---|---|---|---|

## 业务规则
1.

## 待确认问题
1.

## 验收标准
- [ ]
"""),
    ("human", "业务人员说：{原始需求}"),
])

链 = 模板 | 模型

原始需求 = """
仓库那边说想要个页面，能看到每个物料现在还有多少库存，
低于安全库存的要标红，最好能导出。对了，只能看自己部门管的物料。
"""

回复 = 链.invoke({"原始需求": 原始需求})
print(回复.text)

用量 = 回复.usage_metadata
print(f"\n{'-' * 50}")
print(f"花费约 {(用量['input_tokens'] * 2 + 用量['output_tokens'] * 8) / 1_000_000:.6f} 元")
