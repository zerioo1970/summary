#!/usr/bin/env python3
# Ubuntu指令教程/tools/gen_readme.py
# 作用：按 300 个指令的排名，生成 README.md 的目录部分，并给每个指令页加上“上一个 / 下一个”导航。
# 用法（在 Ubuntu指令教程/ 目录下执行）：python3 tools/gen_readme.py
import os
import re

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# (排名, 指令, 用途)。初级 1–100、中级 101–200、高级 201–300
COMMANDS = """
ls|列出目录内容
cd|切换目录
pwd|显示当前目录
sudo|以管理员身份执行
cat|查看文件内容
cp|复制文件和目录
mv|移动或重命名
rm|删除文件和目录
mkdir|新建目录
touch|新建空文件或修改时间
nano|简易文本编辑器
apt|安装、升级、卸载软件
clear|清屏
echo|输出一行文本
man|查看命令手册
history|查看命令历史
exit|退出 Shell 或脚本
less|分页查看长文件
head|查看文件开头
tail|查看文件末尾、跟踪日志
grep|按关键词搜索文本
find|按条件查找文件
chmod|修改文件权限
chown|修改所有者和所属组
df|查看磁盘空间
du|查看目录占用的空间
free|查看内存
top|实时查看负载和进程
ps|查看进程列表
kill|结束进程
ping|测试网络连通
ip|查看 IP、网卡和路由
wget|下载文件
curl|发送 HTTP 请求
tar|打包和解包
zip|压缩成 zip
unzip|解压 zip
reboot|重启系统
shutdown|关机或定时重启
whoami|显示当前用户名
uname|查看内核和系统信息
date|显示和计算日期时间
cal|显示日历
which|查命令对应的程序位置
whereis|查程序和手册的位置
file|判断文件类型
wc|统计行数、字数
sort|排序
uniq|去重和计数
rmdir|删除空目录
ln|创建链接
more|简单的分页查看
passwd|修改密码
su|切换到其他用户
hostname|查看主机名和本机 IP
uptime|查看运行时长和负载
lsb_release|查看 Ubuntu 版本
vim|全功能的终端编辑器
tree|树状显示目录
alias|给命令起别名
type|查看命令的真实身份
help|bash 内建命令的帮助
apt-get|脚本里用的包管理命令
dpkg|安装和查询 .deb 包
snap|管理 Snap 软件包
systemctl|管理系统服务
lsblk|列出磁盘和分区
mount|挂载磁盘和分区
umount|卸载磁盘和分区
id|查看 UID 和所属组
groups|查看所属组
who|查看谁登录了系统
w|查看谁在线、在做什么
last|查看登录历史
env|查看环境变量
export|设置环境变量
source|在当前 Shell 里执行脚本
diff|比较文件差异
cut|按列截取文本
tee|同时输出到屏幕和文件
gzip|压缩单个文件
gunzip|解压 .gz 文件
ssh|远程登录服务器
scp|远程复制文件
htop|更好用的进程监控
fastfetch|一屏看清系统概况
sleep|暂停一段时间
watch|定时重复执行命令
time|统计命令执行时间
basename|从路径取文件名
dirname|从路径取目录
stat|查看文件详细属性
locate|按文件名快速查找
updatedb|更新 locate 的数据库
lscpu|查看 CPU 信息
lsusb|列出 USB 设备
lspci|列出 PCI 设备
nproc|显示 CPU 数量
killall|按程序名结束进程
xdg-open|用默认程序打开文件
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
ITEMS = [(i + 1, *line.split("|")) for i, line in enumerate(COMMANDS)]
LEVELS = [("beginner", "初级", 1, 100), ("intermediate", "中级", 101, 200), ("advanced", "高级", 201, 300)]


def page_path(level_dir, rank, name):
    # 文件名：三位编号-指令名.md；指令名里的空格换成连字符（docker compose → docker-compose）
    return f"docs/{level_dir}/{rank:03d}-{name.replace(' ', '-')}.md"


def existing(path):
    return os.path.exists(os.path.join(ROOT, path))


def build_tables():
    out = []
    for level_dir, level_cn, lo, hi in LEVELS:
        rows = [it for it in ITEMS if lo <= it[0] <= hi]
        done = sum(existing(page_path(level_dir, r, n)) for r, n, _ in rows)
        out.append(f"### {level_cn}指令（{lo}–{hi}）\n")
        if done == 0:
            out.append("📝 待写。\n")
        out.append("| 排名 | 指令 | 用途 | 状态 |")
        out.append("|---|---|---|---|")
        for r, n, use in rows:
            p = page_path(level_dir, r, n)
            if existing(p):
                out.append(f"| {r} | [`{n}`]({p}) | {use} | ✅ |")
            else:
                out.append(f"| {r} | `{n}` | {use} | 📝 |")
        out.append("")
    return "\n".join(out)


FOOTER_RE = re.compile(r"\n---\n\n\[(?:← 上一个|返回目录)[^\n]*\n?\Z")


def add_nav():
    for level_dir, _, lo, hi in LEVELS:
        rows = [it for it in ITEMS if lo <= it[0] <= hi and existing(page_path(level_dir, it[0], it[1]))]
        for idx, (r, n, _) in enumerate(rows):
            p = os.path.join(ROOT, page_path(level_dir, r, n))
            text = FOOTER_RE.sub("\n", open(p, encoding="utf-8").read()).rstrip("\n") + "\n"
            parts = []
            if idx > 0:
                pr, pn, _ = rows[idx - 1]
                parts.append(f"[← 上一个：{pn}]({os.path.basename(page_path(level_dir, pr, pn))})")
            parts.append("[返回目录](../../README.md)")
            if idx < len(rows) - 1:
                nr, nn, _ = rows[idx + 1]
                parts.append(f"[下一个：{nn} →]({os.path.basename(page_path(level_dir, nr, nn))})")
            text += "\n---\n\n" + " · ".join(parts) + "\n"
            open(p, "w", encoding="utf-8").write(text)


def main():
    readme = os.path.join(ROOT, "README.md")
    s = open(readme, encoding="utf-8").read()
    begin, end = "<!-- 目录开始：由 tools/gen_readme.py 生成，请勿手改 -->", "<!-- 目录结束 -->"
    head, rest = s.split(begin)
    _, tail = rest.split(end)
    open(readme, "w", encoding="utf-8").write(head + begin + "\n\n" + build_tables() + "\n" + end + tail)
    add_nav()


if __name__ == "__main__":
    main()
