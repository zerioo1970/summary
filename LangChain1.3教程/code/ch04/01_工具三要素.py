"""第 4 章 - 工具的三要素：函数名 + 类型标注 + docstring。

这个脚本【不需要 API Key】就能跑，只看 LangChain 从你的函数里提取了什么。
跑完你就明白"docstring 是给模型看的说明书"这句话的分量。
"""

from langchain.tools import tool
from langchain_core.tools import tool as tool_from_core

# ── 先确认一件事：两个导入路径是同一个东西 ──────────────────
print("langchain.tools.tool is langchain_core.tools.tool ->", tool is tool_from_core)
print()


# ══════════════════════════════════════════════════
# ✅ 好的写法
# ══════════════════════════════════════════════════
@tool
def get_exchange_rate(currency_code: str) -> str:
    """查询指定外币兑人民币的今日汇率。

    Args:
        currency_code: 三位货币代码，大写，例如 USD、EUR、JPY。
    """
    rates = {"USD": 7.18, "EUR": 7.82, "JPY": 0.047}
    code = currency_code.strip().upper()
    if code not in rates:
        return f"不支持 {code}，当前支持：{', '.join(rates)}"
    return f"1 {code} = {rates[code]} CNY"


# ══════════════════════════════════════════════════
# ❌ 灾难写法：名字没意义、没类型标注说明、docstring 敷衍
# ══════════════════════════════════════════════════
@tool
def q(o: str) -> str:
    """一个函数。"""
    return "结果"


def show(t) -> None:
    """打印模型能看到的全部信息。"""
    print(f"--- {t.name} ---")
    print(f"  描述  : {t.description}")
    print(f"  参数  : {t.args}")
    print()


show(get_exchange_rate)
show(q)

print("👆 模型看到的就是上面这些，它看不到你的源码。")
print("   下面那个工具，模型既不知道它干什么，也不知道 o 该填什么。\n")


# ══════════════════════════════════════════════════
# ⚠️ 加了 @tool 之后不能直接调用了
# ══════════════════════════════════════════════════
try:
    get_exchange_rate("USD")          # type: ignore[operator]
except TypeError as e:
    print(f"⚠️ 直接调用报错：{type(e).__name__}: {e}")

# ✅ 正确的测试方式
print("✅ 正确调用：", get_exchange_rate.invoke({"currency_code": "usd"}))
print("✅ 不支持的币种：", get_exchange_rate.invoke({"currency_code": "KRW"}))
