#!/bin/bash
# 一键恢复教程构建环境（沙箱重置后运行这一条即可，约 1 分钟）
# 用法： bash setup.sh
set -e

echo "==> 1/3 安装 pandoc"
if command -v pandoc >/dev/null 2>&1; then
  echo "    已存在：$(pandoc --version | head -1)"
else
  cd /tmp
  curl -sL -o pandoc.tar.gz \
    "https://github.com/jgm/pandoc/releases/download/3.5/pandoc-3.5-linux-amd64.tar.gz"
  tar xzf pandoc.tar.gz
  cp pandoc-3.5/bin/pandoc /usr/local/bin/pandoc
  chmod +x /usr/local/bin/pandoc
  echo "    已安装：$(pandoc --version | head -1)"
fi

echo "==> 2/3 下载中文字体（画图用，路径被 src/docbuilder.py 引用）"
FONT=/projects/sandbox/fonts/NotoSansSC.ttf
if [ -f "$FONT" ]; then
  echo "    已存在：$FONT"
else
  mkdir -p /projects/sandbox/fonts
  curl -sL -o "$FONT" \
    "https://github.com/googlefonts/noto-cjk/raw/main/Sans/Variable/TTF/Subset/NotoSansSC-VF.ttf"
  echo "    已下载：$(du -h "$FONT" | cut -f1)"
fi

echo "==> 3/3 安装 Python 依赖"
pip install -q python-docx matplotlib 2>&1 | grep -v "^WARNING" || true
python3 -c "import docx, matplotlib; print('    python-docx / matplotlib 就绪')"

echo ""
echo "环境就绪。合成 Word："
echo "  bash dotnet10-最小API教程/build.sh      # 21 章版"
echo "  bash minimal-api-40章版/build.sh       # 40 章版"
