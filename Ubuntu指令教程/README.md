# Ubuntu 常用指令教程（300 个）

> 版本基线：**Ubuntu 26.04.1 LTS**（代号 resolute）｜bash 5.3.9｜Rust 版 coreutils（uutils）0.10.0，其中 cp、mv、rm、df 由 GNU coreutils 9.7 提供｜sudo-rs｜apt 3.2.0｜systemd 259｜OpenSSH 10.2p1
>
> 按用途分类，**每类一个文件**，同一类的初级、中级、高级指令写在同一个文件里。星级表示难度：**★★★ 初级**、**★★ 中级**、**★ 高级**，目录和每个指令的标题里都有标注，如 `001 ★★★  ls：列出目录内容`。
>
> 每个指令的固定结构：作用 · 基本格式 · 常用参数（表格）· 常用示例（带实测输出）· ⚠️ 踩坑警告 · 一句话总结。简单的指令省略用不上的部分。

---

## 这本教程写给谁

- 会写程序、熟悉数据库，但没有系统用过 Linux 命令行的人。
- 要在 Ubuntu 服务器上部署、排查问题，需要一份“查得到、看得懂、照着能跑”的指令手册。
- 不解释什么是文件、什么是进程；重点讲每个参数**什么时候用**、**容易错在哪**。

## 怎么读

| 路径 | 内容 | 时间 |
|---|---|---|
| 最短路径 | 初级的 01 目录和文件操作、02 文件查看和编辑、06 软件包管理、08 进程管理 | 约 4 小时 |
| 完整路径 | 300 个指令，先读完每类的 ★★★，再读 ★★，最后 ★ | 每天 2 小时，约 6 周 |

排名按日常使用频率估算（没有官方统计数据），越靠前越常用。遇到具体问题时，按“分类目录”找到对应的类别，或在“总目录对应表”里按指令名直接跳转，不必按顺序读。

## 演示环境

所有示例都以普通用户 `tom` 的身份，在演示目录 `~/demo`（即 `/home/tom/demo`）下执行，需要管理员权限的会注明。演示目录的内容：

| 文件 | 说明 |
|---|---|
| `readme.txt` | 内容为 `hello`，修改时间 2026-10-01 |
| `big.iso` | 2.4 MB 的大文件，修改时间 2026-10-20 |
| `run.sh` | 可执行脚本 |
| `.env` | 隐藏文件 |
| `link.txt` | 指向 readme.txt 的符号链接 |
| `logs/app.log` | 8 行的应用日志，见下 |
| `src/products.csv` | 5 条产品数据，见下 |

`logs/app.log`：

```
2026-10-28 09:00:01 INFO  服务启动
2026-10-28 09:00:05 INFO  连接数据库成功
2026-10-28 09:15:22 WARN  价格表缓存过期，重新加载
2026-10-28 10:02:13 ERROR 订单 A1002 保存失败：超时
2026-10-28 10:02:14 INFO  重试订单 A1002
2026-10-28 10:02:15 INFO  订单 A1002 保存成功
2026-10-28 11:30:40 ERROR 连接数据库失败
2026-10-28 11:30:45 INFO  连接数据库成功
```

`src/products.csv`：

```
code,name,category,price
U001,探头A,探头,12000
U002,探头B,探头,15000
U003,主机X,主机,280000
U004,凝胶,耗材,35
U005,探头A,探头,12000
```

中级的部分示例需要其他条件，页面里会注明：分区、格式化类的示例用镜像文件（如 `disk.img`）代替真实磁盘，在 `/root` 下以 root 身份执行；端口、防火墙类的示例在只有回环网卡的独立网络里执行；ssh 相关的示例在本机 2222 端口运行一个 sshd。

建立演示环境的脚本是 [`tools/setup-demo.sh`](tools/setup-demo.sh)，在自己的 Ubuntu 上可以用它建一份一样的目录来练习（脚本会创建用户 tom，请在测试机上执行）。

## 输出是怎么来的

- 示例输出在 **Ubuntu 26.04.1 LTS 官方根文件系统**（ubuntu-base）中实际执行得到，执行的命令清单保存在 [`tools/cmds/`](tools/cmds/)。
- 测试环境是运行在云服务器上的容器，所以 `df`、`free`、`top`、`ip`、`lsblk`、`uname` 这类显示系统信息的指令，其设备名、容量、内核版本会和你的机器不同，页面里已注明。
- 宿主机启用了 SELinux，导致 `ls -l` 的权限后面多出一个 `+`，目录大小也和 ext4 不同。文中已去掉 `+`，并把目录大小统一为 ext4 上的 `4096`，以符合普通 Ubuntu 机器上看到的样子。
- 需要图形界面、完整 systemd 或真实终端交互的内容（如 nano、vim 的按键，systemctl start，reboot）无法在测试环境里执行，这些地方都标注了 `⚠️ 未实机验证`，写法已对照该版本的手册核实。

## Ubuntu 26.04 和旧版本的几个差别

