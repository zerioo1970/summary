#!/bin/bash
# 在 Ubuntu 26.04.1 根文件系统里逐行执行命令文件，输出 "$ 命令" + 结果。
#   bash .tools/u.sh cmds.txt         以普通用户 tom 在 ~/demo 下执行
#   ROOT=1 bash .tools/u.sh cmds.txt  以 root 在 /root 下执行
#   NONET=1 ...                      在只有 lo 网卡的独立网络里执行（端口类示例用，输出更干净）
# /home、/tmp 每次都是新的 tmpfs，演示目录由 setup-demo.sh 重建，保证输出可复现。
D=$(cd "$(dirname "$0")" && pwd)
R=${UROOT:-/projects/sandbox/.tools/u2604}   # Ubuntu 26.04.1 根文件系统解包位置
cp "$D/setup-demo.sh" "$R/root/setup-demo.sh"
cp "$1" "$R/opt/cmds.txt"
LOOP='while IFS= read -r line <&3; do [ -z "$line" ] && continue; printf "\$ %s\n" "$line"; eval "$line"; printf "\n"; done 3< /opt/cmds.txt'
if [ -n "$ROOT" ]; then
  INNER="bash /root/setup-demo.sh >/dev/null; cd /root; exec bash -ic $(printf '%q' "$LOOP")"
else
  INNER="bash /root/setup-demo.sh >/dev/null; cd /home/tom/demo; exec runuser -u tom -- env HOME=/home/tom USER=tom LOGNAME=tom bash -ic $(printf '%q' "cd ~/demo; $LOOP")"
fi
NET=""; [ -n "$NONET" ] && NET="--unshare-net"   # NONET=1：使用独立的网络命名空间（只有 lo）
bwrap $NET --clearenv --unshare-pid --die-with-parent --unshare-uts --hostname ubuntu \
  --bind "$R" / --dev /dev --proc /proc --ro-bind /sys /sys --tmpfs /run --tmpfs /home --tmpfs /tmp \
  --ro-bind /etc/resolv.conf /etc/resolv.conf \
  --setenv HOME /root --setenv TERM xterm --setenv LANG C.UTF-8 --setenv COLUMNS 80 \
  --setenv PATH /usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin \
  /bin/bash -c "$INNER" < /dev/null 2>&1 | python3 "$D/norm.py"
