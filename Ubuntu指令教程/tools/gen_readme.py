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

# 初级分类：(分类编号, 分类名, 一句话说明)。文件名为 docs/beginner/编号-分类名.md
BEGINNER_CATEGORIES = [
    ("01", "目录和文件操作", "浏览目录，复制、移动、删除、查找文件"),
    ("02", "文件查看和编辑", "查看文件内容，在终端里编辑文件"),
    ("03", "文本处理", "搜索、统计、排序、截取、比较文本"),
    ("04", "压缩和打包", "tar、zip、gzip 三种格式的打包和解包"),
    ("05", "用户和权限", "提权、切换用户、改密码、改权限、查登录记录"),
    ("06", "软件包管理", "apt、dpkg、snap 装卸软件"),
    ("07", "磁盘管理", "看空间、看分区、挂载和卸载"),
    ("08", "进程管理", "查看、监控、结束进程，计时和定时刷新"),
    ("09", "系统服务和开关机", "systemctl 管服务，重启和关机"),
    ("10", "网络和远程连接", "IP、连通性、下载、HTTP 请求、ssh 远程"),
    ("11", "系统信息和硬件", "版本、内核、时间、CPU、内存、PCI/USB 设备"),
    ("12", "Shell使用", "清屏、历史、别名、环境变量、查帮助"),
]

