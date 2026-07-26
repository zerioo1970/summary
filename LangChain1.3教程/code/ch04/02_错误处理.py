"""第 4 章 - 工具报错怎么办（1.x 最容易踩的坑）。

⚠️ 核心结论（我在 langchain 1.3.14 实测）：
   - 工具【执行中】抛异常 → 默认会让整个 Agent 崩掉，连 ToolException 也一样
   - 模型传错【参数类型】→ 不会崩，自动转成 error ToolMessage，模型能自我纠正

需要 API Key。想离线看行为的话，把 model 换成假模型即可。
"""

from dotenv import load_dotenv
from langchain.agents import create_agent
from langchain.agents.middleware import ToolErrorMiddleware
from langchain.chat_models import init_chat_model
from langchain.tools import tool

load_dotenv()
model = init_chat_model("deepseek:deepseek-chat", temperature=0)


# ══════════════════════════════════════════════════
# 方案一：工具内部自己 try/except（最简单，推荐新手）
# ══════════════════════════════════════════════════
@tool
def query_order_safe(order_no: str) -> str:
    """根据订单号查询订单状态。

    Args:
        order_no: 订单号，例如 SO20260315。
    """
    fake_db = {"SO20260315": ("已发货", 12800.0)}

    try:
        row = fake_db.get(order_no.strip().upper())
    except Exception:
        # ⚠️ 不要把原始异常返回给模型 —— 可能含连接串、表结构等内部信息
        return "订单查询服务暂时不可用，请告知用户稍后重试。"

    if row is None:
        # 关键：把"查不到"和"故障"分开，模型才知道该怎么回复用户
        return f"未找到订单 {order_no}，请让用户确认订单号是否正确。"

    return f"订单 {order_no}：状态 {row[0]}，金额 {row[1]:.2f} 元。"


# ══════════════════════════════════════════════════
# 方案二：ToolErrorMiddleware 统一兜底
# ══════════════════════════════════════════════════
@tool
def broken_tool(x: str) -> str:
    """一个一定会抛异常的工具（用于演示）。

    Args:
        x: 随便传。
    """
    raise ValueError("数据库连接失败：timeout expired")


def on_error(exc: Exception, request) -> str | None:
    """工具执行抛异常时被调用。

    返回字符串 → 转成 ToolMessage(status="error") 递给模型，Agent 继续跑
    返回 None   → 异常继续往上抛（Agent 崩），用于不该被掩盖的错误
    """
    name = request.tool_call["name"]

    if isinstance(exc, (TimeoutError, ConnectionError)):
        return f"`{name}` 连接超时，请告知用户稍后重试，不要立即重复调用。"
    if isinstance(exc, ValueError):
        # ⚠️ 官方建议：返回异常【类型名】而不是 str(exc)，避免泄露内部细节
        return f"`{name}` 执行失败（{type(exc).__name__}），请换个方式或询问用户。"

    return None       # 其他异常照旧抛出，别掩盖代码 bug


if __name__ == "__main__":
    # ---------- 演示 1：不装中间件，工具抛异常 → Agent 崩 ----------
    print("=== 演示 1：不装错误中间件 ===")
    agent_bad = create_agent(model=model, tools=[broken_tool])
    try:
        agent_bad.invoke({"messages": [{"role": "user", "content": "用 broken_tool 处理一下 abc"}]})
        print("（没崩，说明模型没调那个工具）")
    except Exception as e:
        print(f"❌ 整个 invoke 抛异常了：{type(e).__name__}: {e}")

    # ---------- 演示 2：装上中间件 ----------
    print("\n=== 演示 2：装上 ToolErrorMiddleware ===")
    agent_good = create_agent(
        model=model,
        tools=[broken_tool],
        middleware=[ToolErrorMiddleware(on_error)],
    )
    result = agent_good.invoke(
        {"messages": [{"role": "user", "content": "用 broken_tool 处理一下 abc"}]}
    )
    for m in result["messages"]:
        status = getattr(m, "status", None)
        flag = " [status=error]" if status == "error" else ""
        print(f"  [{type(m).__name__}]{flag} {m.text[:70]}")

    # ---------- 演示 3：方案一，查不到 vs 查到 ----------
    print("\n=== 演示 3：区分「查不到」和「故障」===")
    agent_safe = create_agent(
        model=model,
        tools=[query_order_safe],
        system_prompt="你是订单助手。工具说查不到就如实告知用户，不要编造订单信息。",
    )
    for q in ["查一下 SO20260315", "查一下 SO99999999"]:
        r = agent_safe.invoke({"messages": [{"role": "user", "content": q}]})
        print(f"  ❓ {q}\n  💬 {r['messages'][-1].text}\n")
