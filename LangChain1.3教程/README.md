# LangChain 1.3 新手实用教程

面向**有编程基础、但 LangChain / AI 应用开发零基础**的开发者。
只讲日常真用得上的东西，跳过学术性和低频内容。

## 环境与版本

| 项目 | 版本 |
|---|---|
| Python | 3.10+（1.x 已不支持 3.9） |
| `langchain` | 1.3.14 |
| `langchain-core` | **1.5.1**（核心包版本号比主包高，这是正常的） |
| `langgraph` | 1.2.9 |
| `langchain-deepseek` | 1.1.0 |
| 主线模型 | **DeepSeek**（`deepseek-chat`），换其他厂商只改一行 |

本教程所有代码的 API 用法、返回结构、报错信息都在上述真实环境中验证过。

## 目录

| 章节 | 内容 | 状态 |
|---|---|---|
| [第 1 章 认识 LangChain，并跑出第一个程序](第01章-认识LangChain并跑出第一个程序.md) | 环境安装、DeepSeek 接入、第一个 Agent、四个必踩报错 | ✅ 已完成 |
| [第 2 章 模型与消息](第02章-模型与消息.md) | `init_chat_model`、三个常用参数、四种消息、算钱、手动多轮 | ✅ 已完成 |
| [第 3 章 提示词怎么写](第03章-提示词怎么写.md) | 四段结构、好坏对比、模板防注入、Few-shot、三个禁忌 | ✅ 已完成 |
| 第 4 章 工具：给 AI 装上手脚 | `@tool`、工具三要素、错误处理、查数据库实战 | 📝 待写 |
| 第 5 章 Agent：`create_agent` | Agent 循环、六个常用参数、防死循环 | 📝 待写 |
| 第 6 章 结构化输出 | Pydantic、`response_format`、三种策略选型 | 📝 待写 |
| 第 7 章 流式输出 | `stream()`、FastAPI + SSE | 📝 待写 |
| 第 8 章 让 AI 记住对话 | `checkpointer`、`thread_id`、SQLite 持久化 | 📝 待写 |
| 第 9 章 中间件：只学这 6 个就够用 | 摘要、人工确认、脱敏、保险丝、重试 | 📝 待写 |
| 第 10 章 RAG：让 AI 读你自己的资料 | 切分、Embedding、Chroma、带引用回答 | 📝 待写 |
| 第 11 章 出问题了怎么查 | 按症状索引的排错手册 | 📝 待写 |
| 第 12 章 省钱与提速 | 算成本、提示缓存、模型分级 | 📝 待写 |
| 第 13 章 做成一个真正的服务 | FastAPI、Docker、上线检查清单 | 📝 待写 |
| 项目一 生活助理 / 项目二 PDF 知识库 | 两个完整项目 | 📝 待写 |

完整规划（含附录与取舍说明）见 [00-目录.md](00-目录.md)。

## 怎么用这份教程

### 1. 准备环境

```bash
mkdir langchain-learn && cd langchain-learn
uv venv --python 3.12
source .venv/bin/activate          # Windows: .venv\Scripts\activate
uv pip install langchain langchain-deepseek python-dotenv
```

### 2. 配好 Key

把 [`代码/.env.example`](代码/.env.example) 复制成 `.env`，填上你在
[platform.deepseek.com](https://platform.deepseek.com/) 申请的 Key。

⚠️ `.env` 必须进 `.gitignore`，绝不能提交到 Git。

### 3. 按顺序跑代码

```
代码/
├── .env.example
├── 第01章/
│   ├── 00_版本检查.py          ← 不需要 Key，先跑这个
│   ├── 01_连通性测试.py        ← 确认能跟模型说上话
│   └── 02_第一个Agent.py       ← 会自己查资料的程序
├── 第02章/
│   ├── 01_看清AIMessage.py     ← 返回对象里装了什么
│   ├── 02_算钱与批量.py        ← 花了多少钱 + batch 并发
│   └── 03_手动多轮对话.py      ← 理解"记忆"的本质
└── 第03章/
    ├── 01_好坏提示对比.py      ← 各跑 3 次看稳定性
    ├── 02_模板与fewshot.py     ← 前半部分不需要 Key
    └── 03_需求整理器.py        ← 可直接拿去用的小工具
```

## 学习节奏

| 阶段 | 章节 | 时长 | 产出 |
|---|---|---|---|
| 上手 | 1–3 | 3 小时 | 能跟模型对话，会写稳定的提示词 |
| 核心 | 4–7 | 5 小时 | 会调工具、出结构化数据、会流式 |
| 实用 | 8–10 | 4 小时 | 有记忆、有知识库 |
| 交付 | 11–13 | 3 小时 | 能排错、能上线 |
| 项目 | 项目一、二 | 6 小时 | 两个能演示的成品 |

🚦 **最短可用路径**：第 1、2、4、5、8 章 ≈ 4 小时，做出一个带工具和记忆的可用助手。

## 每章的固定结构

1. 生活化场景引入
2. 原理图（Mermaid）
3. 完整可运行代码 + 逐行中文注释
4. 运行结果拆解
5. ⚠️ 常见报错与排查
6. 动手练习（含参考答案）
7. 本章速查卡 + 一句话总结

## 关于代码的可靠性说明

- **API 用法、返回结构、对象属性、报错类型**：全部在真实的 1.3.14 环境中实跑验证过
- **模型输出的具体文字**：因为模型每次措辞不同，教程里标注为"示例输出"，你跑出来的内容会有差异但结构一致
- 遇到跑不通的代码，请对照对应章节的"常见报错"一节

## 参考资料

- [LangChain 官方文档](https://docs.langchain.com/oss/python/langchain/overview)
- [v1 迁移指南](https://docs.langchain.com/oss/python/migrate/langchain-v1)
- [更新日志](https://docs.langchain.com/oss/python/releases/changelog)
- [Python API Reference](https://reference.langchain.com/python/langchain/)

> 官方文档内容均经改写摘要，以符合内容授权要求。
