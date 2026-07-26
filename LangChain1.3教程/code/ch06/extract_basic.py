"""第 6 章 - 结构化输出最小示例。

核心：给模型一个 Pydantic 类当"表格"让它填，拿到的是真正的 Python 对象。
⚠️ 结果不在 messages 里，在 result["structured_response"]。
"""

from typing import Literal

from dotenv import load_dotenv
from langchain.agents import create_agent
from pydantic import BaseModel, Field

load_dotenv()


# ══════════════════════════════════════════════════
# 定义你要的形状
# ⚠️ 类名、类的 docstring、Field(description=) 都是给模型看的说明书
#    （和第 4 章的工具三要素是同一套机制）
# ══════════════════════════════════════════════════
class 工单(BaseModel):
    """从客户报修邮件中抽取的工单记录。"""

    # Literal 把取值锁死在枚举里 —— 模型不可能返回第六个值
    类别: Literal["硬件故障", "软件报错", "账号权限", "网络问题", "其他"] = Field(
        description="故障类别"
    )
    紧急度: Literal["高", "中", "低"] = Field(
        description="高=已影响生产或出货；中=影响效率但可绕过；低=咨询或建议"
    )
    联系人: str
    联系方式: str = Field(description="电话或邮箱，邮件里没提到就填『未提供』")
    问题摘要: str = Field(description="30 字以内，客观描述，不要加处理建议")


agent = create_agent(
    model="deepseek:deepseek-chat",
    tools=[],                        # 这个任务不需要工具
    response_format=工单,             # ← 就这一行
    system_prompt="你是 IT 服务台的工单录入员。只依据邮件明确提到的信息填写，不要推测。",
)

邮件 = """
王工你好，我们车间三号线的扫码枪从今天早上开始就读不出条码了，
换了个新的也一样，现在只能手工录，已经影响出货了，麻烦尽快！
——李建国 13900001111
"""

if __name__ == "__main__":
    result = agent.invoke({"messages": [{"role": "user", "content": 邮件}]})

    print("result 的键：", list(result.keys()))
    print()

    # ⚠️ 新手最常犯的错：去 result["messages"][-1].text 取结果（那里可能是空字符串）
    ticket = result["structured_response"]

    print("返回类型：", type(ticket).__name__)
    print("  类别    ：", ticket.类别)
    print("  紧急度  ：", ticket.紧急度)
    print("  联系人  ：", ticket.联系人)
    print("  联系方式：", ticket.联系方式)
    print("  问题摘要：", ticket.问题摘要)
    print()
    print("转 dict 方便入库：", ticket.model_dump())
    print("转 JSON：", ticket.model_dump_json())

    # 真实项目里接下来就是照你平时的写法参数化 INSERT：
    #   cursor.execute("INSERT INTO 工单表 (类别,紧急度,联系人,联系方式,摘要) VALUES (?,?,?,?,?)",
    #                  (ticket.类别, ticket.紧急度, ticket.联系人, ticket.联系方式, ticket.问题摘要))
