"""第 2 章 - invoke 返回的对象里到底装了什么。"""

from dotenv import load_dotenv
from langchain.chat_models import init_chat_model

load_dotenv()
模型 = init_chat_model("deepseek:deepseek-chat", temperature=0)

回复 = 模型.invoke("用一句话说明什么是数据库索引")

print("① 文字内容  ：", 回复.text)
print("② 对象类型  ：", type(回复).__name__)
print("③ token 用量：", 回复.usage_metadata)
print("④ 标准内容块：", 回复.content_blocks)
print("⑤ 结束原因  ：", 回复.response_metadata.get("finish_reason"))
print("⑥ 工具调用  ：", 回复.tool_calls)

# ⚠️ 检查输出有没有被 max_tokens 截断 —— 批量处理时建议加上这个判断
if 回复.response_metadata.get("finish_reason") == "length":
    print("\n⚠️ 警告：输出被截断了，这条结果不完整，别直接入库！")

# 这个模型支持什么能力（数据来自开源项目 models.dev）
print("\n模型能力档案：")
for 键 in ["max_input_tokens", "tool_calling", "image_inputs", "reasoning_output"]:
    print(f"  {键:20} {模型.profile.get(键)}")
