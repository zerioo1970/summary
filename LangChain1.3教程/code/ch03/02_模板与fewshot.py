"""第 3 章 - 提示模板（防注入）与 Few-shot 示例。

这个脚本不需要 API Key 也能跑前半部分 —— 只看模板展开成什么样。
"""

from langchain_core.prompts import ChatPromptTemplate, FewShotChatMessagePromptTemplate

# ══ 一、基本模板：把指令和用户数据分开放 ═══════════════════════
# 思路等同于 SQL 参数化查询：不要把用户输入拼进指令字符串
模板 = ChatPromptTemplate.from_messages([
    ("system", "你是{角色}，用{语言}回答，回答不超过两句话。"),
    ("human", "{问题}"),
])

消息列表 = 模板.format_messages(角色="资深 DBA", 语言="中文", 问题="索引是什么")

print("模板展开结果：")
for m in 消息列表:
    print(f"  [{type(m).__name__}] {m.text}")

# ⚠️ 坑 1：变量漏填会直接 KeyError（好事，不会静默出错）
try:
    模板.format_messages(角色="DBA")
except KeyError as e:
    print(f"\n⚠️ 漏填变量 -> KeyError: {e}")

# ⚠️ 坑 2：提示里想写大括号，要双写转义
转义模板 = ChatPromptTemplate.from_messages([
    ("human", '返回 JSON：{{"a":1}}，问题：{q}'),
])
print("\n大括号转义：", 转义模板.format_messages(q="测试")[0].text)


# ══ 二、Few-shot：给它看两个例子 ═════════════════════════════
例子 = [
    {"输入": "20*30 蓝色 加厚", "输出": "尺寸=20*30|颜色=蓝色|工艺=加厚"},
    {"输入": "直径50 不锈钢本色", "输出": "尺寸=直径50|颜色=不锈钢本色|工艺=无"},
]

# 每个例子长什么样：一问一答
例子模板 = ChatPromptTemplate.from_messages([
    ("human", "{输入}"),
    ("ai", "{输出}"),
])

少样本块 = FewShotChatMessagePromptTemplate(example_prompt=例子模板, examples=例子)

最终模板 = ChatPromptTemplate.from_messages([
    ("system", "你是产品规格整理员，把描述拆成 尺寸=xxx|颜色=xxx|工艺=xxx。只输出一行。"),
    少样本块,                    # ← 例子插在这里
    ("human", "{输入}"),
])

print("\nFew-shot 展开后的消息（例子被装成『假的历史对话』）：")
for m in 最终模板.format_messages(输入="15*25 红色 防水"):
    print(f"  [{type(m).__name__}] {m.text}")


# ══ 三、模板直接接模型（需要 API Key）══════════════════════════
if __name__ == "__main__":
    try:
        from dotenv import load_dotenv
        from langchain.chat_models import init_chat_model

        load_dotenv()
        模型 = init_chat_model("deepseek:deepseek-chat", temperature=0)

        链 = 最终模板 | 模型          # 用 | 把模板和模型串起来
        回复 = 链.invoke({"输入": "长100宽50 哑光黑 喷砂"})
        print("\n实际调用结果：", 回复.text)
    except Exception as e:
        print(f"\n（跳过实际调用：{type(e).__name__}。配好 .env 里的 Key 后可以看到真实结果）")
