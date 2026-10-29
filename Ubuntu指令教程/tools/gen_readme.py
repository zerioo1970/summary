#!/usr/bin/env python3
# Ubuntu指令教程/tools/gen_readme.py
# 作用：
#   1. 生成 README.md 里的“分类目录”和“总目录对应表”；
#   2. 给每个分类文件加上“上一类 / 下一类 / 返回总目录”导航；
#   3. 检查每个指令在所属分类文件里都有锚点 <a id="cmd-指令名">。
# 用法（在 Ubuntu指令教程/ 目录下执行）：python3 tools/gen_readme.py
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# 分类：(分类编号, 分类名, 一句话说明)。文件名为 docs/编号-分类名.md，同一类的初级、中级、高级指令写在同一个文件里
CATEGORIES = [
    ("01", "目录和文件操作", "浏览目录，复制、移动、删除、查找、校验文件"),
    ("02", "文件查看和编辑", "查看文件内容，在终端里编辑文件"),
    ("03", "文本处理", "搜索、替换、统计、排序、截取、拼接文本"),
    ("04", "压缩和打包", "tar、zip、gzip、xz、bzip2、7z 的打包和解包"),
    ("05", "用户和权限", "建删用户和组、提权、改密码、改权限、查登录记录"),
    ("06", "软件包管理", "apt、dpkg、snap、flatpak，软件源和系统升级"),
    ("07", "磁盘管理", "看空间、分区、格式化、挂载、交换区"),
    ("08", "进程管理", "查看、结束、调度进程，前后台作业"),
    ("09", "系统服务和开关机", "systemctl 管服务，日志和日志轮转，cgroup，重启和关机"),
    ("10", "网络和远程连接", "IP、DNS、端口、防火墙、下载、ssh 远程和同步"),
    ("11", "系统信息和硬件", "版本、内核、时间时区、CPU、内存、硬件和内核模块"),
    ("12", "Shell使用", "历史、别名、环境变量、帮助、脚本基础、终端复用"),
    ("13", "性能监控", "CPU、内存、磁盘 IO 的统计、历史数据和性能剖析"),
    ("14", "定时任务", "周期执行和一次性执行"),
    ("15", "开发和容器", "版本控制、容器编排、调试工具和自动化运维"),
    ("16", "虚拟化和隔离", "虚拟机、系统容器、命名空间和 chroot"),
    ("17", "安全", "证书和加密、强制访问控制、审计、防暴力破解、端口扫描"),
    ("18", "启动和内核", "引导程序、initramfs、安全启动、第三方内核模块、云初始化、崩溃转储"),
]
CAT_NAME = {c[0]: c[1] for c in CATEGORIES}

# 级别：按排名划分，星级越多越基础
LEVELS = [("初级", "★★★", 1, 100), ("中级", "★★", 101, 200), ("高级", "★", 201, 300)]


def level_of(rank):
    for name, stars, lo, hi in LEVELS:
        if lo <= rank <= hi:
            return name, stars


