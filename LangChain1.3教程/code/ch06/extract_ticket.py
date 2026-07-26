"""第 6 章实战 - 把客户报修/投诉邮件批量转成能直接入库的工单记录。

这是本教程 18 个实战例子里的第 1 号，也是投入产出比最高的一类应用。

演示要点：
1. Literal 锁死枚举，保证下游统计不会多出分类
2. 批量处理用 handle_errors=False，把失败的挑出来人工复核（而不是被自动重试掩盖）
3. 统计成本，给出"跑一万条大概多少钱"的估算
"""

from collections import Counter
from typing import Literal

from dotenv import load_dotenv
from langchain.agents import create_agent
from langchain.agents.structured_output import StructuredOutputError, ToolStrategy
from langchain.chat_models import init_chat_model
from pydantic import BaseModel, Field

load_dotenv()


# ══════════════════════════════════════════════════
# 第一步：定义工单结构（和你的数据表字段对齐）
# ══════════════════════════════════════════════════
class 工单(BaseModel):
    """从客户报修或投诉邮件中抽取的工单记录。"""

    类别: Literal[
        "硬件故障", "软件报错", "账号权限", "网络问题", "服务投诉", "其他"
    ] = Field(description="故障或诉求的类别")

    # ⚠️ 给出客观判断标准，否则模型会被"尽快""急"这类情绪词带偏，全标成"高"
    紧急度: Literal["高", "中", "低"] = Field(
        description="高=已影响生产或出货；中=影响效率但可绕过；低=咨询或建议"
    )

    联系人: str = Field(description="邮件中的联系人姓名，没提到填『未提供』")
    联系方式: str = Field(description="电话或邮箱，没提到填『未提供』")

    # 可选字段一定给 default，并在 description 里写清"没有就填什么"
    涉及部门: str | None = Field(default=None, description="邮件明确提到的部门，没提到留空")

    问题摘要: str = Field(description="30 字以内客观描述，不要加处理建议")
    关键词: list[str] = Field(
        default_factory=list, description="2-4 个便于检索的关键词，用原文词汇"
    )


# ══════════════════════════════════════════════════
# 第二步：建 Agent
# ══════════════════════════════════════════════════
model = init_chat_model("deepseek:deepseek-chat", temperature=0, timeout=30, max_retries=2)

SYSTEM_PROMPT = """\
# 角色
你是 IT 服务台的工单录入员，处理车间和办公部门的报修邮件。

# 任务
从邮件原文中抽取工单字段。

# 约束
- 只填邮件里【明确提到】的信息，禁止推测和补充
- 邮件没提到的可选字段留空，不要编造
- 紧急度严格按字段说明判断，不要被"尽快""急"这类情绪词单独左右
- 问题摘要只描述现象，不要写处理建议
"""

# ⚠️ 批量场景用 handle_errors=False：让失败暴露出来，而不是自动重试掩盖问题
#    交互式应用（客服助手）应该用默认的 True，用户感知不到重试
agent = create_agent(
    model=model,
    tools=[],
    response_format=ToolStrategy(schema=工单, handle_errors=False),
    system_prompt=SYSTEM_PROMPT,
)


# ══════════════════════════════════════════════════
# 第三步：待处理的邮件（真实项目里从邮箱/数据库读）
# ══════════════════════════════════════════════════
邮件列表 = [
    """王工你好，我们车间三号线的扫码枪从今天早上开始就读不出条码了，
       换了个新的也一样，现在只能手工录，已经影响出货了，麻烦尽快！
       ——李建国 13900001111""",

    """你好，请问 ERP 系统能不能加一个按客户导出报表的功能？
       现在只能一个个点开看，财务部这边每月对账要花很久。
       财务部 张莉 zhangli@example.com""",

    """我的账号今天登录提示"权限不足"，昨天还好的。
       我是采购部的老陈，工号 U2033。""",

    """上周报修的打印机到现在还没人来处理，第三次催了。
       行政部 刘敏 转 8802""",
]


# ══════════════════════════════════════════════════
# 辅助函数
# ══════════════════════════════════════════════════
def 算钱(result, price_in: float = 2.0, price_out: float = 8.0) -> tuple[int, int, float]:
    """从 messages 里累计 token 并算钱（元/百万 token 计价）。"""
    tin = tout = 0
    for m in result["messages"]:
        if u := getattr(m, "usage_metadata", None):
            tin += u["input_tokens"]
            tout += u["output_tokens"]
    return tin, tout, (tin * price_in + tout * price_out) / 1_000_000


def 入库(t: 工单) -> None:
    """真实项目里这里是参数化 INSERT。这里只打印。"""
    print(f"   ✅ [{t.类别}/{t.紧急度}] {t.联系人}({t.联系方式})"
          f" 部门={t.涉及部门 or '—'} 关键词={t.关键词}")
    print(f"      摘要：{t.问题摘要}")
    # cursor.execute(
    #     "INSERT INTO 工单表 (类别,紧急度,联系人,联系方式,涉及部门,摘要) VALUES (?,?,?,?,?,?)",
    #     (t.类别, t.紧急度, t.联系人, t.联系方式, t.涉及部门, t.问题摘要),
    # )


# ══════════════════════════════════════════════════
# 主流程
# ══════════════════════════════════════════════════
if __name__ == "__main__":
    成功: list[工单] = []
    失败: list[tuple[int, str]] = []
    总花费 = 0.0

    for i, 邮件 in enumerate(邮件列表, 1):
        print(f"\n--- 第 {i} 封 ---")
        try:
            result = agent.invoke({"messages": [{"role": "user", "content": 邮件}]})
        except StructuredOutputError as e:
            # 抽取失败：记下来人工复核，不要静默跳过
            print(f"   ❌ 抽取失败：{type(e).__name__}: {str(e)[:80]}")
            失败.append((i, str(e)))
            continue

        ticket = result["structured_response"]
        入库(ticket)
        成功.append(ticket)

        tin, tout, cost = 算钱(result)
        总花费 += cost
        print(f"      💰 入 {tin}/出 {tout} token，¥{cost:.6f}")

    # ── 汇总 ──
    print("\n" + "=" * 58)
    print(f"成功 {len(成功)} 条 ｜ 需人工复核 {len(失败)} 条 ｜ 总花费 ¥{总花费:.6f}")
    if 成功:
        单条 = 总花费 / len(成功)
        print(f"单条约 ¥{单条:.6f} → 跑 1 万条约 ¥{单条 * 10000:.2f}")

    if 失败:
        print("\n需人工复核的：")
        for i, err in 失败:
            print(f"  第 {i} 封：{err[:100]}")

    # ── 分类统计：因为用了 Literal，这里不可能出现意外分类 ──
    if 成功:
        print("\n分类分布：  ", dict(Counter(t.类别 for t in 成功)))
        print("紧急度分布：", dict(Counter(t.紧急度 for t in 成功)))

    # 💡 如果换成 str 而不是 Literal，上面的统计会变成：
    #    {'硬件故障': 1, '设备问题': 1, '硬件': 1, ...} —— 同一个意思三种写法，报表就废了
    #    Literal 是数据质量的保证，不只是格式约束。
