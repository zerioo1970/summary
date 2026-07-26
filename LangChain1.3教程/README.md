# LangChain 1.3 新手实用教程

> 版本基线：`langchain 1.3.x` + `langchain-core 1.5.x` + `langgraph 1.2.x`｜Python 3.10+
> 主线模型：**DeepSeek**（`langchain-deepseek`），换 OpenAI / Ollama 只改一行
>
> 每章固定七件套：🖼 图解 · 💻 完整可运行代码 · 🔍 拆解运行结果 · 🧱 C#/SQL 类比 · ⚠️ 踩坑警告 · 🎯 动手练习 · 📌 一句话总结

---

## 这本教程写给谁

- 会写代码（任何语言），但没系统用过 LangChain。
- 或者用过 0.x / LCEL，被 1.x 的改动搞懵了。
- 想要的是「能跑起来的东西」，不是概念大全。

**最短路径**：第 1、2、4、5、8 章 ≈ 4 小时，做出一个带工具和记忆的可用助手。
**完整路径**：13 章 + 2 项目 + 5 附录，每天 2 小时约两周。

---

## 进度

| 章节 | 状态 |
| --- | --- |
| 第 1 章 认识 LangChain，并跑出第一个程序 | ✅ |
| 第 2 章 模型与消息 | ✅ |
| 第 3 章 提示词怎么写 | ✅ |
| 第 4 章 工具：给 AI 装上手脚 | ✅ |
| 第 5 章 Agent：`create_agent` | ✅ |
| **第 6 章 结构化输出：拿到能直接入库的数据** | ✅ |
| 第 7 章 流式输出 | 📝 |
| 第 8–13 章 / 两个项目 / 附录 | 📝 |

---

## 目录

### 第一部分 上手（3 章，约 3 小时）

**[第 1 章 认识 LangChain，并跑出第一个程序](docs/ch01-hello-langchain.md)**
- 1.1 LangChain 是什么、不是什么（一页说完）
- 1.2 装环境：Python 3.10+、uv、`.env` 存 Key
- 1.3 模型三选一：DeepSeek（国内直连）/ OpenAI / Ollama（离线）
- 1.4 20 行代码：一个会查天气的 Agent
- 1.5 把返回结果打印出来看懂
- ⚠️ 新手四个必踩报错（模块找不到 / Key 没生效 / 模型名无效 / 代理超时）

**[第 2 章 模型与消息](docs/ch02-models-and-messages.md)**
- 2.1 `init_chat_model()` 一句话换模型
- 2.2 三个真正常用的参数：`temperature`、`max_tokens`、`timeout`
- 2.3 四种消息：System / Human / AI / Tool
- 2.4 取文字用 `.text`（⚠️ 现在是属性，别加括号）
- 2.5 用 `usage_metadata` 看这次花了多少 token
- 2.6 多轮对话：先手动传历史（第 8 章再自动化）

**[第 3 章 提示词怎么写](docs/ch03-prompts.md)**
- 3.1 系统提示的四段结构：角色、任务、约束、输出格式
- 3.2 好坏提示的直接对比
- 3.3 模板与变量替换（别用字符串拼接）
- 3.4 给几个例子（Few-shot）
- ⚠️ 三个最常犯的错误

### 第二部分 让 AI 会干活（4 章，约 5 小时 ← 全书重点）

**[第 4 章 工具：给 AI 装上手脚](docs/ch04-tools.md)**
- 4.1 `@tool` 装饰器（两个导入路径其实是同一个东西）
- 4.2 ⚠️ 函数名 + 类型标注 + docstring 就是给模型看的说明书
- 4.3 🖼 图解：模型只是「申请」调用，真正执行的是你的代码
- 4.4 ⚠️ 工具里报错了怎么办 —— **1.x 里默认会让 Agent 崩掉**，三种处理方案
- 4.5 让工具读到 `user_id` 但不让模型看见（`ToolRuntime` + `context_schema`）
- 4.6 实战：天气、汇率、查数据库（只读 + 参数化 + 行数上限三道保险）
- ⚠️ 工具太多时模型会选错，四个解法

**[第 5 章 Agent：`create_agent`](docs/ch05-agents.md)**
- 5.1 🖼 Agent 循环图 + **手写一个 30 行的极简循环**（写完就没有黑箱感了）
- 5.2 六个日常够用的参数：`model` `tools` `system_prompt` `checkpointer` `response_format` `middleware`
- 5.3 查看它中间调了哪些工具（四种方法 + 成本统计）
- 5.4 防止它自己转圈烧钱：`recursion_limit` + 两个限额中间件
- ⚠️ 三个新手会撞的限制（**实测结果和官方文档不完全一致，如实写出**）
- 5.5 实战：旅行规划助手