# 300 个指令，按使用频率排名。已写的写成“指令|用途|分类编号”，未写的只有“指令|用途”
COMMANDS = """
ls|列出目录内容|01
cd|切换目录|01
pwd|显示当前目录|01
sudo|以管理员身份执行|05
cat|查看文件内容|02
cp|复制文件和目录|01
mv|移动或重命名|01
rm|删除文件和目录|01
mkdir|新建目录|01
touch|新建空文件或修改时间|01
nano|简易文本编辑器|02
apt|安装、升级、卸载软件|06
clear|清屏|12
echo|输出一行文本|03
man|查看命令手册|12
history|查看命令历史|12
exit|退出 Shell 或脚本|12
less|分页查看长文件|02
head|查看文件开头|02
tail|查看文件末尾、跟踪日志|02
grep|按关键词搜索文本|03
find|按条件查找文件|01
chmod|修改文件权限|05
chown|修改所有者和所属组|05
df|查看磁盘空间|07
du|查看目录占用的空间|07
free|查看内存|11
top|实时查看负载和进程|08
ps|查看进程列表|08
kill|结束进程|08
ping|测试网络连通|10
ip|查看 IP、网卡和路由|10
wget|下载文件|10
curl|发送 HTTP 请求|10
tar|打包和解包|04
zip|压缩成 zip|04
unzip|解压 zip|04
reboot|重启系统|09
shutdown|关机或定时重启|09
whoami|显示当前用户名|05
uname|查看内核和系统信息|11
date|显示和计算日期时间|11
cal|显示日历|11
which|查命令对应的程序位置|12
whereis|查程序和手册的位置|12
file|判断文件类型|01
wc|统计行数、字数|03
sort|排序|03
uniq|去重和计数|03
rmdir|删除空目录|01
ln|创建链接|01
more|简单的分页查看|02
passwd|修改密码|05
su|切换到其他用户|05
hostname|查看主机名和本机 IP|10
uptime|查看运行时长和负载|11
lsb_release|查看 Ubuntu 版本|11
vim|全功能的终端编辑器|02
tree|树状显示目录|01
alias|给命令起别名|12
type|查看命令的真实身份|12
help|bash 内建命令的帮助|12
apt-get|脚本里用的包管理命令|06
dpkg|安装和查询 .deb 包|06
snap|管理 Snap 软件包|06
systemctl|管理系统服务|09
lsblk|列出磁盘和分区|07
mount|挂载磁盘和分区|07
umount|卸载磁盘和分区|07
id|查看 UID 和所属组|05
groups|查看所属组|05
who|查看谁登录了系统|05
w|查看谁在线、在做什么|05
last|查看登录历史|05
env|查看环境变量|12
export|设置环境变量|12
source|在当前 Shell 里执行脚本|12
diff|比较文件差异|03
cut|按列截取文本|03
tee|同时输出到屏幕和文件|03
gzip|压缩单个文件|04
gunzip|解压 .gz 文件|04
ssh|远程登录服务器|10
scp|远程复制文件|10
htop|更好用的进程监控|08
fastfetch|一屏看清系统概况|11
sleep|暂停一段时间|08
watch|定时重复执行命令|08
time|统计命令执行时间|08
basename|从路径取文件名|01
dirname|从路径取目录|01
stat|查看文件详细属性|01
locate|按文件名快速查找|01
updatedb|更新 locate 的数据库|01
lscpu|查看 CPU 信息|11
lsusb|列出 USB 设备|11
lspci|列出 PCI 设备|11
nproc|显示 CPU 数量|11
killall|按程序名结束进程|08
xdg-open|用默认程序打开文件|12
journalctl|查看 systemd 日志|09
sed|流式文本替换|03
awk|按列处理文本|03
xargs|把输入转为参数|03
tr|字符替换和删除|03
ufw|简易防火墙|10
useradd|新建用户（底层）|05
adduser|新建用户（交互式）|05
usermod|修改用户|05
userdel|删除用户（底层）|05
deluser|删除用户（交互式）|05
groupadd|新建组|05
visudo|安全编辑 sudoers|05
crontab|定时任务|14
rsync|增量同步|10
ss|查看端口和连接|10
netstat|旧版端口查看|10
dig|DNS 查询|10
nslookup|DNS 查询|10
traceroute|路由追踪|10
tracepath|路由追踪（免 root）|10
nmcli|NetworkManager 命令行|10
netplan|网络配置|10
resolvectl|DNS 解析状态|10
hostnamectl|主机名管理|11
timedatectl|时区和时间同步|11
localectl|语言和键盘布局|11
git|版本控制|15
docker|容器管理|15
pgrep|按名称查进程号|08
pkill|按名称结束进程|08
nice|指定优先级启动|08
renice|调整进程优先级|08
nohup|退出终端后继续运行|08
bg|作业转后台|08
fg|作业转前台|08
jobs|查看后台作业|08
disown|让作业脱离 Shell|08
screen|终端会话保持|12
tmux|终端复用|12
lsof|查看打开的文件和端口|08
fdisk|分区|07
parted|分区（大磁盘）|07
mkfs|格式化|07
blkid|查看分区 UUID|07
fsck|文件系统检查|07
dmesg|内核消息|11
vmstat|内存和 CPU 统计|13
iostat|磁盘 IO 统计|13
ssh-keygen|生成 SSH 密钥|10
ssh-copy-id|上传公钥|10
sftp|安全文件传输|10
chgrp|修改所属组|05
umask|默认权限掩码|05
md5sum|MD5 校验|01
sha256sum|SHA256 校验|01
base64|Base64 编解码|03
xz|xz 压缩|04
bzip2|bzip2 压缩|04
7z|7-Zip 压缩|04
zcat|直接查看 .gz|04
zgrep|在 .gz 中搜索|04
column|按列对齐输出|03
paste|按行合并文件|03
join|按字段连接文件|03
split|拆分大文件|01
comm|比较两个有序文件|03
nl|加行号|03
rev|每行字符反转|03
tac|行倒序输出|03
printf|格式化输出|03
read|读取输入|12
test|条件判断|12
seq|生成数字序列|03
bc|计算器|12
expr|表达式求值|12
dd|块级复制、写镜像|01
sync|把缓存写入磁盘|01
apt-cache|查询软件包信息|06
apt-mark|锁定或标记软件包|06
add-apt-repository|添加软件源或 PPA|06
update-alternatives|多版本切换|06
do-release-upgrade|升级发行版|06
unattended-upgrade|自动安全更新|06
flatpak|Flatpak 包管理|06
loginctl|登录会话管理|05
chage|密码过期策略|05
getent|查询用户、组、主机|05
sudoedit|以 root 身份安全编辑|05
systemd-analyze|开机耗时分析|09
lsmod|已加载的内核模块|11
modprobe|加载或卸载模块|11
swapon|启用或查看交换区|07
mkswap|创建交换区|07
findmnt|挂载点树|07
ncdu|交互式磁盘占用分析|07
iotop|按进程看 IO|13
iftop|按连接看带宽|10
ssh-agent|SSH 密钥代理|10
at|一次性定时任务|14
iptables|传统防火墙规则|10
nft|nftables 防火墙|10
tcpdump|抓包|10
strace|跟踪系统调用|15
ltrace|跟踪库函数调用|15
gdb|调试器|15
perf|性能剖析|13
lvcreate|创建 LVM 逻辑卷|07
pvcreate|创建 LVM 物理卷|07
vgcreate|创建 LVM 卷组|07
lvextend|扩容逻辑卷|07
resize2fs|调整 ext4 大小|07
xfs_growfs|扩容 XFS|07
mdadm|软 RAID|07
cryptsetup|LUKS 磁盘加密|07
zfs|ZFS 数据集管理|07
zpool|ZFS 存储池|07
btrfs|Btrfs 管理|07
lxc|LXD 容器和虚拟机|16
virsh|KVM 虚拟机管理|16
qemu-img|虚拟磁盘镜像|16
multipass|快速起 Ubuntu 虚拟机|16
kubectl|Kubernetes 管理|15
microk8s|轻量 Kubernetes|15
podman|无守护进程容器|15
docker compose|多容器编排|15
openssl|证书和加密工具|17
gpg|加密和签名|17
certbot|Let's Encrypt 证书|17
aa-status|AppArmor 状态|17
aa-complain|AppArmor 告警模式|17
auditctl|审计规则|17
ausearch|搜索审计日志|17
fail2ban-client|防暴力破解|17
nmap|端口扫描（仅限自有或授权主机）|17
setfacl|设置 ACL 权限|05
getfacl|查看 ACL|05
chattr|文件特殊属性|05
lsattr|查看特殊属性|05
sysctl|内核参数|11
ulimit|资源限制|08
systemd-cgls|cgroup 树|09
systemd-run|临时单元运行命令|09
systemd-cgtop|按 cgroup 看资源|09
nsenter|进入命名空间|16
unshare|创建新命名空间|16
chroot|切换根目录|16
debootstrap|构建最小 Ubuntu 根系统|16
update-initramfs|重建 initramfs|18
update-grub|更新 GRUB 配置|18
grub-install|安装引导器|18
dkms|第三方内核模块管理|18
mokutil|安全启动密钥|18
efibootmgr|UEFI 启动项|18
smartctl|硬盘健康|07
hdparm|磁盘参数和测速|07
nvme|NVMe 管理|07
sar|历史性能数据|13
pidstat|按进程统计性能|13
bpftrace|eBPF 动态追踪|13
ethtool|网卡参数|10
bridge|网桥管理|10
tc|流量控制和限速|10
wg|WireGuard VPN|10
ipset|IP 集合|10
conntrack|连接跟踪表|10
socat|多功能端口转发|10
nc|netcat 网络调试|10
mtr|ping 加 traceroute|10
iperf3|带宽测试|10
dpkg-reconfigure|重新配置软件包|06
dpkg-deb|构建和解包 .deb|06
dpkg-query|查询已装软件包|06
debconf-set-selections|预置安装应答|06
cloud-init|云主机初始化|18
pro|Ubuntu Pro 订阅和 ESM|06
ubuntu-drivers|驱动自动安装|06
needrestart|检查需重启的服务|06
logrotate|日志轮转|09
logger|写入系统日志|09
coredumpctl|崩溃转储|09
busctl|D-Bus 调试|09
udevadm|设备规则和事件|11
dmidecode|BIOS 和硬件信息|11
lshw|硬件清单|11
sensors|温度和风扇|11
cpupower|CPU 频率策略|11
numactl|NUMA 绑定|11
taskset|CPU 亲和性|08
ionice|IO 优先级|08
chrt|实时调度策略|08
losetup|回环设备|07
wipefs|清除分区签名|07
sgdisk|GPT 分区脚本化|07
ansible|批量自动化运维|15
sos|收集诊断报告|11
apport-cli|崩溃报告|11
snapcraft|构建 Snap 包|06
kdump-config|内核崩溃转储配置|18
bpftool|eBPF 程序管理|13
""".strip().split("\n")

