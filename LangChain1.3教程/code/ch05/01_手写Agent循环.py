"""第 5 章 - 手写一个 30 行的极简 Agent 循环。

目的不是让你以后手写，而是让你彻底没有"黑箱感"。
写完这个脚本，你就知道 create_agent 内部到底在做什么。

Agent 的全部逻辑只有一个判断：模型这次返回里有没有 tool_calls？
  有 → 执行工具，结果塞回消息列表，再问一次模型
  没有 → 结束，这就是最终答案
"""

from dotenv import load_dotenv
from langchain.chat_models import init_chat_model
from langchain.messages import HumanMessage, ToolMessage
from langchain.tools import tool

load_dotenv()


@tool
def get_weather(city: str) -> str:
    """查询指定城市今天的天气。

    Args:
        city: 城市中文名，例如"上海"。
    """
    data = {"上海": "晴，28°C", "北京": "多云，24°C", "广州": "雷阵雨，31°C"}
    return data.get(city, f"暂无{city}的数据")


# ══════════════════════════════════════════════════
# 准备
# ══════════════════════════════════════════════════
tools = {t.name: t for t in [get_weather]}                  # 名字 → 工具对象
model = init_chat_model("deepseek:deepseek-chat", temperature=0)
model_with_tools = model.bind_tools(list(tools.values()))   # 把工具清单告诉模型

messages = [HumanMessage("上海和广州今天哪个更适合出门？")]

MAX_TURNS = 5        # ← 这就是 recursion_limit 的雏形

# ══════════════════════════════════════════════════
# 循环（这 20 行就是 Agent 的全部核心逻辑）
# ══════════════════════════════════════════════════
for turn in range(1, MAX_TURNS + 1):
    ai_msg = model_with_tools.invoke(messages)      # ① 问模型
    messages.append(ai_msg)

    if not ai_msg.tool_calls:                       # ② 没有工具调用 → 结束
        print(f"\n第 {turn} 轮：模型给出最终回答")
        print(f"💬 {ai_msg.text}")
        break

    for call in ai_msg.tool_calls:                  # ③ 有 → 逐个执行
        result = tools[call["name"]].invoke(call["args"])
        print(f"第 {turn} 轮：执行 {call['name']}({call['args']}) → {result}")

        # ④ ⚠️ tool_call_id 必须和 AIMessage.tool_calls 里的 id 对上，否则 API 报错
        messages.append(ToolMessage(content=str(result), tool_call_id=call["id"]))
else:
    print(f"\n⚠️ 跑了 {MAX_TURNS} 轮还没结束，强制停止")


# ══════════════════════════════════════════════════
# 复盘
# ══════════════════════════════════════════════════
print(f"\n--- 消息列表最终有 {len(messages)} 条 ---")
for i, m in enumerate(messages, 1):
    print(f"{i}. [{type(m).__name__}] {(m.text or '(无文本)')[:50]}")

total_in = sum(u["input_tokens"] for m in messages if (u := getattr(m, "usage_metadata", None)))
total_out = sum(u["output_tokens"] for m in messages if (u := getattr(m, "usage_metadata", None)))
print(f"\n共调用模型 {sum(1 for m in messages if getattr(m, 'usage_metadata', None))} 次，"
      f"入 {total_in} / 出 {total_out} token")

# 💡 对照 create_agent：
#    ① → model 节点   ② → 条件分支   ③ → tools 节点   ④ → 自动完成
#    MAX_TURNS → recursion_limit
#
#    create_agent 额外帮你做的：并行执行多个工具、参数校验兜底、中间件挂载点、
#    检查点持久化、人工审批中断、流式分通道事件、结构化输出……
