"""第 5 章 - 实战：旅行规划助手（第 4、5 章综合）。

演示要点：
1. 三个工具协作（天气 / 汇率 / 预算）
2. 用户偏好走 context，模型看不到也改不了
3. 工具错误分级处理（预料到的兜住，代码 bug 照旧抛）
4. 三道保险防转圈烧钱
5. 打印完整决策过程和成本
"""

from dataclasses import dataclass

from dotenv import load_dotenv
from langchain.agents import create_agent
from langchain.agents.middleware import (
    ModelCallLimitMiddleware,
    ToolCallLimitMiddleware,
    ToolErrorMiddleware,
)
from langchain.chat_models import init_chat_model
from langchain.tools import ToolRuntime, tool
from langgraph.errors import GraphRecursionError

load_dotenv()


# ══════════════════════════════════════════════════
# 运行时上下文：来自用户账户设置
# ══════════════════════════════════════════════════
@dataclass
class UserCtx:
    user_id: str
    home_currency: str = "CNY"
    budget_level: str = "标准"      # 经济 / 标准 / 舒适


# ══════════════════════════════════════════════════
# 工具 1：天气
# ══════════════════════════════════════════════════
@tool
def get_weather(city: str) -> str:
    """查询指定城市未来三天的天气概况。

    Args:
        city: 城市中文名，例如"东京""曼谷"。
    """
    data = {
        "东京": "晴到多云，18~24°C，适合出行",
        "曼谷": "雷阵雨为主，28~34°C，湿热，需带伞",
        "首尔": "多云，15~21°C，早晚偏凉",
    }
    if city not in data:
        # 明确区分"不支持"和"故障"，模型才知道该怎么回复用户
        return f"暂无{city}的天气数据，当前支持：{', '.join(data)}"
    return f"{city}未来三天：{data[city]}"


# ══════════════════════════════════════════════════
# 工具 2：汇率
# ══════════════════════════════════════════════════
@tool
def get_exchange_rate(currency_code: str) -> str:
    """查询指定外币兑人民币的汇率。

    Args:
        currency_code: 三位货币代码，大写，例如 JPY、THB、KRW。
    """
    rates = {"JPY": 0.047, "THB": 0.20, "KRW": 0.0053, "USD": 7.18}
    code = currency_code.strip().upper()
    if code not in rates:
        return f"不支持 {code}，当前支持：{', '.join(rates)}"
    return f"1 {code} = {rates[code]} CNY"


# ══════════════════════════════════════════════════
# 工具 3：预算估算 —— 算数交给 Python，不交给模型
# ══════════════════════════════════════════════════
DAILY_COST_CNY = {"经济": 400, "标准": 800, "舒适": 1600}


@tool
def estimate_budget(days: int, city: str, runtime: ToolRuntime[UserCtx]) -> str:
    """按【当前用户的预算档位】估算旅行总花费。

    Args:
        days: 出行天数，1~30。
        city: 目的地城市中文名。
    """
    if not 1 <= days <= 30:
        return "出行天数需在 1~30 之间，请让用户确认。"

    level = runtime.context.budget_level        # ← 来自账户设置，模型无法指定
    daily = DAILY_COST_CNY[level]
    total = daily * days                        # ⚠️ 乘法交给 Python，别让模型算

    return (
        f"{city} {days} 天（{level}档，人均 {daily} 元/天）"
        f"预计总花费约 {total} 元人民币。"
    )


# ══════════════════════════════════════════════════
# 错误处理：只兜预料到的
# ══════════════════════════════════════════════════
def on_error(exc: Exception, request) -> str | None:
    name = request.tool_call["name"]
    if isinstance(exc, (TimeoutError, ConnectionError)):
        return f"`{name}` 连接超时，请告知用户稍后重试，不要立即重复调用。"
    if isinstance(exc, (ValueError, KeyError)):
        return f"`{name}` 参数有误（{type(exc).__name__}），请检查后重试或询问用户。"
    return None      # 其他异常继续抛，别掩盖 bug


# ══════════════════════════════════════════════════
# 组装
# ══════════════════════════════════════════════════
agent = create_agent(
    model=init_chat_model("deepseek:deepseek-chat", temperature=0, timeout=30, max_retries=2),
    tools=[get_weather, get_exchange_rate, estimate_budget],
    context_schema=UserCtx,
    system_prompt=(
        "你是旅行规划助手。\n"
        "- 涉及天气、汇率、预算时【必须调用工具】，不要凭记忆回答\n"
        "- 预算档位由系统提供，不要询问用户\n"
        "- 需要算钱时优先用 estimate_budget，不要自己做乘法\n"
        "- 工具返回「暂无数据」「不支持」时如实告知，不要编造\n"
        "- 回答用中文，控制在 5 句话以内"
    ),
    middleware=[
        ToolErrorMiddleware(on_error),                                # 错误兜底
        ModelCallLimitMiddleware(run_limit=6, exit_behavior="end"),   # 控成本
        ToolCallLimitMiddleware(run_limit=5, exit_behavior="end"),    # 控循环
    ],
)


# ══════════════════════════════════════════════════
# 调用 + 成本监控
# ══════════════════════════════════════════════════
COST_ALERT = 0.01          # 单次超过 1 分钱就告警

daily_total = 0.0
daily_calls = 0


def ask(question: str, ctx: UserCtx) -> None:
    global daily_total, daily_calls
    print(f"❓ {question}")

    try:
        result = agent.invoke(
            {"messages": [{"role": "user", "content": question}]},
            context=ctx,
            config={"recursion_limit": 15},      # 最后一道硬保险
        )
    except GraphRecursionError:
        print("   ⚠️ 超过步数上限，已中断")
        print("💬 这个问题比较复杂，我暂时处理不了，建议咨询人工客服。\n" + "-" * 58)
        return

    calls = tin = tout = 0
    for m in result["messages"]:
        for c in getattr(m, "tool_calls", None) or []:
            print(f"   🔧 {c['name']}({c['args']})")
        if type(m).__name__ == "ToolMessage" and getattr(m, "status", None) == "error":
            print(f"   ❌ 工具报错：{m.text[:60]}")
        if u := getattr(m, "usage_metadata", None):
            calls += 1
            tin += u["input_tokens"]
            tout += u["output_tokens"]

    cost = (tin * 2.0 + tout * 8.0) / 1_000_000
    daily_total += cost
    daily_calls += 1

    flag = "🔴 超预算" if cost > COST_ALERT else "🟢"
    print(f"   💰 {flag} 模型调用 {calls} 次｜入 {tin}/出 {tout}｜约 ¥{cost:.6f}")
    if cost > COST_ALERT:
        print("   ⚠️ 建议检查：工具描述是否过长、是否有多余的调用轮次")

    print(f"💬 {result['messages'][-1].text}")
    print(f"   [今日累计 {daily_calls} 次问答，¥{daily_total:.4f}]\n" + "-" * 58)


if __name__ == "__main__":
    ctx = UserCtx(user_id="U1001", budget_level="标准")

    ask("我想去东京玩 5 天，天气怎么样？大概要花多少钱？", ctx)
    ask("那 5000 日元大概是多少人民币？", ctx)
    ask("去火星度假的话天气如何？", ctx)        # 测"查不到"的处理