assert len(COMMANDS) == 300, len(COMMANDS)
ITEMS = []  # (排名, 指令, 用途, 分类编号或 None)
for i, line in enumerate(COMMANDS):
    parts = line.split("|")
    ITEMS.append((i + 1, parts[0], parts[1], parts[2] if len(parts) > 2 else None))



def cat_file(cat_id):
    return f"docs/{cat_id}-{CAT_NAME[cat_id]}.md"


def anchor(name):
    return "cmd-" + name.replace(" ", "-")


def heading_prefix(rank):
    # 指令标题的开头：排名 + 空格 + 星级 + 两个空格（用 &nbsp; 防止 Markdown 把两个空格合并成一个）
    return f"## {rank:03d} {level_of(rank)[1]}&nbsp;&nbsp;"


def cmds_in(cat_id):
    return [it for it in ITEMS if it[3] == cat_id]


def level_counts(cat_id):
    parts = []
    for name, stars, lo, hi in LEVELS:
        k = sum(1 for it in cmds_in(cat_id) if lo <= it[0] <= hi)
        if k:
            parts.append(f"{stars} {name} {k} 个")
    return "，".join(parts)


def build_category_table():
    out = ["| 分类 | ★★★ 初级 | ★★ 中级 | ★ 高级 |", "|---|---|---|---|"]
    for cid, cname, desc in CATEGORIES:
        cols = []
        for name, stars, lo, hi in LEVELS:
            links = [f"[{n}]({cat_file(cid)}#{anchor(n)})" for r, n, _, c in cmds_in(cid) if lo <= r <= hi]
            cols.append("、".join(links) if links else "—")
        out.append(f"| [{cid} {cname}]({cat_file(cid)})<br/>{desc} | " + " | ".join(cols) + " |")
    return "\n".join(out)