| 变化 | 影响 | 详见 |
|---|---|---|
| 基础命令（ls、cat、mkdir、chmod、chown 等）改由 Rust 重写的 uutils 提供 | 常用参数相同，部分报错文字和 GNU 版不同；个别行为有差异 | [chown](docs/05-用户和权限.md#cmd-chown)、[mkdir](docs/01-目录和文件操作.md#cmd-mkdir) |
| `sudo` 改由 sudo-rs 提供 | 输入密码时显示 `*`；没有 `-E` 参数 | [sudo](docs/05-用户和权限.md#cmd-sudo) |
| apt 3.x 的输出改为分块显示 | 安装前的摘要更清楚 | [apt](docs/06-软件包管理.md#cmd-apt) |
| ssh 默认由 `ssh.socket` 按需启动 | 改端口的步骤和以前不同 | [systemctl](docs/09-系统服务和开关机.md#cmd-systemctl) |
| 登录记录改由 wtmpdb 保存 | `last` 读的是 `/var/log/wtmp.db` | [last](docs/05-用户和权限.md#cmd-last) |
| `awk` 默认是 mawk，不是 gawk | gawk 特有的函数不能用 | [awk](docs/03-文本处理.md#cmd-awk) |
| `visudo` 也由 sudo-rs 提供 | sudoers.d 里含 `.` 的文件名会被跳过 | [visudo](docs/05-用户和权限.md#cmd-visudo) |
| 7-Zip 改用官方的 `7zip` 包 | 命令是 `7z`，不再是 p7zip 的 `7za` | [7z](docs/04-压缩和打包.md#cmd-7z) |

## 进度

| 部分 | 状态 |
|---|---|
| **初级指令 001–100** | ✅ |
| **中级指令 101–200** | ✅ |
| 高级指令 201–300 | 📝 |

## 分类目录（15 类）

<!-- 分类目录开始 -->

| 分类 | ★★★ 初级 | ★★ 中级 | ★ 高级 |
|---|---|---|---|
| [01 目录和文件操作](docs/01-目录和文件操作.md)<br/>浏览目录，复制、移动、删除、查找、校验文件 | [ls](docs/01-目录和文件操作.md#cmd-ls)、[cd](docs/01-目录和文件操作.md#cmd-cd)、[pwd](docs/01-目录和文件操作.md#cmd-pwd)、[cp](docs/01-目录和文件操作.md#cmd-cp)、[mv](docs/01-目录和文件操作.md#cmd-mv)、[rm](docs/01-目录和文件操作.md#cmd-rm)、[mkdir](docs/01-目录和文件操作.md#cmd-mkdir)、[touch](docs/01-目录和文件操作.md#cmd-touch)、[find](docs/01-目录和文件操作.md#cmd-find)、[file](docs/01-目录和文件操作.md#cmd-file)、[rmdir](docs/01-目录和文件操作.md#cmd-rmdir)、[ln](docs/01-目录和文件操作.md#cmd-ln)、[tree](docs/01-目录和文件操作.md#cmd-tree)、[basename](docs/01-目录和文件操作.md#cmd-basename)、[dirname](docs/01-目录和文件操作.md#cmd-dirname)、[stat](docs/01-目录和文件操作.md#cmd-stat)、[locate](docs/01-目录和文件操作.md#cmd-locate)、[updatedb](docs/01-目录和文件操作.md#cmd-updatedb) | [md5sum](docs/01-目录和文件操作.md#cmd-md5sum)、[sha256sum](docs/01-目录和文件操作.md#cmd-sha256sum)、[split](docs/01-目录和文件操作.md#cmd-split)、[dd](docs/01-目录和文件操作.md#cmd-dd)、[sync](docs/01-目录和文件操作.md#cmd-sync) | — |
| [02 文件查看和编辑](docs/02-文件查看和编辑.md)<br/>查看文件内容，在终端里编辑文件 | [cat](docs/02-文件查看和编辑.md#cmd-cat)、[nano](docs/02-文件查看和编辑.md#cmd-nano)、[less](docs/02-文件查看和编辑.md#cmd-less)、[head](docs/02-文件查看和编辑.md#cmd-head)、[tail](docs/02-文件查看和编辑.md#cmd-tail)、[more](docs/02-文件查看和编辑.md#cmd-more)、[vim](docs/02-文件查看和编辑.md#cmd-vim) | — | — |
| [03 文本处理](docs/03-文本处理.md)<br/>搜索、替换、统计、排序、截取、拼接文本 | [echo](docs/03-文本处理.md#cmd-echo)、[grep](docs/03-文本处理.md#cmd-grep)、[wc](docs/03-文本处理.md#cmd-wc)、[sort](docs/03-文本处理.md#cmd-sort)、[uniq](docs/03-文本处理.md#cmd-uniq)、[diff](docs/03-文本处理.md#cmd-diff)、[cut](docs/03-文本处理.md#cmd-cut)、[tee](docs/03-文本处理.md#cmd-tee) | [sed](docs/03-文本处理.md#cmd-sed)、[awk](docs/03-文本处理.md#cmd-awk)、[xargs](docs/03-文本处理.md#cmd-xargs)、[tr](docs/03-文本处理.md#cmd-tr)、[base64](docs/03-文本处理.md#cmd-base64)、[column](docs/03-文本处理.md#cmd-column)、[paste](docs/03-文本处理.md#cmd-paste)、[join](docs/03-文本处理.md#cmd-join)、[comm](docs/03-文本处理.md#cmd-comm)、[nl](docs/03-文本处理.md#cmd-nl)、[rev](docs/03-文本处理.md#cmd-rev)、[tac](docs/03-文本处理.md#cmd-tac)、[printf](docs/03-文本处理.md#cmd-printf)、[seq](docs/03-文本处理.md#cmd-seq) | — |
| [04 压缩和打包](docs/04-压缩和打包.md)<br/>tar、zip、gzip、xz、bzip2、7z 的打包和解包 | [tar](docs/04-压缩和打包.md#cmd-tar)、[zip](docs/04-压缩和打包.md#cmd-zip)、[unzip](docs/04-压缩和打包.md#cmd-unzip)、[gzip](docs/04-压缩和打包.md#cmd-gzip)、[gunzip](docs/04-压缩和打包.md#cmd-gunzip) | [xz](docs/04-压缩和打包.md#cmd-xz)、[bzip2](docs/04-压缩和打包.md#cmd-bzip2)、[7z](docs/04-压缩和打包.md#cmd-7z)、[zcat](docs/04-压缩和打包.md#cmd-zcat)、[zgrep](docs/04-压缩和打包.md#cmd-zgrep) | — |
| [05 用户和权限](docs/05-用户和权限.md)<br/>建删用户和组、提权、改密码、改权限、查登录记录 | [sudo](docs/05-用户和权限.md#cmd-sudo)、[chmod](docs/05-用户和权限.md#cmd-chmod)、[chown](docs/05-用户和权限.md#cmd-chown)、[whoami](docs/05-用户和权限.md#cmd-whoami)、[passwd](docs/05-用户和权限.md#cmd-passwd)、[su](docs/05-用户和权限.md#cmd-su)、[id](docs/05-用户和权限.md#cmd-id)、[groups](docs/05-用户和权限.md#cmd-groups)、[who](docs/05-用户和权限.md#cmd-who)、[w](docs/05-用户和权限.md#cmd-w)、[last](docs/05-用户和权限.md#cmd-last) | [useradd](docs/05-用户和权限.md#cmd-useradd)、[adduser](docs/05-用户和权限.md#cmd-adduser)、[usermod](docs/05-用户和权限.md#cmd-usermod)、[userdel](docs/05-用户和权限.md#cmd-userdel)、[deluser](docs/05-用户和权限.md#cmd-deluser)、[groupadd](docs/05-用户和权限.md#cmd-groupadd)、[visudo](docs/05-用户和权限.md#cmd-visudo)、[chgrp](docs/05-用户和权限.md#cmd-chgrp)、[umask](docs/05-用户和权限.md#cmd-umask)、[loginctl](docs/05-用户和权限.md#cmd-loginctl)、[chage](docs/05-用户和权限.md#cmd-chage)、[getent](docs/05-用户和权限.md#cmd-getent)、[sudoedit](docs/05-用户和权限.md#cmd-sudoedit) | — |
| [06 软件包管理](docs/06-软件包管理.md)<br/>apt、dpkg、snap、flatpak，软件源和系统升级 | [apt](docs/06-软件包管理.md#cmd-apt)、[apt-get](docs/06-软件包管理.md#cmd-apt-get)、[dpkg](docs/06-软件包管理.md#cmd-dpkg)、[snap](docs/06-软件包管理.md#cmd-snap) | [apt-cache](docs/06-软件包管理.md#cmd-apt-cache)、[apt-mark](docs/06-软件包管理.md#cmd-apt-mark)、[add-apt-repository](docs/06-软件包管理.md#cmd-add-apt-repository)、[update-alternatives](docs/06-软件包管理.md#cmd-update-alternatives)、[do-release-upgrade](docs/06-软件包管理.md#cmd-do-release-upgrade)、[unattended-upgrade](docs/06-软件包管理.md#cmd-unattended-upgrade)、[flatpak](docs/06-软件包管理.md#cmd-flatpak) | — |
| [07 磁盘管理](docs/07-磁盘管理.md)<br/>看空间、分区、格式化、挂载、交换区 | [df](docs/07-磁盘管理.md#cmd-df)、[du](docs/07-磁盘管理.md#cmd-du)、[lsblk](docs/07-磁盘管理.md#cmd-lsblk)、[mount](docs/07-磁盘管理.md#cmd-mount)、[umount](docs/07-磁盘管理.md#cmd-umount) | [fdisk](docs/07-磁盘管理.md#cmd-fdisk)、[parted](docs/07-磁盘管理.md#cmd-parted)、[mkfs](docs/07-磁盘管理.md#cmd-mkfs)、[blkid](docs/07-磁盘管理.md#cmd-blkid)、[fsck](docs/07-磁盘管理.md#cmd-fsck)、[swapon](docs/07-磁盘管理.md#cmd-swapon)、[mkswap](docs/07-磁盘管理.md#cmd-mkswap)、[findmnt](docs/07-磁盘管理.md#cmd-findmnt)、[ncdu](docs/07-磁盘管理.md#cmd-ncdu) | — |
| [08 进程管理](docs/08-进程管理.md)<br/>查看、结束、调度进程，前后台作业 | [top](docs/08-进程管理.md#cmd-top)、[ps](docs/08-进程管理.md#cmd-ps)、[kill](docs/08-进程管理.md#cmd-kill)、[htop](docs/08-进程管理.md#cmd-htop)、[sleep](docs/08-进程管理.md#cmd-sleep)、[watch](docs/08-进程管理.md#cmd-watch)、[time](docs/08-进程管理.md#cmd-time)、[killall](docs/08-进程管理.md#cmd-killall) | [pgrep](docs/08-进程管理.md#cmd-pgrep)、[pkill](docs/08-进程管理.md#cmd-pkill)、[nice](docs/08-进程管理.md#cmd-nice)、[renice](docs/08-进程管理.md#cmd-renice)、[nohup](docs/08-进程管理.md#cmd-nohup)、[bg](docs/08-进程管理.md#cmd-bg)、[fg](docs/08-进程管理.md#cmd-fg)、[jobs](docs/08-进程管理.md#cmd-jobs)、[disown](docs/08-进程管理.md#cmd-disown)、[lsof](docs/08-进程管理.md#cmd-lsof) | — |
| [09 系统服务和开关机](docs/09-系统服务和开关机.md)<br/>systemctl 管服务，看日志，开机分析，重启和关机 | [reboot](docs/09-系统服务和开关机.md#cmd-reboot)、[shutdown](docs/09-系统服务和开关机.md#cmd-shutdown)、[systemctl](docs/09-系统服务和开关机.md#cmd-systemctl) | [journalctl](docs/09-系统服务和开关机.md#cmd-journalctl)、[systemd-analyze](docs/09-系统服务和开关机.md#cmd-systemd-analyze) | — |
| [10 网络和远程连接](docs/10-网络和远程连接.md)<br/>IP、DNS、端口、防火墙、下载、ssh 远程和同步 | [ping](docs/10-网络和远程连接.md#cmd-ping)、[ip](docs/10-网络和远程连接.md#cmd-ip)、[wget](docs/10-网络和远程连接.md#cmd-wget)、[curl](docs/10-网络和远程连接.md#cmd-curl)、[hostname](docs/10-网络和远程连接.md#cmd-hostname)、[ssh](docs/10-网络和远程连接.md#cmd-ssh)、[scp](docs/10-网络和远程连接.md#cmd-scp) | [ufw](docs/10-网络和远程连接.md#cmd-ufw)、[rsync](docs/10-网络和远程连接.md#cmd-rsync)、[ss](docs/10-网络和远程连接.md#cmd-ss)、[netstat](docs/10-网络和远程连接.md#cmd-netstat)、[dig](docs/10-网络和远程连接.md#cmd-dig)、[nslookup](docs/10-网络和远程连接.md#cmd-nslookup)、[traceroute](docs/10-网络和远程连接.md#cmd-traceroute)、[tracepath](docs/10-网络和远程连接.md#cmd-tracepath)、[nmcli](docs/10-网络和远程连接.md#cmd-nmcli)、[netplan](docs/10-网络和远程连接.md#cmd-netplan)、[resolvectl](docs/10-网络和远程连接.md#cmd-resolvectl)、[ssh-keygen](docs/10-网络和远程连接.md#cmd-ssh-keygen)、[ssh-copy-id](docs/10-网络和远程连接.md#cmd-ssh-copy-id)、[sftp](docs/10-网络和远程连接.md#cmd-sftp)、[iftop](docs/10-网络和远程连接.md#cmd-iftop)、[ssh-agent](docs/10-网络和远程连接.md#cmd-ssh-agent) | — |
| [11 系统信息和硬件](docs/11-系统信息和硬件.md)<br/>版本、内核、时间时区、CPU、内存、硬件和内核模块 | [free](docs/11-系统信息和硬件.md#cmd-free)、[uname](docs/11-系统信息和硬件.md#cmd-uname)、[date](docs/11-系统信息和硬件.md#cmd-date)、[cal](docs/11-系统信息和硬件.md#cmd-cal)、[uptime](docs/11-系统信息和硬件.md#cmd-uptime)、[lsb_release](docs/11-系统信息和硬件.md#cmd-lsb_release)、[fastfetch](docs/11-系统信息和硬件.md#cmd-fastfetch)、[lscpu](docs/11-系统信息和硬件.md#cmd-lscpu)、[lsusb](docs/11-系统信息和硬件.md#cmd-lsusb)、[lspci](docs/11-系统信息和硬件.md#cmd-lspci)、[nproc](docs/11-系统信息和硬件.md#cmd-nproc) | [hostnamectl](docs/11-系统信息和硬件.md#cmd-hostnamectl)、[timedatectl](docs/11-系统信息和硬件.md#cmd-timedatectl)、[localectl](docs/11-系统信息和硬件.md#cmd-localectl)、[dmesg](docs/11-系统信息和硬件.md#cmd-dmesg)、[lsmod](docs/11-系统信息和硬件.md#cmd-lsmod)、[modprobe](docs/11-系统信息和硬件.md#cmd-modprobe) | — |
| [12 Shell使用](docs/12-Shell使用.md)<br/>历史、别名、环境变量、帮助、脚本基础、终端复用 | [clear](docs/12-Shell使用.md#cmd-clear)、[man](docs/12-Shell使用.md#cmd-man)、[history](docs/12-Shell使用.md#cmd-history)、[exit](docs/12-Shell使用.md#cmd-exit)、[which](docs/12-Shell使用.md#cmd-which)、[whereis](docs/12-Shell使用.md#cmd-whereis)、[alias](docs/12-Shell使用.md#cmd-alias)、[type](docs/12-Shell使用.md#cmd-type)、[help](docs/12-Shell使用.md#cmd-help)、[env](docs/12-Shell使用.md#cmd-env)、[export](docs/12-Shell使用.md#cmd-export)、[source](docs/12-Shell使用.md#cmd-source)、[xdg-open](docs/12-Shell使用.md#cmd-xdg-open) | [screen](docs/12-Shell使用.md#cmd-screen)、[tmux](docs/12-Shell使用.md#cmd-tmux)、[read](docs/12-Shell使用.md#cmd-read)、[test](docs/12-Shell使用.md#cmd-test)、[bc](docs/12-Shell使用.md#cmd-bc)、[expr](docs/12-Shell使用.md#cmd-expr) | — |
| [13 性能监控](docs/13-性能监控.md)<br/>CPU、内存、磁盘 IO 的统计和监控 | — | [vmstat](docs/13-性能监控.md#cmd-vmstat)、[iostat](docs/13-性能监控.md#cmd-iostat)、[iotop](docs/13-性能监控.md#cmd-iotop) | — |
| [14 定时任务](docs/14-定时任务.md)<br/>周期执行和一次性执行 | — | [crontab](docs/14-定时任务.md#cmd-crontab)、[at](docs/14-定时任务.md#cmd-at) | — |
| [15 开发和容器](docs/15-开发和容器.md)<br/>版本控制和容器 | — | [git](docs/15-开发和容器.md#cmd-git)、[docker](docs/15-开发和容器.md#cmd-docker) | — |

<!-- 分类目录结束 -->

## 总目录对应表（300 个）

按使用频率排名。点指令名直接跳到它所在分类文件中的位置；高级指令写完后再归入分类。

<!-- 总目录开始 -->

### ★★★ 初级指令（1–100）

| 排名 | 级别 | 指令 | 用途 | 所在分类 | 状态 |
|---|---|---|---|---|---|
| 1 | ★★★ | [`ls`](docs/01-目录和文件操作.md#cmd-ls) | 列出目录内容 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 2 | ★★★ | [`cd`](docs/01-目录和文件操作.md#cmd-cd) | 切换目录 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 3 | ★★★ | [`pwd`](docs/01-目录和文件操作.md#cmd-pwd) | 显示当前目录 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 4 | ★★★ | [`sudo`](docs/05-用户和权限.md#cmd-sudo) | 以管理员身份执行 | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 5 | ★★★ | [`cat`](docs/02-文件查看和编辑.md#cmd-cat) | 查看文件内容 | [02 文件查看和编辑](docs/02-文件查看和编辑.md) | ✅ |
| 6 | ★★★ | [`cp`](docs/01-目录和文件操作.md#cmd-cp) | 复制文件和目录 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 7 | ★★★ | [`mv`](docs/01-目录和文件操作.md#cmd-mv) | 移动或重命名 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 8 | ★★★ | [`rm`](docs/01-目录和文件操作.md#cmd-rm) | 删除文件和目录 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 9 | ★★★ | [`mkdir`](docs/01-目录和文件操作.md#cmd-mkdir) | 新建目录 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 10 | ★★★ | [`touch`](docs/01-目录和文件操作.md#cmd-touch) | 新建空文件或修改时间 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 11 | ★★★ | [`nano`](docs/02-文件查看和编辑.md#cmd-nano) | 简易文本编辑器 | [02 文件查看和编辑](docs/02-文件查看和编辑.md) | ✅ |
| 12 | ★★★ | [`apt`](docs/06-软件包管理.md#cmd-apt) | 安装、升级、卸载软件 | [06 软件包管理](docs/06-软件包管理.md) | ✅ |
| 13 | ★★★ | [`clear`](docs/12-Shell使用.md#cmd-clear) | 清屏 | [12 Shell使用](docs/12-Shell使用.md) | ✅ |
| 14 | ★★★ | [`echo`](docs/03-文本处理.md#cmd-echo) | 输出一行文本 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 15 | ★★★ | [`man`](docs/12-Shell使用.md#cmd-man) | 查看命令手册 | [12 Shell使用](docs/12-Shell使用.md) | ✅ |
| 16 | ★★★ | [`history`](docs/12-Shell使用.md#cmd-history) | 查看命令历史 | [12 Shell使用](docs/12-Shell使用.md) | ✅ |
| 17 | ★★★ | [`exit`](docs/12-Shell使用.md#cmd-exit) | 退出 Shell 或脚本 | [12 Shell使用](docs/12-Shell使用.md) | ✅ |
| 18 | ★★★ | [`less`](docs/02-文件查看和编辑.md#cmd-less) | 分页查看长文件 | [02 文件查看和编辑](docs/02-文件查看和编辑.md) | ✅ |
| 19 | ★★★ | [`head`](docs/02-文件查看和编辑.md#cmd-head) | 查看文件开头 | [02 文件查看和编辑](docs/02-文件查看和编辑.md) | ✅ |
| 20 | ★★★ | [`tail`](docs/02-文件查看和编辑.md#cmd-tail) | 查看文件末尾、跟踪日志 | [02 文件查看和编辑](docs/02-文件查看和编辑.md) | ✅ |
| 21 | ★★★ | [`grep`](docs/03-文本处理.md#cmd-grep) | 按关键词搜索文本 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 22 | ★★★ | [`find`](docs/01-目录和文件操作.md#cmd-find) | 按条件查找文件 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 23 | ★★★ | [`chmod`](docs/05-用户和权限.md#cmd-chmod) | 修改文件权限 | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 24 | ★★★ | [`chown`](docs/05-用户和权限.md#cmd-chown) | 修改所有者和所属组 | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 25 | ★★★ | [`df`](docs/07-磁盘管理.md#cmd-df) | 查看磁盘空间 | [07 磁盘管理](docs/07-磁盘管理.md) | ✅ |
| 26 | ★★★ | [`du`](docs/07-磁盘管理.md#cmd-du) | 查看目录占用的空间 | [07 磁盘管理](docs/07-磁盘管理.md) | ✅ |
| 27 | ★★★ | [`free`](docs/11-系统信息和硬件.md#cmd-free) | 查看内存 | [11 系统信息和硬件](docs/11-系统信息和硬件.md) | ✅ |
| 28 | ★★★ | [`top`](docs/08-进程管理.md#cmd-top) | 实时查看负载和进程 | [08 进程管理](docs/08-进程管理.md) | ✅ |
| 29 | ★★★ | [`ps`](docs/08-进程管理.md#cmd-ps) | 查看进程列表 | [08 进程管理](docs/08-进程管理.md) | ✅ |
| 30 | ★★★ | [`kill`](docs/08-进程管理.md#cmd-kill) | 结束进程 | [08 进程管理](docs/08-进程管理.md) | ✅ |
| 31 | ★★★ | [`ping`](docs/10-网络和远程连接.md#cmd-ping) | 测试网络连通 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 32 | ★★★ | [`ip`](docs/10-网络和远程连接.md#cmd-ip) | 查看 IP、网卡和路由 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 33 | ★★★ | [`wget`](docs/10-网络和远程连接.md#cmd-wget) | 下载文件 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 34 | ★★★ | [`curl`](docs/10-网络和远程连接.md#cmd-curl) | 发送 HTTP 请求 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 35 | ★★★ | [`tar`](docs/04-压缩和打包.md#cmd-tar) | 打包和解包 | [04 压缩和打包](docs/04-压缩和打包.md) | ✅ |
| 36 | ★★★ | [`zip`](docs/04-压缩和打包.md#cmd-zip) | 压缩成 zip | [04 压缩和打包](docs/04-压缩和打包.md) | ✅ |
| 37 | ★★★ | [`unzip`](docs/04-压缩和打包.md#cmd-unzip) | 解压 zip | [04 压缩和打包](docs/04-压缩和打包.md) | ✅ |
| 38 | ★★★ | [`reboot`](docs/09-系统服务和开关机.md#cmd-reboot) | 重启系统 | [09 系统服务和开关机](docs/09-系统服务和开关机.md) | ✅ |
| 39 | ★★★ | [`shutdown`](docs/09-系统服务和开关机.md#cmd-shutdown) | 关机或定时重启 | [09 系统服务和开关机](docs/09-系统服务和开关机.md) | ✅ |
| 40 | ★★★ | [`whoami`](docs/05-用户和权限.md#cmd-whoami) | 显示当前用户名 | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 41 | ★★★ | [`uname`](docs/11-系统信息和硬件.md#cmd-uname) | 查看内核和系统信息 | [11 系统信息和硬件](docs/11-系统信息和硬件.md) | ✅ |
| 42 | ★★★ | [`date`](docs/11-系统信息和硬件.md#cmd-date) | 显示和计算日期时间 | [11 系统信息和硬件](docs/11-系统信息和硬件.md) | ✅ |
| 43 | ★★★ | [`cal`](docs/11-系统信息和硬件.md#cmd-cal) | 显示日历 | [11 系统信息和硬件](docs/11-系统信息和硬件.md) | ✅ |
| 44 | ★★★ | [`which`](docs/12-Shell使用.md#cmd-which) | 查命令对应的程序位置 | [12 Shell使用](docs/12-Shell使用.md) | ✅ |
| 45 | ★★★ | [`whereis`](docs/12-Shell使用.md#cmd-whereis) | 查程序和手册的位置 | [12 Shell使用](docs/12-Shell使用.md) | ✅ |
| 46 | ★★★ | [`file`](docs/01-目录和文件操作.md#cmd-file) | 判断文件类型 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 47 | ★★★ | [`wc`](docs/03-文本处理.md#cmd-wc) | 统计行数、字数 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 48 | ★★★ | [`sort`](docs/03-文本处理.md#cmd-sort) | 排序 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 49 | ★★★ | [`uniq`](docs/03-文本处理.md#cmd-uniq) | 去重和计数 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 50 | ★★★ | [`rmdir`](docs/01-目录和文件操作.md#cmd-rmdir) | 删除空目录 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 51 | ★★★ | [`ln`](docs/01-目录和文件操作.md#cmd-ln) | 创建链接 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 52 | ★★★ | [`more`](docs/02-文件查看和编辑.md#cmd-more) | 简单的分页查看 | [02 文件查看和编辑](docs/02-文件查看和编辑.md) | ✅ |
| 53 | ★★★ | [`passwd`](docs/05-用户和权限.md#cmd-passwd) | 修改密码 | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 54 | ★★★ | [`su`](docs/05-用户和权限.md#cmd-su) | 切换到其他用户 | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 55 | ★★★ | [`hostname`](docs/10-网络和远程连接.md#cmd-hostname) | 查看主机名和本机 IP | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 56 | ★★★ | [`uptime`](docs/11-系统信息和硬件.md#cmd-uptime) | 查看运行时长和负载 | [11 系统信息和硬件](docs/11-系统信息和硬件.md) | ✅ |
| 57 | ★★★ | [`lsb_release`](docs/11-系统信息和硬件.md#cmd-lsb_release) | 查看 Ubuntu 版本 | [11 系统信息和硬件](docs/11-系统信息和硬件.md) | ✅ |
| 58 | ★★★ | [`vim`](docs/02-文件查看和编辑.md#cmd-vim) | 全功能的终端编辑器 | [02 文件查看和编辑](docs/02-文件查看和编辑.md) | ✅ |
| 59 | ★★★ | [`tree`](docs/01-目录和文件操作.md#cmd-tree) | 树状显示目录 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 60 | ★★★ | [`alias`](docs/12-Shell使用.md#cmd-alias) | 给命令起别名 | [12 Shell使用](docs/12-Shell使用.md) | ✅ |
| 61 | ★★★ | [`type`](docs/12-Shell使用.md#cmd-type) | 查看命令的真实身份 | [12 Shell使用](docs/12-Shell使用.md) | ✅ |
| 62 | ★★★ | [`help`](docs/12-Shell使用.md#cmd-help) | bash 内建命令的帮助 | [12 Shell使用](docs/12-Shell使用.md) | ✅ |
| 63 | ★★★ | [`apt-get`](docs/06-软件包管理.md#cmd-apt-get) | 脚本里用的包管理命令 | [06 软件包管理](docs/06-软件包管理.md) | ✅ |
| 64 | ★★★ | [`dpkg`](docs/06-软件包管理.md#cmd-dpkg) | 安装和查询 .deb 包 | [06 软件包管理](docs/06-软件包管理.md) | ✅ |
| 65 | ★★★ | [`snap`](docs/06-软件包管理.md#cmd-snap) | 管理 Snap 软件包 | [06 软件包管理](docs/06-软件包管理.md) | ✅ |
| 66 | ★★★ | [`systemctl`](docs/09-系统服务和开关机.md#cmd-systemctl) | 管理系统服务 | [09 系统服务和开关机](docs/09-系统服务和开关机.md) | ✅ |
| 67 | ★★★ | [`lsblk`](docs/07-磁盘管理.md#cmd-lsblk) | 列出磁盘和分区 | [07 磁盘管理](docs/07-磁盘管理.md) | ✅ |
| 68 | ★★★ | [`mount`](docs/07-磁盘管理.md#cmd-mount) | 挂载磁盘和分区 | [07 磁盘管理](docs/07-磁盘管理.md) | ✅ |
| 69 | ★★★ | [`umount`](docs/07-磁盘管理.md#cmd-umount) | 卸载磁盘和分区 | [07 磁盘管理](docs/07-磁盘管理.md) | ✅ |
| 70 | ★★★ | [`id`](docs/05-用户和权限.md#cmd-id) | 查看 UID 和所属组 | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 71 | ★★★ | [`groups`](docs/05-用户和权限.md#cmd-groups) | 查看所属组 | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 72 | ★★★ | [`who`](docs/05-用户和权限.md#cmd-who) | 查看谁登录了系统 | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 73 | ★★★ | [`w`](docs/05-用户和权限.md#cmd-w) | 查看谁在线、在做什么 | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 74 | ★★★ | [`last`](docs/05-用户和权限.md#cmd-last) | 查看登录历史 | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 75 | ★★★ | [`env`](docs/12-Shell使用.md#cmd-env) | 查看环境变量 | [12 Shell使用](docs/12-Shell使用.md) | ✅ |
| 76 | ★★★ | [`export`](docs/12-Shell使用.md#cmd-export) | 设置环境变量 | [12 Shell使用](docs/12-Shell使用.md) | ✅ |
| 77 | ★★★ | [`source`](docs/12-Shell使用.md#cmd-source) | 在当前 Shell 里执行脚本 | [12 Shell使用](docs/12-Shell使用.md) | ✅ |
| 78 | ★★★ | [`diff`](docs/03-文本处理.md#cmd-diff) | 比较文件差异 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 79 | ★★★ | [`cut`](docs/03-文本处理.md#cmd-cut) | 按列截取文本 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 80 | ★★★ | [`tee`](docs/03-文本处理.md#cmd-tee) | 同时输出到屏幕和文件 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 81 | ★★★ | [`gzip`](docs/04-压缩和打包.md#cmd-gzip) | 压缩单个文件 | [04 压缩和打包](docs/04-压缩和打包.md) | ✅ |
| 82 | ★★★ | [`gunzip`](docs/04-压缩和打包.md#cmd-gunzip) | 解压 .gz 文件 | [04 压缩和打包](docs/04-压缩和打包.md) | ✅ |
| 83 | ★★★ | [`ssh`](docs/10-网络和远程连接.md#cmd-ssh) | 远程登录服务器 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 84 | ★★★ | [`scp`](docs/10-网络和远程连接.md#cmd-scp) | 远程复制文件 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 85 | ★★★ | [`htop`](docs/08-进程管理.md#cmd-htop) | 更好用的进程监控 | [08 进程管理](docs/08-进程管理.md) | ✅ |
| 86 | ★★★ | [`fastfetch`](docs/11-系统信息和硬件.md#cmd-fastfetch) | 一屏看清系统概况 | [11 系统信息和硬件](docs/11-系统信息和硬件.md) | ✅ |
| 87 | ★★★ | [`sleep`](docs/08-进程管理.md#cmd-sleep) | 暂停一段时间 | [08 进程管理](docs/08-进程管理.md) | ✅ |
| 88 | ★★★ | [`watch`](docs/08-进程管理.md#cmd-watch) | 定时重复执行命令 | [08 进程管理](docs/08-进程管理.md) | ✅ |
| 89 | ★★★ | [`time`](docs/08-进程管理.md#cmd-time) | 统计命令执行时间 | [08 进程管理](docs/08-进程管理.md) | ✅ |
| 90 | ★★★ | [`basename`](docs/01-目录和文件操作.md#cmd-basename) | 从路径取文件名 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 91 | ★★★ | [`dirname`](docs/01-目录和文件操作.md#cmd-dirname) | 从路径取目录 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 92 | ★★★ | [`stat`](docs/01-目录和文件操作.md#cmd-stat) | 查看文件详细属性 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 93 | ★★★ | [`locate`](docs/01-目录和文件操作.md#cmd-locate) | 按文件名快速查找 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 94 | ★★★ | [`updatedb`](docs/01-目录和文件操作.md#cmd-updatedb) | 更新 locate 的数据库 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 95 | ★★★ | [`lscpu`](docs/11-系统信息和硬件.md#cmd-lscpu) | 查看 CPU 信息 | [11 系统信息和硬件](docs/11-系统信息和硬件.md) | ✅ |
| 96 | ★★★ | [`lsusb`](docs/11-系统信息和硬件.md#cmd-lsusb) | 列出 USB 设备 | [11 系统信息和硬件](docs/11-系统信息和硬件.md) | ✅ |
| 97 | ★★★ | [`lspci`](docs/11-系统信息和硬件.md#cmd-lspci) | 列出 PCI 设备 | [11 系统信息和硬件](docs/11-系统信息和硬件.md) | ✅ |
| 98 | ★★★ | [`nproc`](docs/11-系统信息和硬件.md#cmd-nproc) | 显示 CPU 数量 | [11 系统信息和硬件](docs/11-系统信息和硬件.md) | ✅ |
| 99 | ★★★ | [`killall`](docs/08-进程管理.md#cmd-killall) | 按程序名结束进程 | [08 进程管理](docs/08-进程管理.md) | ✅ |
| 100 | ★★★ | [`xdg-open`](docs/12-Shell使用.md#cmd-xdg-open) | 用默认程序打开文件 | [12 Shell使用](docs/12-Shell使用.md) | ✅ |

### ★★ 中级指令（101–200）

| 排名 | 级别 | 指令 | 用途 | 所在分类 | 状态 |
|---|---|---|---|---|---|
| 101 | ★★ | [`journalctl`](docs/09-系统服务和开关机.md#cmd-journalctl) | 查看 systemd 日志 | [09 系统服务和开关机](docs/09-系统服务和开关机.md) | ✅ |
| 102 | ★★ | [`sed`](docs/03-文本处理.md#cmd-sed) | 流式文本替换 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 103 | ★★ | [`awk`](docs/03-文本处理.md#cmd-awk) | 按列处理文本 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 104 | ★★ | [`xargs`](docs/03-文本处理.md#cmd-xargs) | 把输入转为参数 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 105 | ★★ | [`tr`](docs/03-文本处理.md#cmd-tr) | 字符替换和删除 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 106 | ★★ | [`ufw`](docs/10-网络和远程连接.md#cmd-ufw) | 简易防火墙 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 107 | ★★ | [`useradd`](docs/05-用户和权限.md#cmd-useradd) | 新建用户（底层） | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 108 | ★★ | [`adduser`](docs/05-用户和权限.md#cmd-adduser) | 新建用户（交互式） | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 109 | ★★ | [`usermod`](docs/05-用户和权限.md#cmd-usermod) | 修改用户 | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 110 | ★★ | [`userdel`](docs/05-用户和权限.md#cmd-userdel) | 删除用户（底层） | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 111 | ★★ | [`deluser`](docs/05-用户和权限.md#cmd-deluser) | 删除用户（交互式） | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 112 | ★★ | [`groupadd`](docs/05-用户和权限.md#cmd-groupadd) | 新建组 | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 113 | ★★ | [`visudo`](docs/05-用户和权限.md#cmd-visudo) | 安全编辑 sudoers | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 114 | ★★ | [`crontab`](docs/14-定时任务.md#cmd-crontab) | 定时任务 | [14 定时任务](docs/14-定时任务.md) | ✅ |
| 115 | ★★ | [`rsync`](docs/10-网络和远程连接.md#cmd-rsync) | 增量同步 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 116 | ★★ | [`ss`](docs/10-网络和远程连接.md#cmd-ss) | 查看端口和连接 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 117 | ★★ | [`netstat`](docs/10-网络和远程连接.md#cmd-netstat) | 旧版端口查看 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 118 | ★★ | [`dig`](docs/10-网络和远程连接.md#cmd-dig) | DNS 查询 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 119 | ★★ | [`nslookup`](docs/10-网络和远程连接.md#cmd-nslookup) | DNS 查询 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 120 | ★★ | [`traceroute`](docs/10-网络和远程连接.md#cmd-traceroute) | 路由追踪 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 121 | ★★ | [`tracepath`](docs/10-网络和远程连接.md#cmd-tracepath) | 路由追踪（免 root） | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 122 | ★★ | [`nmcli`](docs/10-网络和远程连接.md#cmd-nmcli) | NetworkManager 命令行 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 123 | ★★ | [`netplan`](docs/10-网络和远程连接.md#cmd-netplan) | 网络配置 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 124 | ★★ | [`resolvectl`](docs/10-网络和远程连接.md#cmd-resolvectl) | DNS 解析状态 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 125 | ★★ | [`hostnamectl`](docs/11-系统信息和硬件.md#cmd-hostnamectl) | 主机名管理 | [11 系统信息和硬件](docs/11-系统信息和硬件.md) | ✅ |
| 126 | ★★ | [`timedatectl`](docs/11-系统信息和硬件.md#cmd-timedatectl) | 时区和时间同步 | [11 系统信息和硬件](docs/11-系统信息和硬件.md) | ✅ |
| 127 | ★★ | [`localectl`](docs/11-系统信息和硬件.md#cmd-localectl) | 语言和键盘布局 | [11 系统信息和硬件](docs/11-系统信息和硬件.md) | ✅ |
| 128 | ★★ | [`git`](docs/15-开发和容器.md#cmd-git) | 版本控制 | [15 开发和容器](docs/15-开发和容器.md) | ✅ |
| 129 | ★★ | [`docker`](docs/15-开发和容器.md#cmd-docker) | 容器管理 | [15 开发和容器](docs/15-开发和容器.md) | ✅ |
| 130 | ★★ | [`pgrep`](docs/08-进程管理.md#cmd-pgrep) | 按名称查进程号 | [08 进程管理](docs/08-进程管理.md) | ✅ |
| 131 | ★★ | [`pkill`](docs/08-进程管理.md#cmd-pkill) | 按名称结束进程 | [08 进程管理](docs/08-进程管理.md) | ✅ |
| 132 | ★★ | [`nice`](docs/08-进程管理.md#cmd-nice) | 指定优先级启动 | [08 进程管理](docs/08-进程管理.md) | ✅ |
| 133 | ★★ | [`renice`](docs/08-进程管理.md#cmd-renice) | 调整进程优先级 | [08 进程管理](docs/08-进程管理.md) | ✅ |
| 134 | ★★ | [`nohup`](docs/08-进程管理.md#cmd-nohup) | 退出终端后继续运行 | [08 进程管理](docs/08-进程管理.md) | ✅ |
| 135 | ★★ | [`bg`](docs/08-进程管理.md#cmd-bg) | 作业转后台 | [08 进程管理](docs/08-进程管理.md) | ✅ |
| 136 | ★★ | [`fg`](docs/08-进程管理.md#cmd-fg) | 作业转前台 | [08 进程管理](docs/08-进程管理.md) | ✅ |
| 137 | ★★ | [`jobs`](docs/08-进程管理.md#cmd-jobs) | 查看后台作业 | [08 进程管理](docs/08-进程管理.md) | ✅ |
| 138 | ★★ | [`disown`](docs/08-进程管理.md#cmd-disown) | 让作业脱离 Shell | [08 进程管理](docs/08-进程管理.md) | ✅ |
| 139 | ★★ | [`screen`](docs/12-Shell使用.md#cmd-screen) | 终端会话保持 | [12 Shell使用](docs/12-Shell使用.md) | ✅ |
| 140 | ★★ | [`tmux`](docs/12-Shell使用.md#cmd-tmux) | 终端复用 | [12 Shell使用](docs/12-Shell使用.md) | ✅ |
| 141 | ★★ | [`lsof`](docs/08-进程管理.md#cmd-lsof) | 查看打开的文件和端口 | [08 进程管理](docs/08-进程管理.md) | ✅ |
| 142 | ★★ | [`fdisk`](docs/07-磁盘管理.md#cmd-fdisk) | 分区 | [07 磁盘管理](docs/07-磁盘管理.md) | ✅ |
| 143 | ★★ | [`parted`](docs/07-磁盘管理.md#cmd-parted) | 分区（大磁盘） | [07 磁盘管理](docs/07-磁盘管理.md) | ✅ |
| 144 | ★★ | [`mkfs`](docs/07-磁盘管理.md#cmd-mkfs) | 格式化 | [07 磁盘管理](docs/07-磁盘管理.md) | ✅ |
| 145 | ★★ | [`blkid`](docs/07-磁盘管理.md#cmd-blkid) | 查看分区 UUID | [07 磁盘管理](docs/07-磁盘管理.md) | ✅ |
| 146 | ★★ | [`fsck`](docs/07-磁盘管理.md#cmd-fsck) | 文件系统检查 | [07 磁盘管理](docs/07-磁盘管理.md) | ✅ |
| 147 | ★★ | [`dmesg`](docs/11-系统信息和硬件.md#cmd-dmesg) | 内核消息 | [11 系统信息和硬件](docs/11-系统信息和硬件.md) | ✅ |
| 148 | ★★ | [`vmstat`](docs/13-性能监控.md#cmd-vmstat) | 内存和 CPU 统计 | [13 性能监控](docs/13-性能监控.md) | ✅ |
| 149 | ★★ | [`iostat`](docs/13-性能监控.md#cmd-iostat) | 磁盘 IO 统计 | [13 性能监控](docs/13-性能监控.md) | ✅ |
| 150 | ★★ | [`ssh-keygen`](docs/10-网络和远程连接.md#cmd-ssh-keygen) | 生成 SSH 密钥 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 151 | ★★ | [`ssh-copy-id`](docs/10-网络和远程连接.md#cmd-ssh-copy-id) | 上传公钥 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 152 | ★★ | [`sftp`](docs/10-网络和远程连接.md#cmd-sftp) | 安全文件传输 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 153 | ★★ | [`chgrp`](docs/05-用户和权限.md#cmd-chgrp) | 修改所属组 | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 154 | ★★ | [`umask`](docs/05-用户和权限.md#cmd-umask) | 默认权限掩码 | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 155 | ★★ | [`md5sum`](docs/01-目录和文件操作.md#cmd-md5sum) | MD5 校验 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 156 | ★★ | [`sha256sum`](docs/01-目录和文件操作.md#cmd-sha256sum) | SHA256 校验 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 157 | ★★ | [`base64`](docs/03-文本处理.md#cmd-base64) | Base64 编解码 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 158 | ★★ | [`xz`](docs/04-压缩和打包.md#cmd-xz) | xz 压缩 | [04 压缩和打包](docs/04-压缩和打包.md) | ✅ |
| 159 | ★★ | [`bzip2`](docs/04-压缩和打包.md#cmd-bzip2) | bzip2 压缩 | [04 压缩和打包](docs/04-压缩和打包.md) | ✅ |
| 160 | ★★ | [`7z`](docs/04-压缩和打包.md#cmd-7z) | 7-Zip 压缩 | [04 压缩和打包](docs/04-压缩和打包.md) | ✅ |
| 161 | ★★ | [`zcat`](docs/04-压缩和打包.md#cmd-zcat) | 直接查看 .gz | [04 压缩和打包](docs/04-压缩和打包.md) | ✅ |
| 162 | ★★ | [`zgrep`](docs/04-压缩和打包.md#cmd-zgrep) | 在 .gz 中搜索 | [04 压缩和打包](docs/04-压缩和打包.md) | ✅ |
| 163 | ★★ | [`column`](docs/03-文本处理.md#cmd-column) | 按列对齐输出 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 164 | ★★ | [`paste`](docs/03-文本处理.md#cmd-paste) | 按行合并文件 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 165 | ★★ | [`join`](docs/03-文本处理.md#cmd-join) | 按字段连接文件 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 166 | ★★ | [`split`](docs/01-目录和文件操作.md#cmd-split) | 拆分大文件 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 167 | ★★ | [`comm`](docs/03-文本处理.md#cmd-comm) | 比较两个有序文件 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 168 | ★★ | [`nl`](docs/03-文本处理.md#cmd-nl) | 加行号 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 169 | ★★ | [`rev`](docs/03-文本处理.md#cmd-rev) | 每行字符反转 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 170 | ★★ | [`tac`](docs/03-文本处理.md#cmd-tac) | 行倒序输出 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 171 | ★★ | [`printf`](docs/03-文本处理.md#cmd-printf) | 格式化输出 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 172 | ★★ | [`read`](docs/12-Shell使用.md#cmd-read) | 读取输入 | [12 Shell使用](docs/12-Shell使用.md) | ✅ |
| 173 | ★★ | [`test`](docs/12-Shell使用.md#cmd-test) | 条件判断 | [12 Shell使用](docs/12-Shell使用.md) | ✅ |
| 174 | ★★ | [`seq`](docs/03-文本处理.md#cmd-seq) | 生成数字序列 | [03 文本处理](docs/03-文本处理.md) | ✅ |
| 175 | ★★ | [`bc`](docs/12-Shell使用.md#cmd-bc) | 计算器 | [12 Shell使用](docs/12-Shell使用.md) | ✅ |
| 176 | ★★ | [`expr`](docs/12-Shell使用.md#cmd-expr) | 表达式求值 | [12 Shell使用](docs/12-Shell使用.md) | ✅ |
| 177 | ★★ | [`dd`](docs/01-目录和文件操作.md#cmd-dd) | 块级复制、写镜像 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 178 | ★★ | [`sync`](docs/01-目录和文件操作.md#cmd-sync) | 把缓存写入磁盘 | [01 目录和文件操作](docs/01-目录和文件操作.md) | ✅ |
| 179 | ★★ | [`apt-cache`](docs/06-软件包管理.md#cmd-apt-cache) | 查询软件包信息 | [06 软件包管理](docs/06-软件包管理.md) | ✅ |
| 180 | ★★ | [`apt-mark`](docs/06-软件包管理.md#cmd-apt-mark) | 锁定或标记软件包 | [06 软件包管理](docs/06-软件包管理.md) | ✅ |
| 181 | ★★ | [`add-apt-repository`](docs/06-软件包管理.md#cmd-add-apt-repository) | 添加软件源或 PPA | [06 软件包管理](docs/06-软件包管理.md) | ✅ |
| 182 | ★★ | [`update-alternatives`](docs/06-软件包管理.md#cmd-update-alternatives) | 多版本切换 | [06 软件包管理](docs/06-软件包管理.md) | ✅ |
| 183 | ★★ | [`do-release-upgrade`](docs/06-软件包管理.md#cmd-do-release-upgrade) | 升级发行版 | [06 软件包管理](docs/06-软件包管理.md) | ✅ |
| 184 | ★★ | [`unattended-upgrade`](docs/06-软件包管理.md#cmd-unattended-upgrade) | 自动安全更新 | [06 软件包管理](docs/06-软件包管理.md) | ✅ |
| 185 | ★★ | [`flatpak`](docs/06-软件包管理.md#cmd-flatpak) | Flatpak 包管理 | [06 软件包管理](docs/06-软件包管理.md) | ✅ |
| 186 | ★★ | [`loginctl`](docs/05-用户和权限.md#cmd-loginctl) | 登录会话管理 | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 187 | ★★ | [`chage`](docs/05-用户和权限.md#cmd-chage) | 密码过期策略 | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 188 | ★★ | [`getent`](docs/05-用户和权限.md#cmd-getent) | 查询用户、组、主机 | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 189 | ★★ | [`sudoedit`](docs/05-用户和权限.md#cmd-sudoedit) | 以 root 身份安全编辑 | [05 用户和权限](docs/05-用户和权限.md) | ✅ |
| 190 | ★★ | [`systemd-analyze`](docs/09-系统服务和开关机.md#cmd-systemd-analyze) | 开机耗时分析 | [09 系统服务和开关机](docs/09-系统服务和开关机.md) | ✅ |
| 191 | ★★ | [`lsmod`](docs/11-系统信息和硬件.md#cmd-lsmod) | 已加载的内核模块 | [11 系统信息和硬件](docs/11-系统信息和硬件.md) | ✅ |
| 192 | ★★ | [`modprobe`](docs/11-系统信息和硬件.md#cmd-modprobe) | 加载或卸载模块 | [11 系统信息和硬件](docs/11-系统信息和硬件.md) | ✅ |
| 193 | ★★ | [`swapon`](docs/07-磁盘管理.md#cmd-swapon) | 启用或查看交换区 | [07 磁盘管理](docs/07-磁盘管理.md) | ✅ |
| 194 | ★★ | [`mkswap`](docs/07-磁盘管理.md#cmd-mkswap) | 创建交换区 | [07 磁盘管理](docs/07-磁盘管理.md) | ✅ |
| 195 | ★★ | [`findmnt`](docs/07-磁盘管理.md#cmd-findmnt) | 挂载点树 | [07 磁盘管理](docs/07-磁盘管理.md) | ✅ |
| 196 | ★★ | [`ncdu`](docs/07-磁盘管理.md#cmd-ncdu) | 交互式磁盘占用分析 | [07 磁盘管理](docs/07-磁盘管理.md) | ✅ |
| 197 | ★★ | [`iotop`](docs/13-性能监控.md#cmd-iotop) | 按进程看 IO | [13 性能监控](docs/13-性能监控.md) | ✅ |
| 198 | ★★ | [`iftop`](docs/10-网络和远程连接.md#cmd-iftop) | 按连接看带宽 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 199 | ★★ | [`ssh-agent`](docs/10-网络和远程连接.md#cmd-ssh-agent) | SSH 密钥代理 | [10 网络和远程连接](docs/10-网络和远程连接.md) | ✅ |
| 200 | ★★ | [`at`](docs/14-定时任务.md#cmd-at) | 一次性定时任务 | [14 定时任务](docs/14-定时任务.md) | ✅ |

### ★ 高级指令（201–300）

| 排名 | 级别 | 指令 | 用途 | 所在分类 | 状态 |
|---|---|---|---|---|---|
| 201 | ★ | `iptables` | 传统防火墙规则 | 待分类 | 📝 |
| 202 | ★ | `nft` | nftables 防火墙 | 待分类 | 📝 |
| 203 | ★ | `tcpdump` | 抓包 | 待分类 | 📝 |
| 204 | ★ | `strace` | 跟踪系统调用 | 待分类 | 📝 |
| 205 | ★ | `ltrace` | 跟踪库函数调用 | 待分类 | 📝 |
| 206 | ★ | `gdb` | 调试器 | 待分类 | 📝 |
| 207 | ★ | `perf` | 性能剖析 | 待分类 | 📝 |
| 208 | ★ | `lvcreate` | 创建 LVM 逻辑卷 | 待分类 | 📝 |
| 209 | ★ | `pvcreate` | 创建 LVM 物理卷 | 待分类 | 📝 |
| 210 | ★ | `vgcreate` | 创建 LVM 卷组 | 待分类 | 📝 |
| 211 | ★ | `lvextend` | 扩容逻辑卷 | 待分类 | 📝 |
| 212 | ★ | `resize2fs` | 调整 ext4 大小 | 待分类 | 📝 |
| 213 | ★ | `xfs_growfs` | 扩容 XFS | 待分类 | 📝 |
| 214 | ★ | `mdadm` | 软 RAID | 待分类 | 📝 |
| 215 | ★ | `cryptsetup` | LUKS 磁盘加密 | 待分类 | 📝 |
| 216 | ★ | `zfs` | ZFS 数据集管理 | 待分类 | 📝 |
| 217 | ★ | `zpool` | ZFS 存储池 | 待分类 | 📝 |
| 218 | ★ | `btrfs` | Btrfs 管理 | 待分类 | 📝 |
| 219 | ★ | `lxc` | LXD 容器和虚拟机 | 待分类 | 📝 |
| 220 | ★ | `virsh` | KVM 虚拟机管理 | 待分类 | 📝 |
| 221 | ★ | `qemu-img` | 虚拟磁盘镜像 | 待分类 | 📝 |
| 222 | ★ | `multipass` | 快速起 Ubuntu 虚拟机 | 待分类 | 📝 |
| 223 | ★ | `kubectl` | Kubernetes 管理 | 待分类 | 📝 |
| 224 | ★ | `microk8s` | 轻量 Kubernetes | 待分类 | 📝 |
| 225 | ★ | `podman` | 无守护进程容器 | 待分类 | 📝 |
| 226 | ★ | `docker compose` | 多容器编排 | 待分类 | 📝 |
| 227 | ★ | `openssl` | 证书和加密工具 | 待分类 | 📝 |
| 228 | ★ | `gpg` | 加密和签名 | 待分类 | 📝 |
| 229 | ★ | `certbot` | Let's Encrypt 证书 | 待分类 | 📝 |
| 230 | ★ | `aa-status` | AppArmor 状态 | 待分类 | 📝 |
| 231 | ★ | `aa-complain` | AppArmor 告警模式 | 待分类 | 📝 |
| 232 | ★ | `auditctl` | 审计规则 | 待分类 | 📝 |
| 233 | ★ | `ausearch` | 搜索审计日志 | 待分类 | 📝 |
| 234 | ★ | `fail2ban-client` | 防暴力破解 | 待分类 | 📝 |
| 235 | ★ | `nmap` | 端口扫描（仅限自有或授权主机） | 待分类 | 📝 |
| 236 | ★ | `setfacl` | 设置 ACL 权限 | 待分类 | 📝 |
| 237 | ★ | `getfacl` | 查看 ACL | 待分类 | 📝 |
| 238 | ★ | `chattr` | 文件特殊属性 | 待分类 | 📝 |
| 239 | ★ | `lsattr` | 查看特殊属性 | 待分类 | 📝 |
| 240 | ★ | `sysctl` | 内核参数 | 待分类 | 📝 |
| 241 | ★ | `ulimit` | 资源限制 | 待分类 | 📝 |
| 242 | ★ | `systemd-cgls` | cgroup 树 | 待分类 | 📝 |
| 243 | ★ | `systemd-run` | 临时单元运行命令 | 待分类 | 📝 |
| 244 | ★ | `systemd-cgtop` | 按 cgroup 看资源 | 待分类 | 📝 |
| 245 | ★ | `nsenter` | 进入命名空间 | 待分类 | 📝 |
| 246 | ★ | `unshare` | 创建新命名空间 | 待分类 | 📝 |
| 247 | ★ | `chroot` | 切换根目录 | 待分类 | 📝 |
| 248 | ★ | `debootstrap` | 构建最小 Ubuntu 根系统 | 待分类 | 📝 |
| 249 | ★ | `update-initramfs` | 重建 initramfs | 待分类 | 📝 |
| 250 | ★ | `update-grub` | 更新 GRUB 配置 | 待分类 | 📝 |
| 251 | ★ | `grub-install` | 安装引导器 | 待分类 | 📝 |
| 252 | ★ | `dkms` | 第三方内核模块管理 | 待分类 | 📝 |
| 253 | ★ | `mokutil` | 安全启动密钥 | 待分类 | 📝 |
| 254 | ★ | `efibootmgr` | UEFI 启动项 | 待分类 | 📝 |
| 255 | ★ | `smartctl` | 硬盘健康 | 待分类 | 📝 |
| 256 | ★ | `hdparm` | 磁盘参数和测速 | 待分类 | 📝 |
| 257 | ★ | `nvme` | NVMe 管理 | 待分类 | 📝 |
| 258 | ★ | `sar` | 历史性能数据 | 待分类 | 📝 |
| 259 | ★ | `pidstat` | 按进程统计性能 | 待分类 | 📝 |
| 260 | ★ | `bpftrace` | eBPF 动态追踪 | 待分类 | 📝 |
| 261 | ★ | `ethtool` | 网卡参数 | 待分类 | 📝 |
| 262 | ★ | `bridge` | 网桥管理 | 待分类 | 📝 |
| 263 | ★ | `tc` | 流量控制和限速 | 待分类 | 📝 |
| 264 | ★ | `wg` | WireGuard VPN | 待分类 | 📝 |
| 265 | ★ | `ipset` | IP 集合 | 待分类 | 📝 |
| 266 | ★ | `conntrack` | 连接跟踪表 | 待分类 | 📝 |
| 267 | ★ | `socat` | 多功能端口转发 | 待分类 | 📝 |
| 268 | ★ | `nc` | netcat 网络调试 | 待分类 | 📝 |
| 269 | ★ | `mtr` | ping 加 traceroute | 待分类 | 📝 |
| 270 | ★ | `iperf3` | 带宽测试 | 待分类 | 📝 |
| 271 | ★ | `dpkg-reconfigure` | 重新配置软件包 | 待分类 | 📝 |
| 272 | ★ | `dpkg-deb` | 构建和解包 .deb | 待分类 | 📝 |
| 273 | ★ | `dpkg-query` | 查询已装软件包 | 待分类 | 📝 |
| 274 | ★ | `debconf-set-selections` | 预置安装应答 | 待分类 | 📝 |
| 275 | ★ | `cloud-init` | 云主机初始化 | 待分类 | 📝 |
| 276 | ★ | `pro` | Ubuntu Pro 订阅和 ESM | 待分类 | 📝 |
| 277 | ★ | `ubuntu-drivers` | 驱动自动安装 | 待分类 | 📝 |
| 278 | ★ | `needrestart` | 检查需重启的服务 | 待分类 | 📝 |
| 279 | ★ | `logrotate` | 日志轮转 | 待分类 | 📝 |
| 280 | ★ | `logger` | 写入系统日志 | 待分类 | 📝 |
| 281 | ★ | `coredumpctl` | 崩溃转储 | 待分类 | 📝 |
| 282 | ★ | `busctl` | D-Bus 调试 | 待分类 | 📝 |
| 283 | ★ | `udevadm` | 设备规则和事件 | 待分类 | 📝 |
| 284 | ★ | `dmidecode` | BIOS 和硬件信息 | 待分类 | 📝 |
| 285 | ★ | `lshw` | 硬件清单 | 待分类 | 📝 |
| 286 | ★ | `sensors` | 温度和风扇 | 待分类 | 📝 |
| 287 | ★ | `cpupower` | CPU 频率策略 | 待分类 | 📝 |
| 288 | ★ | `numactl` | NUMA 绑定 | 待分类 | 📝 |
| 289 | ★ | `taskset` | CPU 亲和性 | 待分类 | 📝 |
| 290 | ★ | `ionice` | IO 优先级 | 待分类 | 📝 |
| 291 | ★ | `chrt` | 实时调度策略 | 待分类 | 📝 |
| 292 | ★ | `losetup` | 回环设备 | 待分类 | 📝 |
| 293 | ★ | `wipefs` | 清除分区签名 | 待分类 | 📝 |
| 294 | ★ | `sgdisk` | GPT 分区脚本化 | 待分类 | 📝 |
| 295 | ★ | `ansible` | 批量自动化运维 | 待分类 | 📝 |
| 296 | ★ | `sos` | 收集诊断报告 | 待分类 | 📝 |
| 297 | ★ | `apport-cli` | 崩溃报告 | 待分类 | 📝 |
| 298 | ★ | `snapcraft` | 构建 Snap 包 | 待分类 | 📝 |
| 299 | ★ | `kdump-config` | 内核崩溃转储配置 | 待分类 | 📝 |
| 300 | ★ | `bpftool` | eBPF 程序管理 | 待分类 | 📝 |


<!-- 总目录结束 -->