**[第 6 章 结构化输出：拿到能直接入库的数据](docs/ch06-structured-output.md)**
- 6.1 为什么「请只返回 JSON」不靠谱（五种真实翻车方式）
- 6.2 用 Pydantic 定义你要的形状（⚠️ 类名/docstring/Field 描述都是给模型看的）
- 6.3 `response_format` 一行搞定 —— 结果在 `result["structured_response"]`
- 6.4 三种策略，一句话选型（**实测：DeepSeek 走 `ToolStrategy`，OpenAI 走 `ProviderStrategy`**）
- ⚠️ 提示式 JSON 输出在 1.x 已被移除，旧教程写法会失效
- 6.5 校验失败怎么兜：`handle_errors` 四种取值实测
- 6.6 实战：把投诉邮件批量变成工单记录（含成本估算）

**第 7 章 流式输出：别让用户看着空白屏幕**
- 7.1 `stream()` 做打字机效果（最简写法）
- 7.2 只要文字、不要杂七杂八
- 7.3 事件的那种写法
- 7.4 工具调用过程也想显示出来
- 7.5 实战：FastAPI + SSE 推到网页（含 30 行前端 JS）
- ⚠️ 流式常见两个坑

### 第三部分 记得住、查得到（3 章，约 4 小时）

**第 8 章 让 AI 记住对话**
- 8.1 🖼 短期记忆 vs 长期记忆
- 8.2 `checkpointer` + `thread_id`：三行代码实现多轮记忆
- 8.3 存进 SQLite，重启不丢
- 8.4 多用户会话隔离
- 8.5 历史太长怎么办：交给摘要中间件自动处理
- 8.6 跨会话记住用户偏好（`store` 最简用法）

**第 9 章 中间件：只学这 6 个就够用**
- 9.1 🖼 一张图看清它挂在哪
- 9.2 `SummarizationMiddleware` —— 对话太长自动摘要
- 9.3 `HumanInTheLoopMiddleware` —— 危险操作先人工确认
- 9.4 `PIIMiddleware` —— 手机号身份证发出去之前脱敏
- 9.5 `ModelCallLimitMiddleware` + `ToolCallLimitMiddleware` —— 预算和死循环的保险丝
- 9.6 `ModelRetryMiddleware` —— 限流/超时自动重试
- 9.7 自己写一个：给每次调用打日志（30 行）
- 9.8 需要动态改提示词时：`@dynamic_prompt`

**第 10 章 RAG：让 AI 读你自己的资料**
- 10.1 🖼 原理图：切分 → 向量化 → 入库 → 检索 → 回答（用「开卷考试」理解）
- 10.2 加载 PDF / Word / Markdown
- 10.3 切分：`chunk_size` 和重叠怎么定，中文要注意什么
- 10.4 Embedding + Chroma 建库（完整脚本）
- 10.5 两种用法：固定先检索，或把检索做成工具让 Agent 自己决定
- 10.6 回答里带来源引用
- ⚠️ 「明明有资料却搜不到」排查五步
- ⚠️ 高级检索器（MultiQuery / 压缩 / ParentDocument）已移到 `langchain-classic`，import 会报错
- 10.7 实战：公司制度问答助手

### 第四部分 能交付出去（3 章，约 3 小时）

**第 11 章 出问题了怎么查（症状式手册）**
- 按症状索引：工具不被调用 / 工具被反复调用 / Agent 不肯停 / 结构化输出校验失败 / 对话串台 / Token 超限 / 中文乱码 / 流式不完整 / 旧教程代码跑不了 / ImportError / 太慢太贵
- 附：LangSmith 5 分钟接上，看清每一步（以及不想用它时的日志方案）

**第 12 章 省钱与提速**
- 12.1 算清一次请求花了多少钱
- 12.2 提示缓存（Prompt Caching）—— 降本最大的一招
- 12.3 精简工具描述能省多少（官方案例：输入 token 降约 65%）
- 12.4 便宜模型打头阵、失败再上贵的
- 12.5 控制历史长度、加结果缓存

**第 13 章 做成一个真正的服务**
- 13.1 推荐的项目目录结构
- 13.2 FastAPI 包一层：普通接口 + 流式接口 + 会话管理
- 13.3 密钥与环境隔离
- 13.4 Docker 打包
- ✓ 上线前 12 项检查清单
- 13.5 安全三件事：提示注入、工具越权、日志脱敏

### 第五部分 两个完整项目

**项目一 会用工具、记得住事的生活助理**
需求 → 三个工具 → 建 Agent → 加记忆 → 加流式 → 加保险丝 → 完整代码讲解

