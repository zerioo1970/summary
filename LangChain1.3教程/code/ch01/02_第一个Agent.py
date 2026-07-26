"""第 1 章 - 第一个 Agent：会自己决定要不要查资料的程序。

重点看：你没有写任何"什么时候该查天气"的判断逻辑，是模型自己决定的。
"""

from dotenv import load_dotenv
from langchain.agents import create_agent
from langchain.tools import tool

load_dotenv()


# ── 第一步：写一个工具。它就是个普通的 Python 函数 ────────────────
@tool
def 查天气(城市: str) -> str:
    """查询指定城市今天的天气情况。城市名用中文，例如：上海。"""
    # 演示用假数据。真实项目里这里就是调你公司的 API 或查数据库
    天气表 = {
        "上海": "晴，28℃，东南风 3 级",
        "北京": "多云，24℃，空气质量良",
        "广州": "雷阵雨，31℃，注意带伞",
    }
    return 天气表.get(城市, f"抱歉，没有 {城市} 的天气数据")


# ── 第二步：把模型和工具装成一个 Agent ──────────────────────────
助手 = create_agent(
    model="deepseek:deepseek-chat",
    tools=[查天气],
    system_prompt="你是一个简洁的天气助手，回答不超过两句话。",
)

# ── 第三步：提问。注意入参是带 messages 的字典，不是字符串 ────────
结果 = 助手.invoke({"messages": [{"role": "user", "content": "广州今天要带伞吗？"}]})

# ── 第四步：最后一条消息就是最终答案 ───────────────────────────
print("=" * 50)
print("最终回答：", 结果["messages"][-1].text)
print("=" * 50)

# ── 加餐：把中间全过程打印出来，看清 Agent 是怎么工作的 ──────────
print("\n完整对话过程：")
for i, 消息 in enumerate(结果["messages"], 1):
    print(f"{i}. [{type(消息).__name__}] {消息.text}")
    # AIMessage 申请调用工具时，文字是空的，调用信息在 tool_calls 里
    if getattr(消息, "tool_calls", None):
        for 调用 in 消息.tool_calls:
            print(f"    ↳ 申请调用 {调用['name']}，参数 {调用['args']}")

# 💡 你会看到 4 条消息，而不是 5 条 ——
#    system_prompt 每次都会发给模型，但不占用对话历史的位置