def build_index_table():
    out = []
    for name, stars, lo, hi in LEVELS:
        out.append(f"### {stars} {name}指令（{lo}–{hi}）\n")
        out.append("| 排名 | 级别 | 指令 | 用途 | 所在分类 | 状态 |")
        out.append("|---|---|---|---|---|---|")
        for r, n, use, cid in ITEMS:
            if not lo <= r <= hi:
                continue
            if cid:
                out.append(f"| {r} | {stars} | [`{n}`]({cat_file(cid)}#{anchor(n)}) | {use} | [{cid} {CAT_NAME[cid]}]({cat_file(cid)}) | ✅ |")
            else:
                out.append(f"| {r} | {stars} | `{n}` | {use} | 待分类 | 📝 |")
        out.append("")
    return "\n".join(out)


def build_category_head(cid, cname, desc):
    # 分类文件开头：标题、说明、本类目录。正文从第一个“---”加指令锚点开始，由人工维护
    lines = [f"# {cid} {cname}（{level_counts(cid)}）\n",
             f"> {desc}。星级表示难度：★★★ 初级、★★ 中级、★ 高级。标题里的三位数字是使用频率排名。"
             f"每个指令的结构：作用 · 基本格式 · 常用参数 · 常用示例 · ⚠️ 踩坑警告 · 一句话总结。"
             f"演示环境见 [总目录](../README.md#演示环境)。\n",
             '<a id="本类目录"></a>\n',
             "## 本类目录\n",
             "| 排名 | 级别 | 指令 | 用途 |", "|---|---|---|---|"]
    for r, n, use, _ in cmds_in(cid):
        lines.append(f"| {r} | {level_of(r)[1]} | [`{n}`](#{anchor(n)}) | {use} |")
    return "\n".join(lines) + "\n"


