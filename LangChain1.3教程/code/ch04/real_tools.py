"""第 4 章 - 实战：ToolRuntime 权限隔离 + 只读数据库查询（三道保险）。

这是本章最重要的一个脚本。它演示了把 AI 接到业务库上的最低安全要求：
  保险一：不让模型写 SQL，只让它填【受控参数】
  保险二：SQL 全部参数化，绝不字符串拼接
  保险三：强制 LIMIT，且部门条件来自 runtime.context 而非模型参数
（外加工程实践：只读数据库账号、异常不透传原文）
"""

import sqlite3
from dataclasses import dataclass

from dotenv import load_dotenv
from langchain.agents import create_agent
from langchain.agents.middleware import ToolErrorMiddleware
from langchain.chat_models import init_chat_model
from langchain.tools import ToolRuntime, tool

load_dotenv()


# ══════════════════════════════════════════════════
# 演示用数据库（真实项目里换成你自己的连接，用只读账号）
# ══════════════════════════════════════════════════
def init_demo_db() -> sqlite3.Connection:
    conn = sqlite3.connect(":memory:", check_same_thread=False)
    conn.row_factory = sqlite3.Row
    conn.executescript("""
        CREATE TABLE orders (
            order_no   TEXT PRIMARY KEY,
            customer   TEXT,
            dept       TEXT,
            amount     REAL,
            status     TEXT,
            order_date TEXT
        );
        INSERT INTO orders VALUES
            ('SO20260301', '华东贸易', 'SALES',    12800.00, '已发货', '2026-03-01'),
            ('SO20260302', '南方物流', 'SALES',     3200.50, '待发货', '2026-03-02'),
            ('SO20260303', '华东贸易', 'SALES',    45600.00, '已完成', '2026-03-03'),
            ('SO20260304', '北方建材', 'PURCHASE',  8900.00, '待发货', '2026-03-04');
    """)
    return conn


DB = init_demo_db()

MAX_ROWS = 20
ALLOWED_STATUS = {"待发货", "已发货", "已完成"}


# ══════════════════════════════════════════════════
# 运行时上下文：来自登录会话，模型看不到、也改不了
# ══════════════════════════════════════════════════
@dataclass
class Ctx:
    user_id: str
    dept: str
    can_see_amount: bool = True     # 演示"按权限裁剪返回字段"


# ══════════════════════════════════════════════════
# 工具：查订单
# ══════════════════════════════════════════════════
@tool
def query_orders(
    customer: str | None = None,
    status: str | None = None,
    min_amount: float | None = None,
    runtime: ToolRuntime[Ctx] = None,   # type: ignore[assignment]
) -> str:
    """查询【当前用户所属部门】的订单列表。只能查本部门数据，最多返回 20 条。

    Args:
        customer: 客户名称，支持部分匹配，例如"华东"。不填则不限。
        status: 订单状态，只能是「待发货」「已发货」「已完成」之一。不填则不限。
        min_amount: 最小金额（元），不填则不限。
    """
    # ── 保险三：部门来自运行时上下文，模型无法指定 ──
    ctx = runtime.context
    dept = ctx.dept

    # ── 入参校验：把模型给的值当外部不可信输入 ──
    if status is not None and status not in ALLOWED_STATUS:
        return f"状态 `{status}` 无效，只能是：{', '.join(ALLOWED_STATUS)}"
    if min_amount is not None and min_amount < 0:
        return "最小金额不能为负数。"

    # ── 保险二：参数化，不拼接 ──
    sql = "SELECT order_no, customer, amount, status, order_date FROM orders WHERE dept = ?"
    params: list = [dept]

    if customer:
        sql += " AND customer LIKE ?"
        params.append(f"%{customer.strip()}%")
    if status:
        sql += " AND status = ?"
        params.append(status)
    if min_amount is not None:
        sql += " AND amount >= ?"
        params.append(min_amount)

    # 多查一条，用来判断结果是否被截断
    sql += " ORDER BY order_date DESC LIMIT ?"
    params.append(MAX_ROWS + 1)

    try:
        rows = DB.execute(sql, params).fetchall()
    except Exception:
        # ⚠️ 不把原始异常返回给模型（可能含表名、连接信息）
        return "订单查询服务暂时不可用，请告知用户稍后重试。"

    if not rows:
        return "没有符合条件的订单。"

    truncated = len(rows) > MAX_ROWS
    rows = rows[:MAX_ROWS]

    # ── 按权限裁剪字段：只有有权限的人才看到金额 ──
    lines = []
    for r in rows:
        parts = [r["order_no"], r["customer"]]
        if ctx.can_see_amount:
            parts.append(f"{r['amount']:.2f}元")
        parts += [r["status"], r["order_date"]]
        lines.append(" | ".join(parts))

    out = f"共 {len(lines)} 条：\n" + "\n".join(lines)
    if truncated:
        out += f"\n（已截断到 {MAX_ROWS} 条，请让用户补充更具体的筛选条件）"
    return out


# ══════════════════════════════════════════════════
# 组装
# ══════════════════════════════════════════════════
def on_error(exc: Exception, request) -> str | None:
    if isinstance(exc, (TimeoutError, ConnectionError)):
        return f"`{request.tool_call['name']}` 连接超时，请告知用户稍后重试。"
    return None


agent = create_agent(
    model=init_chat_model("deepseek:deepseek-chat", temperature=0, timeout=30),
    tools=[query_orders],
    context_schema=Ctx,
    middleware=[ToolErrorMiddleware(on_error)],
    system_prompt=(
        "你是企业内部订单助手。\n"
        "- 查订单直接调用工具，不要询问用户的部门（系统已知）\n"
        "- 工具说「没有符合条件的订单」时如实告知，不要编造数据\n"
        "- 回答简洁，用中文"
    ),
)


if __name__ == "__main__":
    # 先确认 runtime 对模型不可见
    print("模型能看到的参数：", list(query_orders.args))
    print("runtime 是否暴露给模型：", "runtime" in query_orders.args)
    print()

    ctx = Ctx(user_id="U1001", dept="SALES", can_see_amount=True)

    for q in [
        "华东贸易一共有几个订单？",
        "金额超过 1 万的订单有哪些？",
        "北方建材的订单呢？",          # 属于 PURCHASE 部门，应该查不到 ← 越权被挡住
    ]:
        print(f"❓ {q}")
        result = agent.invoke({"messages": [{"role": "user", "content": q}]}, context=ctx)
        for m in result["messages"]:
            for c in getattr(m, "tool_calls", None) or []:
                print(f"   🔧 {c['name']}({c['args']})")
        print(f"💬 {result['messages'][-1].text}\n{'-' * 58}")

    # 💡 最后那个问题演示了保险三的价值：
    #    用户（或模型）无论怎么问，都拿不到别的部门的数据 —— 因为 dept 不是参数。
