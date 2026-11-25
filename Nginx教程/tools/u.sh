#!/bin/bash
# 在 Ubuntu 26.04.1 根文件系统里逐行执行命令文件，输出 "$ 命令" + 结果（以 root 身份）。
#   bash tools/u.sh cmds.txt          使用宿主机网络
#   NONET=1 bash tools/u.sh cmds.txt  只有回环网卡的独立网络（防火墙类示例必须用）
# 同一个命令文件里的命令在同一个 bash 里执行，后台启动的 nginx 在文件执行完后随沙箱一起结束。
D=$(cd "$(dirname "$0")" && pwd)
R=${UROOT:-/projects/sandbox/.tools/u2604}
cp "$1" "$R/opt/cmds.txt"
LOOP='while IFS= read -r line <&3; do [ -z "$line" ] && continue; [[ "$line" == "#"* ]] && { eval "${line:1}" >/dev/null 2>&1; continue; }; printf "\$ %s\n" "$line"; eval "$line"; printf "\n"; done 3< /opt/cmds.txt'
NET=""; [ -n "$NONET" ] && NET="--unshare-net"
bwrap $NET --clearenv --unshare-pid --die-with-parent --unshare-uts --hostname web01 \
  --bind "$R" / --dev /dev --proc /proc --ro-bind /sys /sys --tmpfs /run --tmpfs /tmp \
  --ro-bind /etc/resolv.conf /etc/resolv.conf \
  --setenv HOME /root --setenv TERM xterm --setenv LANG C.UTF-8 --setenv COLUMNS 100 \
  --setenv PATH /usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin \
  /bin/bash -c "cd /root; $LOOP" < /dev/null 2>&1
