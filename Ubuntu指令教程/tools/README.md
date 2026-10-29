# 实测工具

本目录的脚本用来在 Ubuntu 26.04.1 的官方根文件系统里执行示例命令，得到文中的输出。它们是给维护教程的人（或 AI）用的，读者练习时只需要 `setup-demo.sh`。

| 文件 | 作用 |
|---|---|
| `setup-demo.sh` | 建用户 tom 和演示目录 `~/demo`。在自己的测试机上用 `sudo bash setup-demo.sh` 也能建一份同样的环境 |
| `u.sh` | 用 bwrap 进入 Ubuntu 根文件系统，逐行执行一个命令清单，输出 `$ 命令` 和结果。`ROOT=1` 以 root 执行；`NONET=1` 在只有回环网卡的独立网络里执行 |
| `norm.py` | 把 `ls -l` 风格的输出规整成普通 ext4 机器上的样子（去掉 SELinux 带来的 `+`，目录大小改为 4096，重新对齐列） |
| `cmds/` | 各指令示例用到的命令清单，文件名前的数字是指令排名 |
| `gen_readme.py` | 300 个指令的排名、用途和所属分类都登记在这里。它生成 README 的“分类目录”和“总目录对应表”，给每个分类文件加上“上一类 / 下一类”导航，并检查每个指令在分类文件里都有 `<a id="cmd-指令名">` 锚点 |

## 在一个新的 Linux 沙箱里重建

```bash
# 在任意可写目录执行，这里以 /projects/sandbox/.tools 为例
mkdir -p /projects/sandbox/.tools/u2604 && cd /projects/sandbox/.tools
curl -sSLo ub.tar.gz https://cdimage.ubuntu.com/ubuntu-base/releases/26.04.1/release/ubuntu-base-26.04.1-base-amd64.tar.gz
tar -xzf ub.tar.gz -C u2604
```

然后以 root 身份，在根文件系统里装好需要的软件包（命令清单写在一个文件里，用 `ROOT=1` 执行）：

```bash
cd Ubuntu指令教程/tools
printf '%s\n' 'echo "APT::Sandbox::User \"root\";" > /etc/apt/apt.conf.d/99sandbox' \
  'apt-get update -qq && DEBIAN_FRONTEND=noninteractive apt-get install -y -qq sudo sudo-rs nano vim less man-db wget curl ca-certificates zip unzip xz-utils bzip2 tree htop fastfetch iputils-ping iproute2 procps psmisc plocate lsb-release pciutils usbutils file ncal xdg-utils openssh-client openssh-server diffutils hostname manpages systemd cron wtmpdb' > /tmp/install.txt
ROOT=1 bash u.sh /tmp/install.txt
```

基础镜像是“最小化”的，没有手册页。要让 `man` 可用，还需要删除 `/etc/dpkg/dpkg.cfg.d/excludes`、移除 `/usr/bin/man` 的转向（`rm -f /usr/bin/man; dpkg-divert --local --rename --remove /usr/bin/man`），再用 `apt-get install --reinstall` 重装相关软件包并执行 `mandb`。

执行一个页面的命令清单：

```bash
bash u.sh cmds/001-ls.txt           # 以 tom 身份在 ~/demo 下执行
ROOT=1 bash u.sh cmds/063-064.txt   # 以 root 身份执行
```

根文件系统不在默认位置时，用环境变量 `UROOT` 指定。

## 测试环境的限制

| 限制 | 影响的指令 |
|---|---|
| 进程带有 no_new_privs 标志，setuid/setgid 程序无法提权 | sudo、普通用户的 locate、su 到 root |
| 没有以 systemd 作为 1 号进程启动 | systemctl start/stop/status、reboot、shutdown、snap |
| 不能挂载设备 | mount、umount |
| 没有真实终端 | nano、vim、less、htop 的界面，交互式输入密码 |
| `/home`、`/tmp` 是 tmpfs，updatedb 默认不收录 | locate 的演示改用 `/opt` |
| 没有独立网络时，root 修改防火墙会改到宿主机 | **ufw、iptables、nft 等防火墙示例必须加 `NONET=1`** |
| 内核参数是全机共享的，root 修改会改到宿主机 | **不要在测试环境执行 `sysctl -w`、`sysctl --system`**，只做读取类示例 |
| 没有 D-Bus | hostnamectl、timedatectl、localectl、loginctl、resolvectl、nmcli |

这些内容在页面里都标注了 `⚠️ 未实机验证`。

## 新增指令的写法

1. 在 `gen_readme.py` 的 `COMMANDS` 里给该指令补上分类编号；需要新类别时在 `CATEGORIES` 里加一行。
2. 在 `docs/编号-分类名.md` 里按排名顺序加入一节：先写 `<a id="cmd-指令名"></a>`，再写标题 `## 排名 星级&nbsp;&nbsp;指令：用途`（如 `## 201 ★&nbsp;&nbsp;iptables：传统防火墙规则`），各小节用 `###`，结尾加 `[↑ 回到本类目录](#本类目录)`。新类别的文件只需要先写一行占位标题，开头部分由脚本生成。
3. 在 `Ubuntu指令教程/` 目录下执行 `python3 tools/gen_readme.py`：它会重写每个分类文件开头的标题和本类目录、README 的两张目录表、分类之间的导航，并检查锚点和标题格式。
