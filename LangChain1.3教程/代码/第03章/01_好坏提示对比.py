"""第 3 章 - 同一个任务，坏提示 vs 好提示，各跑 3 次看稳定性。

跑完你就再也不会忘记写 temperature=0 和输出格式约束了。
"""

from dotenv import load_dotenv
from langchain.chat_models import init_chat_model
from langchain.messages import SystemMessage, HumanMessage

load_dotenv()

产品描述 = "20*30 蓝色 加厚"

# ── ❌ 坏提示：没角色、没格式、没边界，temperature 也没管 ──────────
坏模型 = init_chat_model("deepseek:deepseek-chat")  # 默认 temperature，随机性大

print("❌ 坏提示（跑 3 次）")
print("-" * 55)
for i in range(3):
    回复 = 坏模型.invoke(f"帮我分析一下这个产品信息：{产品描述}")
    print(f"第{i+1}次：{回复.text}")

# ── ✅ 好提示：四段结构 + temperature=0 ────────────────────────
系统提示 = """你是产品数据整理员。

任务：从用户给的产品描述里提取三个字段。

规则：
- 尺寸保持原文写法，不要换算单位
- 颜色只填颜色名
- 工艺指加厚/防水/抛光这类加工特性，没有就填「无」
- 原文里没提到的字段填「无」，禁止推测

输出：只输出一行，格式为
尺寸=xxx|颜色=xxx|工艺=xxx

不要输出任何解释、前言或结尾。"""

好模型 = init_chat_model("deepseek:deepseek-chat", temperature=0)

print("\n✅ 好提示（跑 3 次）")
print("-" * 55)
for i in range(3):
    回复 = 好模型.invoke([SystemMessage(系统提示), HumanMessage(产品描述)])
    print(f"第{i+1}次：{回复.text}")

print("\n结论：稳定的配方 = 好提示（四段结构）+ temperature=0，缺一不可。")
