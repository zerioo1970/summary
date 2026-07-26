"""第 1 章 - 第一个程序：确认能跟模型说上话。

运行前：
1. 把 ../.env.example 复制成 ../.env（或本目录下的 .env），填上 DEEPSEEK_API_KEY
2. python 01_连通性测试.py
"""

from dotenv import load_dotenv
from langchain.chat_models import init_chat_model

# 把 .env 里的 DEEPSEEK_API_KEY 读进环境变量
# find_dotenv 会向上层目录查找，所以 .env 放在教程代码根目录也能找到
load_dotenv()

# 连上模型。格式固定是 "厂商:模型名"
模型 = init_chat_model("deepseek:deepseek-chat")

# 问一句话
回复 = 模型.invoke("用一句话介绍你自己")

# 取出文字。⚠️ .text 是属性，后面没有括号！
print(回复.text)
