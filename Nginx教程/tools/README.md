# 实测工具

本目录的脚本用来在 Ubuntu 26.04.1 官方根文件系统里运行教程中的命令，得到正文里的输出。读者不需要它们。

| 文件 | 作用 |
|---|---|
| `u.sh` | 用 bwrap 进入根文件系统，以 root 逐行执行一个命令清单，输出 `$ 命令` 和结果。`NONET=1` 时在只有回环网卡的独立网络里执行（**ufw 等防火墙命令必须加**，否则会改到宿主机）。以 `#` 开头的行照常执行但不显示，用来做准备和清理 |
| `pty_run.py` | 在真实终端（pty）里按"剧本"驱动交互式命令，用来实测 ssh 首次连接提示、输入密码、scp 进度等。剧本是 JSON：`setup` 是进入沙箱后先执行的命令，`steps` 是"等待出现的文字 → 要输入的内容"列表 |
| `cmds/` | 各章用到的命令清单（`.txt` 给 `u.sh`，`.json` 给 `pty_run.py`） |
| `shot.mjs` | 用无头 Chromium 截取网页，生成 `docs/images/` 里的截图 |
| `mmdcheck.mjs` | 检查 `docs/*.md` 里的 Mermaid 图能否被解析：`node tools/mmdcheck.mjs docs/*.md` |

## 在新的 Linux 沙箱里重建

```bash
# 1. 解包 Ubuntu 26.04.1 根文件系统
mkdir -p /projects/sandbox/.tools/u2604 && cd /projects/sandbox/.tools
curl -sSLo ub.tar.gz https://cdimage.ubuntu.com/ubuntu-base/releases/26.04.1/release/ubuntu-base-26.04.1-base-amd64.tar.gz
tar -xzf ub.tar.gz -C u2604

# 2. 在根文件系统里装软件、建用户 ubuntu（密码 Abc12345）和主机密钥（在 Nginx教程/tools 目录执行）
printf '%s\n' 'echo "APT::Sandbox::User \"root\";" > /etc/apt/apt.conf.d/99sandbox' \
  'apt-get update -qq' \
  'DEBIAN_FRONTEND=noninteractive apt-get install -y -qq nginx curl ca-certificates openssh-server openssh-client ufw iproute2 procps sudo nano less tree' \
  'useradd -m -s /bin/bash -G sudo ubuntu; echo "ubuntu:Abc12345" | chpasswd; ssh-keygen -A; echo web01 > /etc/hostname' > /tmp/i.txt
bash u.sh /tmp/i.txt

# 3. 截图和 Mermaid 检查需要的 Node 包与中文字体
mkdir -p /projects/sandbox/.tools/pw && cd /projects/sandbox/.tools/pw && npm init -y && npm i playwright-core mermaid
mkdir -p /projects/sandbox/.tools/home/.fonts && curl -sSLo /projects/sandbox/.tools/home/.fonts/NotoSansSC.ttf \
  "https://github.com/google/fonts/raw/main/ofl/notosanssc/NotoSansSC%5Bwght%5D.ttf"
```

截图时 nginx 用宿主机网络在 80 端口运行（`HOME` 指向放字体的目录）：

```bash
R=/projects/sandbox/.tools/u2604
bwrap --die-with-parent --bind $R / --dev /dev --proc /proc --tmpfs /run --tmpfs /tmp /usr/sbin/nginx -g 'daemon off;' &
sleep 1; HOME=/projects/sandbox/.tools/home node shot.mjs http://127.0.0.1/ ../docs/images/ch03-欢迎页.png 900 420
kill %1
```

ssh 类剧本模拟"Windows 连服务器"：在独立网络里给回环网卡加上 `192.168.1.50`，同一个沙箱里启动 `sshd`，root 用户扮演 Windows 上的你（提示符设成 `PS C:\Users\zhang>`），`ubuntu` 用户是服务器上的账号。

## 测试环境的限制

| 限制 | 正文的处理 |
|---|---|
| 宿主机进程带有 no_new_privs 标志，`sudo` 不能提权 | 命令以 root 直接执行，输出与加 `sudo` 相同；sudo 的密码提示标注未实机验证 |
| 没有以 systemd 作为 1 号进程启动 | `systemctl` 的输出标注未实机验证；启动和停止用 `nginx`、`nginx -s stop` 代替 |
| 没有 Windows | `ssh`、`scp`、`ssh-keygen`、`curl.exe` 用 Linux 版同名程序实测，Windows 上的路径写法按 Windows 改写并标注 |
| sshd 和客户端在同一个终端 | 输错密码时 sshd 打出的 `unix_chkpwd` 日志行由 `pty_run.py` 去掉，真实客户端看不到这一行 |
