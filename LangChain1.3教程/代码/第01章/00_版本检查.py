"""第 1 章 - 确认环境装对了。这个脚本不需要 API Key 就能跑。"""

from importlib.metadata import version

预期 = {
    "langchain": "1.3.x",
    "langchain-core": "1.5.x   ← 注意：核心包版本号比主包高，这是正常的",
    "langgraph": "1.2.x",
    "langchain-deepseek": "1.1.x",
}

print("包名                   已装版本      预期")
print("-" * 60)
for 包名, 说明 in 预期.items():
    try:
        print(f"{包名:22} {version(包名):12}  {说明}")
    except Exception:
        print(f"{包名:22} {'未安装':12}  ❌ 请执行 uv pip install {包名}")
