"""第 2 章 - 算清每次调用花了多少钱 + 用 batch 批量处理。"""

from dotenv import load_dotenv
from langchain.chat_models import init_chat_model

load_dotenv()
模型 = init_chat_model("deepseek:deepseek-chat", temperature=0)

# DeepSeek 价格（元 / 百万 token）—— 请以官网最新价格为准
输入单价 = 2.0
输出单价 = 8.0


def 算钱(回复) -> float:
    """根据 usage_metadata 算出这次调用的花费（元）。"""
    用量 = 回复.usage_metadata
    return (用量["input_tokens"] * 输入单价 + 用量["output_tokens"] * 输出单价) / 1_000_000


# ── 单次调用 ──────────────────────────────────────────────
回复 = 模型.invoke("用一句话解释什么是数据库事务")
print("回答：", 回复.text)
print(f"输入 {回复.usage_metadata['input_tokens']} token，"
      f"输出 {回复.usage_metadata['output_tokens']} token，"
      f"花费约 {算钱(回复):.6f} 元\n")

# ── 批量处理：脏地址数据归一化 ──────────────────────────────
待处理 = [
    "上海市浦东新区张江镇科苑路88号",
    "北京海淀中关村大街1号院",
    "广东省深圳南山区科技园南区",
]

提示模板 = "把下面这个地址拆成 省|市|区 三部分，用竖线分隔，只输出结果不要解释：\n{}"
提示列表 = [提示模板.format(地址) for 地址 in 待处理]

# batch 会并发发出所有请求，比 for 循环快很多
结果列表 = 模型.batch(提示列表)

总花费 = 0.0
print("批量处理结果：")
for 原文, 回复 in zip(待处理, 结果列表):
    print(f"  {原文}\n    → {回复.text}")
    总花费 += 算钱(回复)

print(f"\n共 {len(待处理)} 条，总花费约 {总花费:.6f} 元")

# ⚠️ 几千条数据时不要一次性全塞进 batch，会被限流（HTTP 429）。
#    建议每批 20~50 条，中间 time.sleep(1)。
#    第 9 章会介绍用 ModelRetryMiddleware 自动重试。