**项目二 带引用的 PDF 知识库问答**
上传解析 → 中文切分 → 建索引 → 检索 → 带来源回答 → 不知道就说不知道 → 简单网页界面 → 完整代码讲解

### 附录

- **A** 一页 API 速查表（日常够用的那些）
- **B** 模型接入速查（DeepSeek / OpenAI / 通义 / Kimi / 智谱 / Ollama 的写法）
- **C** 报错速查表
- **D** 旧教程代码怎么改：0.x → 1.3 最少必知 8 条
- **E** 以后想深入再看：LangGraph 自定义图、多 Agent、Deep Agents、MCP、LCEL、评估体系（每项两句话说明「什么时候你才需要它」）

---

## 配套代码

章内代码都可以直接复制运行，也可以用 `code/` 目录下的现成脚本：

```
code/
├── .env.example              ← 复制成 .env 并填 Key
├── ch01/
│   ├── check_versions.py     ← 不需要 Key，先跑这个确认环境
│   ├── smoke_test.py         ← 确认能跟模型说上话
│   └── hello_agent.py        ← 第一个会查天气的 Agent
├── ch02/
│   ├── model_basics.py       ← AIMessage 里到底装了什么
│   ├── token_count.py        ← 算钱 + batch 批量并发
│   └── manual_memory.py      ← 手动多轮，理解"记忆"的本质
├── ch03/
│   ├── prompt_compare.py     ← 好坏提示各跑 3 次看稳定性
│   ├── prompt_template.py    ← 模板防注入 + Few-shot（前半段不需要 Key）
│   └── prompt_structure.py   ← 四段结构实战：需求整理器
├── ch04/
│   ├── tool_basics.py        ← 不需要 Key，看模型到底能"看到"什么
│   ├── tool_errors.py        ← 工具报错的三种处理方案
│   └── real_tools.py         ← 权限隔离 + 查库三道保险（最重要的一个）
├── ch05/
│   ├── manual_agent.py       ← 手写 30 行极简 Agent 循环
│   ├── inspect_agent.py      ← 调试函数，可直接抄进你的项目
│   └── travel_agent.py       ← 综合实战：旅行规划助手
└── ch06/
    ├── extract_basic.py      ← 结构化输出最小示例
    ├── strategies.py         ← 不需要 Key，看框架给模型的"人造工具"长什么样
    └── extract_ticket.py     ← 实战：邮件批量转工单（可直接改造后用于生产）
```

准备环境：

```bash
uv venv --python 3.12
source .venv/bin/activate        # Windows: .venv\Scripts\activate
uv pip install langchain langchain-deepseek python-dotenv
cp code/.env.example code/.env   # 然后填上你的 DEEPSEEK_API_KEY
```

⚠️ `.env` 必须进 `.gitignore`，绝不能提交到 Git。

---

## 关于版本

LangChain 1.x 是一次不兼容的大改版。本教程所有代码按 1.3.x 编写，具体来说：

| 你在旧教程里看到的 | 1.3 里的写法 |
| --- | --- |
| `from langchain.agents import AgentExecutor, create_react_agent` | `from langchain.agents import create_agent` |
| `LLMChain` / `ConversationChain` | `create_agent` 或直接 `model.invoke()` |
| `ConversationBufferMemory` | `checkpointer` + `thread_id` |
| `response.content` 拿文本 | `response.text`（属性，不加括号） |
| `config["configurable"]` 传用户身份 | `context_schema=` + `invoke(context=...)` |
| 工具异常自动兜底（`handle_tool_errors`） | 显式处理：工具内 try/except 或 `ToolErrorMiddleware` |
| 流式事件节点名 `"agent"` | `"model"` |
| `langchain.retrievers.MultiQueryRetriever` | 已移到 `langchain-classic` |

完整迁移清单见附录 D。

---

## 关于代码可靠性

- **API 用法、返回结构、对象属性、报错原文**：在真实的 `langchain 1.3.14` 环境中实跑验证过
- **模型输出的具体文字**：模型每次措辞不同，教程里标注为"示例输出"，你跑出来的内容会有差异但结构一致
- **和官方文档不一致的地方会明确标出**（例如第 5 章那三个"限制"，实测有两条并不会报错——教程会告诉你实际情况，同时建议仍按官方推荐写法）

---

## 版权与来源

本教程为原创教学内容。涉及的框架行为以 [LangChain 官方文档](https://docs.langchain.com/oss/python/langchain/overview) 与 [API 参考](https://reference.langchain.com/python/langchain/) 为准；官方描述经改写以符合授权要求。