# 300 个指令，按使用频率排名。初级写成“指令|用途|分类编号”，中高级暂只有“指令|用途”
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
journalctl|查看 systemd 日志
sed|流式文本替换
awk|按列处理文本
xargs|把输入转为参数
tr|字符替换和删除
ufw|简易防火墙
useradd|新建用户（底层）
adduser|新建用户（交互式）
usermod|修改用户
userdel|删除用户（底层）
deluser|删除用户（交互式）
groupadd|新建组
visudo|安全编辑 sudoers
crontab|定时任务
rsync|增量同步
ss|查看端口和连接
netstat|旧版端口查看
dig|DNS 查询
nslookup|DNS 查询
traceroute|路由追踪
tracepath|路由追踪（免 root）
nmcli|NetworkManager 命令行
netplan|网络配置
resolvectl|DNS 解析状态
hostnamectl|主机名管理
timedatectl|时区和时间同步
localectl|语言和键盘布局
git|版本控制
docker|容器管理
pgrep|按名称查进程号
pkill|按名称结束进程
nice|指定优先级启动
renice|调整进程优先级
nohup|退出终端后继续运行
bg|作业转后台
fg|作业转前台
jobs|查看后台作业
disown|让作业脱离 Shell
screen|终端会话保持
tmux|终端复用
lsof|查看打开的文件和端口
fdisk|分区
parted|分区（大磁盘）
mkfs|格式化
blkid|查看分区 UUID
fsck|文件系统检查
dmesg|内核消息
vmstat|内存和 CPU 统计
iostat|磁盘 IO 统计
ssh-keygen|生成 SSH 密钥
ssh-copy-id|上传公钥
sftp|安全文件传输
chgrp|修改所属组
umask|默认权限掩码
md5sum|MD5 校验
sha256sum|SHA256 校验
base64|Base64 编解码
xz|xz 压缩
bzip2|bzip2 压缩
7z|7-Zip 压缩
zcat|直接查看 .gz
zgrep|在 .gz 中搜索
column|按列对齐输出
paste|按行合并文件
join|按字段连接文件
split|拆分大文件
comm|比较两个有序文件
nl|加行号
rev|每行字符反转
tac|行倒序输出
printf|格式化输出
read|读取输入
test|条件判断
seq|生成数字序列
bc|计算器
expr|表达式求值
dd|块级复制、写镜像
sync|把缓存写入磁盘
apt-cache|查询软件包信息
apt-mark|锁定或标记软件包
add-apt-repository|添加软件源或 PPA
update-alternatives|多版本切换
do-release-upgrade|升级发行版
unattended-upgrade|自动安全更新
flatpak|Flatpak 包管理
loginctl|登录会话管理
chage|密码过期策略
getent|查询用户、组、主机
sudoedit|以 root 身份安全编辑
systemd-analyze|开机耗时分析
lsmod|已加载的内核模块
modprobe|加载或卸载模块
swapon|启用或查看交换区
mkswap|创建交换区
findmnt|挂载点树
ncdu|交互式磁盘占用分析
iotop|按进程看 IO
iftop|按连接看带宽
ssh-agent|SSH 密钥代理
at|一次性定时任务
iptables|传统防火墙规则
nft|nftables 防火墙
tcpdump|抓包
strace|跟踪系统调用
ltrace|跟踪库函数调用
gdb|调试器
perf|性能剖析
lvcreate|创建 LVM 逻辑卷
pvcreate|创建 LVM 物理卷
vgcreate|创建 LVM 卷组
lvextend|扩容逻辑卷
resize2fs|调整 ext4 大小
xfs_growfs|扩容 XFS
mdadm|软 RAID
cryptsetup|LUKS 磁盘加密
zfs|ZFS 数据集管理
zpool|ZFS 存储池
btrfs|Btrfs 管理
lxc|LXD 容器和虚拟机
virsh|KVM 虚拟机管理
qemu-img|虚拟磁盘镜像
multipass|快速起 Ubuntu 虚拟机
kubectl|Kubernetes 管理
microk8s|轻量 Kubernetes
podman|无守护进程容器
docker compose|多容器编排
openssl|证书和加密工具
gpg|加密和签名
certbot|Let's Encrypt 证书
aa-status|AppArmor 状态
aa-complain|AppArmor 告警模式
auditctl|审计规则
ausearch|搜索审计日志
fail2ban-client|防暴力破解
nmap|端口扫描（仅限自有或授权主机）
setfacl|设置 ACL 权限
getfacl|查看 ACL
chattr|文件特殊属性
lsattr|查看特殊属性
sysctl|内核参数
ulimit|资源限制
systemd-cgls|cgroup 树
systemd-run|临时单元运行命令
systemd-cgtop|按 cgroup 看资源
nsenter|进入命名空间
unshare|创建新命名空间
chroot|切换根目录
debootstrap|构建最小 Ubuntu 根系统
update-initramfs|重建 initramfs
update-grub|更新 GRUB 配置
grub-install|安装引导器
dkms|第三方内核模块管理
mokutil|安全启动密钥
efibootmgr|UEFI 启动项
smartctl|硬盘健康
hdparm|磁盘参数和测速
nvme|NVMe 管理
sar|历史性能数据
pidstat|按进程统计性能
bpftrace|eBPF 动态追踪
ethtool|网卡参数
bridge|网桥管理
tc|流量控制和限速
wg|WireGuard VPN
ipset|IP 集合
conntrack|连接跟踪表
socat|多功能端口转发
nc|netcat 网络调试
mtr|ping 加 traceroute
iperf3|带宽测试
dpkg-reconfigure|重新配置软件包
dpkg-deb|构建和解包 .deb
dpkg-query|查询已装软件包
debconf-set-selections|预置安装应答
cloud-init|云主机初始化
pro|Ubuntu Pro 订阅和 ESM
ubuntu-drivers|驱动自动安装
needrestart|检查需重启的服务
logrotate|日志轮转
logger|写入系统日志
coredumpctl|崩溃转储
busctl|D-Bus 调试
udevadm|设备规则和事件
dmidecode|BIOS 和硬件信息
lshw|硬件清单
sensors|温度和风扇
cpupower|CPU 频率策略
numactl|NUMA 绑定
taskset|CPU 亲和性
ionice|IO 优先级
chrt|实时调度策略
losetup|回环设备
wipefs|清除分区签名
sgdisk|GPT 分区脚本化
ansible|批量自动化运维
sos|收集诊断报告
apport-cli|崩溃报告
snapcraft|构建 Snap 包
kdump-config|内核崩溃转储配置
bpftool|eBPF 程序管理
""".strip().split("\n")

assert len(COMMANDS) == 300, len(COMMANDS)
ITEMS = []  # (排名, 指令, 用途, 分类编号或 None)
for i, line in enumerate(COMMANDS):
    parts = line.split("|")
    ITEMS.append((i + 1, parts[0], parts[1], parts[2] if len(parts) > 2 else None))

CAT_NAME = {c[0]: c[1] for c in BEGINNER_CATEGORIES}


def cat_file(cat_id):
    return f"docs/beginner/{cat_id}-{CAT_NAME[cat_id]}.md"


def anchor(name):
    return "cmd-" + name.replace(" ", "-")


def build_category_table():
    out = ["| 分类 | 个数 | 包含的指令 |", "|---|---|---|"]
    for cid, cname, desc in BEGINNER_CATEGORIES:
        cmds = [it for it in ITEMS if it[3] == cid]
        links = "、".join(f"[{n}]({cat_file(cid)}#{anchor(n)})" for _, n, _, _ in cmds)
        out.append(f"| [{cid} {cname}]({cat_file(cid)})<br/>{desc} | {len(cmds)} | {links} |")
    return "\n".join(out)


def build_index_table():
    out = []
    for title, lo, hi in [("初级指令（1–100）", 1, 100), ("中级指令（101–200）", 101, 200), ("高级指令（201–300）", 201, 300)]:
        out.append(f"### {title}\n")
        out.append("| 排名 | 指令 | 用途 | 所在分类 | 状态 |")
        out.append("|---|---|---|---|---|")
        for r, n, use, cid in ITEMS:
            if not lo <= r <= hi:
                continue
            if cid:
                out.append(f"| {r} | [`{n}`]({cat_file(cid)}#{anchor(n)}) | {use} | [{cid} {CAT_NAME[cid]}]({cat_file(cid)}) | ✅ |")
            else:
                out.append(f"| {r} | `{n}` | {use} | 待分类 | 📝 |")
        out.append("")
    return "\n".join(out)


def replace_block(text, begin, end, content):
    head, rest = text.split(begin)
    _, tail = rest.split(end)
    return head + begin + "\n\n" + content + "\n\n" + end + tail


FOOTER_RE = re.compile(r"\n---\n\n\[(?:← 上一类|返回总目录)[^\n]*\n?\Z")


def add_category_nav():
    ids = [c[0] for c in BEGINNER_CATEGORIES]
    for idx, cid in enumerate(ids):
        p = os.path.join(ROOT, cat_file(cid))
        text = FOOTER_RE.sub("\n", open(p, encoding="utf-8").read()).rstrip("\n") + "\n"
        parts = []
        if idx > 0:
            prev = ids[idx - 1]
            parts.append(f"[← 上一类：{CAT_NAME[prev]}]({os.path.basename(cat_file(prev))})")
        parts.append("[返回总目录](../../README.md)")
        if idx < len(ids) - 1:
            nxt = ids[idx + 1]
            parts.append(f"[下一类：{CAT_NAME[nxt]} →]({os.path.basename(cat_file(nxt))})")
        open(p, "w", encoding="utf-8").write(text + "\n---\n\n" + " · ".join(parts) + "\n")


def check_anchors():
    missing = []
    for r, n, _, cid in ITEMS:
        if cid and f'<a id="{anchor(n)}"></a>' not in open(os.path.join(ROOT, cat_file(cid)), encoding="utf-8").read():
            missing.append(n)
    if missing:
        sys.exit("以下指令在分类文件里找不到锚点：" + "、".join(missing))


def main():
    check_anchors()
    readme = os.path.join(ROOT, "README.md")
    s = open(readme, encoding="utf-8").read()
    s = replace_block(s, "<!-- 分类目录开始 -->", "<!-- 分类目录结束 -->", build_category_table())
    s = replace_block(s, "<!-- 总目录开始 -->", "<!-- 总目录结束 -->", build_index_table())
    open(readme, "w", encoding="utf-8").write(s)
    add_category_nav()


if __name__ == "__main__":
    main()
