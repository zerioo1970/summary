"""第 2 章 - 手动实现多轮对话记忆。

核心认知：模型是无状态的，没有记忆。
所谓"记忆"，就是你自己维护的这个消息列表，每次全部重发一遍。

第 8 章会用 checkpointer 三行代码自动化这件事，
但先手动做一遍，你才知道那三行背后发生了什么。
"""

from dotenv import load_dotenv
from langchain.chat_models import init_chat_model
from langchain.messages import SystemMessage, HumanMessage

load_dotenv()
模型 = init_chat_model("deepseek:deepseek-chat", temperature=0)

# ↓↓↓ 这个列表就是"记忆"本体 ↓↓↓
对话历史 = [SystemMessage("你是一个简洁的助手，回答不超过一句话。")]

print("开始聊天（输入 q 退出）")
print("试试：先说『我叫老王，在做仓库系统』，再问『我刚才说我叫什么』\n")

while True:
    用户输入 = input("你：").strip()
    if 用户输入.lower() == "q":
        break
    if not 用户输入:
        continue

    # 1. 把用户这句加进历史
    对话历史.append(HumanMessage(用户输入))

    # 2. 把【整个历史】发给模型 —— 不是只发最新这一句
    回复 = 模型.invoke(对话历史)

    # 3. ⚠️ 把模型的回答也加回历史。忘了这步，AI 就会失忆
    对话历史.append(回复)

    print(f"AI：{回复.text}")
    print(f"   （历史 {len(对话历史)} 条 | 本次输入 {回复.usage_metadata['input_tokens']} token）\n")

# 💡 注意观察那个 token 数字：它每轮都在涨。
#    这就是长对话越聊越贵的原因 —— 前面所有内容每轮都要重发一次。