def replace_block(text, begin, end, content):
    head, rest = text.split(begin)
    _, tail = rest.split(end)
    return head + begin + "\n\n" + content + "\n\n" + end + tail


FOOTER_RE = re.compile(r"\n---\n\n\[(?:← 上一类|返回总目录)[^\n]*\n?\Z")
BODY_START = '\n---\n\n<a id="cmd-'


def rebuild_category_files():
    ids = [c[0] for c in CATEGORIES if cmds_in(c[0])]
    for idx, cid in enumerate(ids):
        cname, desc = CAT_NAME[cid], [c[2] for c in CATEGORIES if c[0] == cid][0]
        p = os.path.join(ROOT, cat_file(cid))
        text = open(p, encoding="utf-8").read()
        body = text[text.index(BODY_START):]
        body = FOOTER_RE.sub("\n", body).rstrip("\n") + "\n"
        parts = []
        if idx > 0:
            prev = ids[idx - 1]
            parts.append(f"[← 上一类：{CAT_NAME[prev]}]({os.path.basename(cat_file(prev))})")
        parts.append("[返回总目录](../README.md)")
        if idx < len(ids) - 1:
            nxt = ids[idx + 1]
            parts.append(f"[下一类：{CAT_NAME[nxt]} →]({os.path.basename(cat_file(nxt))})")
        new = build_category_head(cid, cname, desc) + body + "\n---\n\n" + " · ".join(parts) + "\n"
        open(p, "w", encoding="utf-8").write(new)


def check_files():
    problems = []
    for r, n, _, cid in ITEMS:
        if not cid:
            continue
        p = os.path.join(ROOT, cat_file(cid))
        if not os.path.exists(p):
            problems.append(f"{cat_file(cid)} 不存在")
            continue
        text = open(p, encoding="utf-8").read()
        if f'<a id="{anchor(n)}"></a>' not in text:
            problems.append(f"{n}：缺少锚点")
        if heading_prefix(r) + n.split()[0] not in text:
            problems.append(f"{n}：标题应以“{heading_prefix(r)}{n}”开头")
    if problems:
        sys.exit("\n".join(problems))


def main():
    check_files()
    readme = os.path.join(ROOT, "README.md")
    s = open(readme, encoding="utf-8").read()
    s = replace_block(s, "<!-- 分类目录开始 -->", "<!-- 分类目录结束 -->", build_category_table())
    s = replace_block(s, "<!-- 总目录开始 -->", "<!-- 总目录结束 -->", build_index_table())
    open(readme, "w", encoding="utf-8").write(s)
    rebuild_category_files()


if __name__ == "__main__":
    main()
