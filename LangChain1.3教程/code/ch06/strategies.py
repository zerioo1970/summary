"""第 6 章 - 三种策略：AutoStrategy / ToolStrategy / ProviderStrategy。

前半部分【不需要 API Key】就能跑：
  1. 看框架从你的 Pydantic 类里提取了什么给模型（"人造工具"长什么样）
  2. 实测你的模型会被判定走哪种策略
"""

import json
from typing import Literal

from langchain.agents.structured_output import (
    AutoStrategy,
    OutputToolBinding,
    ProviderStrategy,
    ToolStrategy,
)
from pydantic import BaseModel, Field


class 工单(BaseModel):
    """从报修邮件抽取的工单记录。"""

    类别: Literal["硬件故障", "软件报错", "账号权限", "网络问题", "其他"] = Field(
        description="故障类别"
    )
    紧急度: Literal["高", "中", "低"]
    联系人: str
    问题摘要: str = Field(description="30 字以内")


# ══════════════════════════════════════════════════
# 一、ToolStrategy 会造一个"人造工具"，看看它长什么样
# ══════════════════════════════════════════════════
print("=" * 60)
print("一、框架从 Pydantic 类里提取了什么给模型")
print("=" * 60)

绑定 = OutputToolBinding.from_schema_spec(ToolStrategy(schema=工单).schema_specs[0])

print("人造工具名  ：", 绑定.tool.name, "        ← 来自【类名】")
print("人造工具描述：", 绑定.tool.description, "  ← 来自【类的 docstring】")
print("人造工具参数：")
print(json.dumps(绑定.tool.args, ensure_ascii=False, indent=2))

print("""
👆 看出来了吗？这和第 4 章的工具是同一套机制：
     函数名        → 类名
     docstring     → 类的 docstring
     参数类型标注   → 字段类型（注意 Literal 变成了 enum）
     参数说明      → Field(description=...)

   所以：类名、docstring、Field 描述写得好不好，直接决定抽取准确率。
""")


# ══════════════════════════════════════════════════
# 二、AutoStrategy 会帮你选哪个？实测一下
# ══════════════════════════════════════════════════
print("=" * 60)
print("二、你的模型会走哪种策略")
print("=" * 60)

from langchain.agents.factory import _supports_provider_strategy  # noqa: E402
from langchain.chat_models import init_chat_model  # noqa: E402

for spec in ["deepseek:deepseek-chat", "openai:gpt-4o-mini"]:
    try:
        m = init_chat_model(spec, api_key="sk-fake-for-detection")
        支持 = _supports_provider_strategy(m, tools=[])
        策略 = "ProviderStrategy（厂商原生）" if 支持 else "ToolStrategy（人造工具调用）"
        print(f"  {spec:26} → {策略}")
    except Exception as e:
        print(f"  {spec:26} → 跳过（{type(e).__name__}，可能没装对应集成包）")

print("""
判定规则（读源码得到）：
  1. 看 model.profile 里 structured_output 是否为真
  2. 不为真则匹配框架内置的兜底模型名单
  3. 都不满足 → ToolStrategy

📌 DeepSeek 的 profile 里没有 structured_output 这个键，
   所以本教程实际一直走 ToolStrategy —— 它工作得很好，你不需要做任何事。
   但要知道这一点，因为 handle_errors / tool_message_content 是 ToolStrategy 专属参数。
""")


# ══════════════════════════════════════════════════
# 三、三种写法对照
# ══════════════════════════════════════════════════
print("=" * 60)
print("三、三种写法")
print("=" * 60)
print("""
# ① 不确定就这么写（框架自动包成 AutoStrategy 并帮你判断）
create_agent(model=..., response_format=工单)

# ② 显式指定人造工具调用（DeepSeek 等国内模型的实际路径）
create_agent(model=..., response_format=ToolStrategy(
    schema=工单,
    handle_errors=True,                  # 校验失败自动让模型重试
    tool_message_content="工单已生成",     # 成功时插入的消息（会进对话历史）
))

# ③ 显式指定厂商原生（仅 OpenAI 等支持的厂商）
create_agent(model=..., response_format=ProviderStrategy(schema=工单, strict=True))

⚠️ 别硬指定 ProviderStrategy —— 厂商不支持时会报错或行为异常。
""")

# 顺手确认三种都能正常构造
for s in [AutoStrategy(schema=工单), ToolStrategy(schema=工单), ProviderStrategy(schema=工单)]:
    print(f"  ✅ {type(s).__name__} 构造成功")
