"""第 5 章 - 查看 Agent 的决策过程 + 三道防转圈保险。

两个日常必备技能：
1. 看清它中间调了哪些工具、花了多少钱（每天都会用）
2. 防止它自己转圈烧钱（上生产必须有）
"""

from dotenv import load_dotenv
from langchain.agents import create_agent
from langchain.agents.middleware import (
    ModelCallLimitMiddleware,
    ToolCallLimitMiddleware,
)
from langchain.chat_models import init_chat_model
from langchain.tools import tool
from langgraph.errors import GraphRecursionError

load_dotenv()
model = init_chat_model("deepseek:deepseek-chat", temperature=0, timeout=30)


@tool
def get_weather(city: str) -> str:
    """查询指定城市今天的天气。

    Args:
        city: 城市中文名，例如"上海"。
    """
    data = {"上海": "晴，28°C", "北京": "多云，24°C", "广州": "雷阵雨，31°C"}
    return data.get(city, f"暂无{city}的数据")


@tool
def check_status(task_id: str) -> str:
    """查询任务处理状态。

    Args:
        task_id: 任务编号，例如 T001。
    """
    # 永远返回"还没完"，用来演示 Agent 转圈
    return "任务仍在处理中，请再次查询以获取最新状态。"


# ══════════════════════════════════════════════════
# 调试工具函数（建议抄进你自己的项目）
# ══════════════════════════════════════════════════
def show_process(result) -> None:
    """打印完整决策过程 + 成本。"""
    for i, m in enumerate(result["messages"], 1):
        kind = type(m).__name__
        print(f"  [{i}] {kind}")

        for c in getattr(m, "tool_calls", None) or []:
            print(f"      🔧 {c['name']}({c['args']})")

        if kind == "ToolMessage":
            flag = "❌" if getattr(m, "status", None) == "error" else "✅"
            print(f"      {flag} {m.text[:70]}")
        elif m.text:
            print(f"      💬 {m.text[:70]}")

        if u := getattr(m, "usage_metadata", None):
            print(f"      💰 入 {u['input_tokens']} / 出 {u['output_tokens']} token")


def total_cost(result, price_in: float = 2.0, price_out: float = 8.0) -> None:
    """统计这一次 invoke 总共花了多少（元/百万 token 计价）。"""
    calls = tin = tout = 0
    for m in result["messages"]:
        if u := getattr(m, "usage_metadata", None):
            calls += 1
            tin += u["input_tokens"]
            tout += u["output_tokens"]
    cost = (tin * price_in + tout * price_out) / 1_000_000
    print(f"  💰 模型调用 {calls} 次｜入 {tin} / 出 {tout} token｜约 ¥{cost:.6f}")


def show_tool_calls(result) -> None:
    """只看工具调用摘要 —— 排查"它为什么没调那个工具"时最顺手。"""
    for m in result["messages"]:
        for c in getattr(m, "tool_calls", None) or []:
            print(f"  🔧 {c['name']}({c['args']})")


if __name__ == "__main__":
    # ══════════════════════════════════════════════
    # 一、查看决策过程
    # ══════════════════════════════════════════════
    print("=" * 60)
    print("一、查看决策过程")
    print("=" * 60)

    agent = create_agent(
        model=model,
        tools=[get_weather],
        system_prompt="你是天气助手，回答简洁，用中文。",
    )
    result = agent.invoke(
        {"messages": [{"role": "user", "content": "上海和广州今天哪个更适合出门？"}]}
    )
    show_process(result)
    total_cost(result)

    # 内部图结构（确认只有 model 和 tools 两个节点）
    print(f"\n  Agent 内部节点：{list(agent.get_graph().nodes)}")
    print("  ⚠️ 注意节点叫 model 不叫 agent —— 第 7 章按节点名过滤事件时会用到")

    # ══════════════════════════════════════════════
    # 二、只靠 recursion_limit：会抛异常，用户拿不到回答
    # ══════════════════════════════════════════════
    print("\n" + "=" * 60)
    print("二、方式 A：只靠 recursion_limit（硬中断）")
    print("=" * 60)

    question = {"messages": [{"role": "user", "content": "帮我盯着任务 T001，直到它完成"}]}

    agent_a = create_agent(model=model, tools=[check_status])
    try:
        r = agent_a.invoke(question, config={"recursion_limit": 6})
        print("  正常结束：", r["messages"][-1].text)
    except GraphRecursionError as e:
        print("  ❌ 抛异常，用户拿不到任何回答")
        print("    ", str(e)[:110])

    # ══════════════════════════════════════════════
    # 三、加上中间件：优雅收尾，仍能返回结果
    # ══════════════════════════════════════════════
    print("\n" + "=" * 60)
    print("三、方式 B：中间件（优雅收尾）")
    print("=" * 60)

    agent_b = create_agent(
        model=model,
        tools=[check_status],
        middleware=[
            ModelCallLimitMiddleware(run_limit=6, exit_behavior="end"),   # 控成本
            ToolCallLimitMiddleware(run_limit=2, exit_behavior="end"),    # 控循环
        ],
    )
    r = agent_b.invoke(question, config={"recursion_limit": 50})
    print(f"  ✅ 正常返回，共 {len(r['messages'])} 条消息")

    last = r["messages"][-1].text
    print(f"  框架给的最后一句：{last}")

    # ⚠️ 框架的提示是英文的，别直接给用户看 —— 换成自己的话术
    if "limit reached" in last.lower() or "limit exceeded" in last.lower():
        last = "这个任务需要较长时间，我已经帮你查了几次，建议稍后再来看看。"
    print(f"  💬 换成给用户看的：{last}")

    print("\n📌 结论：中间件负责优雅收尾，recursion_limit 当最后的断路器，两个都要。")
