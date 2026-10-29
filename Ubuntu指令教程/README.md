# Ubuntu 常用指令教程（300 个）

> 版本基线：**Ubuntu 26.04.1 LTS**（代号 resolute）｜bash 5.3.9｜Rust 版 coreutils（uutils）0.10.0，其中 cp、mv、rm、df 由 GNU coreutils 9.7 提供｜sudo-rs｜apt 3.2.0｜systemd 259｜OpenSSH 10.2p1
>
> 按用途分类，**每类一个文件**。每个指令的固定结构：作用 · 基本格式 · 常用参数（表格）· 常用示例（带实测输出）· ⚠️ 踩坑警告 · 一句话总结。简单的指令省略用不上的部分。

---

## 这本教程写给谁

- 会写程序、熟悉数据库，但没有系统用过 Linux 命令行的人。
- 要在 Ubuntu 服务器上部署、排查问题，需要一份“查得到、看得懂、照着能跑”的指令手册。
- 不解释什么是文件、什么是进程；重点讲每个参数**什么时候用**、**容易错在哪**。

## 怎么读

| 路径 | 内容 | 时间 |
|---|---|---|
| 最短路径 | 初级的 01 目录和文件操作、02 文件查看和编辑、06 软件包管理、08 进程管理 | 约 4 小时 |
| 完整路径 | 300 个指令，按初级 → 中级 → 高级 | 每天 2 小时，约 6 周 |

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

建立演示环境的脚本是 [`tools/setup-demo.sh`](tools/setup-demo.sh)，在自己的 Ubuntu 上可以用它建一份一样的目录来练习（脚本会创建用户 tom，请在测试机上执行）。

## 输出是怎么来的

- 示例输出在 **Ubuntu 26.04.1 LTS 官方根文件系统**（ubuntu-base）中实际执行得到，执行的命令清单保存在 [`tools/cmds/`](tools/cmds/)。
- 测试环境是运行在云服务器上的容器，所以 `df`、`free`、`top`、`ip`、`lsblk`、`uname` 这类显示系统信息的指令，其设备名、容量、内核版本会和你的机器不同，页面里已注明。
- 宿主机启用了 SELinux，导致 `ls -l` 的权限后面多出一个 `+`，目录大小也和 ext4 不同。文中已去掉 `+`，并把目录大小统一为 ext4 上的 `4096`，以符合普通 Ubuntu 机器上看到的样子。
- 需要图形界面、完整 systemd 或真实终端交互的内容（如 nano、vim 的按键，systemctl start，reboot）无法在测试环境里执行，这些地方都标注了 `⚠️ 未实机验证`，写法已对照该版本的手册核实。

## Ubuntu 26.04 和旧版本的几个差别

| 变化 | 影响 | 详见 |
|---|---|---|
| 基础命令（ls、cat、mkdir、chmod、chown 等）改由 Rust 重写的 uutils 提供 | 常用参数相同，部分报错文字和 GNU 版不同；个别行为有差异 | [chown](docs/beginner/05-用户和权限.md#cmd-chown)、[mkdir](docs/beginner/01-目录和文件操作.md#cmd-mkdir) |
| `sudo` 改由 sudo-rs 提供 | 输入密码时显示 `*`；没有 `-E` 参数 | [sudo](docs/beginner/05-用户和权限.md#cmd-sudo) |
| apt 3.x 的输出改为分块显示 | 安装前的摘要更清楚 | [apt](docs/beginner/06-软件包管理.md#cmd-apt) |
| ssh 默认由 `ssh.socket` 按需启动 | 改端口的步骤和以前不同 | [systemctl](docs/beginner/09-系统服务和开关机.md#cmd-systemctl) |
| 登录记录改由 wtmpdb 保存 | `last` 读的是 `/var/log/wtmp.db` | [last](docs/beginner/05-用户和权限.md#cmd-last) |

## 进度

| 部分 | 状态 |
|---|---|
| **初级指令 001–100** | ✅ |
| 中级指令 101–200 | 📝 |
| 高级指令 201–300 | 📝 |

## 分类目录（初级 100 个）

<!-- 分类目录开始 -->

| 分类 | 个数 | 包含的指令 |
|---|---|---|
| [01 目录和文件操作](docs/beginner/01-目录和文件操作.md)<br/>浏览目录，复制、移动、删除、查找文件 | 18 | [ls](docs/beginner/01-目录和文件操作.md#cmd-ls)、[cd](docs/beginner/01-目录和文件操作.md#cmd-cd)、[pwd](docs/beginner/01-目录和文件操作.md#cmd-pwd)、[cp](docs/beginner/01-目录和文件操作.md#cmd-cp)、[mv](docs/beginner/01-目录和文件操作.md#cmd-mv)、[rm](docs/beginner/01-目录和文件操作.md#cmd-rm)、[mkdir](docs/beginner/01-目录和文件操作.md#cmd-mkdir)、[touch](docs/beginner/01-目录和文件操作.md#cmd-touch)、[find](docs/beginner/01-目录和文件操作.md#cmd-find)、[file](docs/beginner/01-目录和文件操作.md#cmd-file)、[rmdir](docs/beginner/01-目录和文件操作.md#cmd-rmdir)、[ln](docs/beginner/01-目录和文件操作.md#cmd-ln)、[tree](docs/beginner/01-目录和文件操作.md#cmd-tree)、[basename](docs/beginner/01-目录和文件操作.md#cmd-basename)、[dirname](docs/beginner/01-目录和文件操作.md#cmd-dirname)、[stat](docs/beginner/01-目录和文件操作.md#cmd-stat)、[locate](docs/beginner/01-目录和文件操作.md#cmd-locate)、[updatedb](docs/beginner/01-目录和文件操作.md#cmd-updatedb) |
| [02 文件查看和编辑](docs/beginner/02-文件查看和编辑.md)<br/>查看文件内容，在终端里编辑文件 | 7 | [cat](docs/beginner/02-文件查看和编辑.md#cmd-cat)、[nano](docs/beginner/02-文件查看和编辑.md#cmd-nano)、[less](docs/beginner/02-文件查看和编辑.md#cmd-less)、[head](docs/beginner/02-文件查看和编辑.md#cmd-head)、[tail](docs/beginner/02-文件查看和编辑.md#cmd-tail)、[more](docs/beginner/02-文件查看和编辑.md#cmd-more)、[vim](docs/beginner/02-文件查看和编辑.md#cmd-vim) |
| [03 文本处理](docs/beginner/03-文本处理.md)<br/>搜索、统计、排序、截取、比较文本 | 8 | [echo](docs/beginner/03-文本处理.md#cmd-echo)、[grep](docs/beginner/03-文本处理.md#cmd-grep)、[wc](docs/beginner/03-文本处理.md#cmd-wc)、[sort](docs/beginner/03-文本处理.md#cmd-sort)、[uniq](docs/beginner/03-文本处理.md#cmd-uniq)、[diff](docs/beginner/03-文本处理.md#cmd-diff)、[cut](docs/beginner/03-文本处理.md#cmd-cut)、[tee](docs/beginner/03-文本处理.md#cmd-tee) |
| [04 压缩和打包](docs/beginner/04-压缩和打包.md)<br/>tar、zip、gzip 三种格式的打包和解包 | 5 | [tar](docs/beginner/04-压缩和打包.md#cmd-tar)、[zip](docs/beginner/04-压缩和打包.md#cmd-zip)、[unzip](docs/beginner/04-压缩和打包.md#cmd-unzip)、[gzip](docs/beginner/04-压缩和打包.md#cmd-gzip)、[gunzip](docs/beginner/04-压缩和打包.md#cmd-gunzip) |
| [05 用户和权限](docs/beginner/05-用户和权限.md)<br/>提权、切换用户、改密码、改权限、查登录记录 | 11 | [sudo](docs/beginner/05-用户和权限.md#cmd-sudo)、[chmod](docs/beginner/05-用户和权限.md#cmd-chmod)、[chown](docs/beginner/05-用户和权限.md#cmd-chown)、[whoami](docs/beginner/05-用户和权限.md#cmd-whoami)、[passwd](docs/beginner/05-用户和权限.md#cmd-passwd)、[su](docs/beginner/05-用户和权限.md#cmd-su)、[id](docs/beginner/05-用户和权限.md#cmd-id)、[groups](docs/beginner/05-用户和权限.md#cmd-groups)、[who](docs/beginner/05-用户和权限.md#cmd-who)、[w](docs/beginner/05-用户和权限.md#cmd-w)、[last](docs/beginner/05-用户和权限.md#cmd-last) |
| [06 软件包管理](docs/beginner/06-软件包管理.md)<br/>apt、dpkg、snap 装卸软件 | 4 | [apt](docs/beginner/06-软件包管理.md#cmd-apt)、[apt-get](docs/beginner/06-软件包管理.md#cmd-apt-get)、[dpkg](docs/beginner/06-软件包管理.md#cmd-dpkg)、[snap](docs/beginner/06-软件包管理.md#cmd-snap) |
| [07 磁盘管理](docs/beginner/07-磁盘管理.md)<br/>看空间、看分区、挂载和卸载 | 5 | [df](docs/beginner/07-磁盘管理.md#cmd-df)、[du](docs/beginner/07-磁盘管理.md#cmd-du)、[lsblk](docs/beginner/07-磁盘管理.md#cmd-lsblk)、[mount](docs/beginner/07-磁盘管理.md#cmd-mount)、[umount](docs/beginner/07-磁盘管理.md#cmd-umount) |
| [08 进程管理](docs/beginner/08-进程管理.md)<br/>查看、监控、结束进程，计时和定时刷新 | 8 | [top](docs/beginner/08-进程管理.md#cmd-top)、[ps](docs/beginner/08-进程管理.md#cmd-ps)、[kill](docs/beginner/08-进程管理.md#cmd-kill)、[htop](docs/beginner/08-进程管理.md#cmd-htop)、[sleep](docs/beginner/08-进程管理.md#cmd-sleep)、[watch](docs/beginner/08-进程管理.md#cmd-watch)、[time](docs/beginner/08-进程管理.md#cmd-time)、[killall](docs/beginner/08-进程管理.md#cmd-killall) |
| [09 系统服务和开关机](docs/beginner/09-系统服务和开关机.md)<br/>systemctl 管服务，重启和关机 | 3 | [reboot](docs/beginner/09-系统服务和开关机.md#cmd-reboot)、[shutdown](docs/beginner/09-系统服务和开关机.md#cmd-shutdown)、[systemctl](docs/beginner/09-系统服务和开关机.md#cmd-systemctl) |
| [10 网络和远程连接](docs/beginner/10-网络和远程连接.md)<br/>IP、连通性、下载、HTTP 请求、ssh 远程 | 7 | [ping](docs/beginner/10-网络和远程连接.md#cmd-ping)、[ip](docs/beginner/10-网络和远程连接.md#cmd-ip)、[wget](docs/beginner/10-网络和远程连接.md#cmd-wget)、[curl](docs/beginner/10-网络和远程连接.md#cmd-curl)、[hostname](docs/beginner/10-网络和远程连接.md#cmd-hostname)、[ssh](docs/beginner/10-网络和远程连接.md#cmd-ssh)、[scp](docs/beginner/10-网络和远程连接.md#cmd-scp) |
| [11 系统信息和硬件](docs/beginner/11-系统信息和硬件.md)<br/>版本、内核、时间、CPU、内存、PCI/USB 设备 | 11 | [free](docs/beginner/11-系统信息和硬件.md#cmd-free)、[uname](docs/beginner/11-系统信息和硬件.md#cmd-uname)、[date](docs/beginner/11-系统信息和硬件.md#cmd-date)、[cal](docs/beginner/11-系统信息和硬件.md#cmd-cal)、[uptime](docs/beginner/11-系统信息和硬件.md#cmd-uptime)、[lsb_release](docs/beginner/11-系统信息和硬件.md#cmd-lsb_release)、[fastfetch](docs/beginner/11-系统信息和硬件.md#cmd-fastfetch)、[lscpu](docs/beginner/11-系统信息和硬件.md#cmd-lscpu)、[lsusb](docs/beginner/11-系统信息和硬件.md#cmd-lsusb)、[lspci](docs/beginner/11-系统信息和硬件.md#cmd-lspci)、[nproc](docs/beginner/11-系统信息和硬件.md#cmd-nproc) |
| [12 Shell使用](docs/beginner/12-Shell使用.md)<br/>清屏、历史、别名、环境变量、查帮助 | 13 | [clear](docs/beginner/12-Shell使用.md#cmd-clear)、[man](docs/beginner/12-Shell使用.md#cmd-man)、[history](docs/beginner/12-Shell使用.md#cmd-history)、[exit](docs/beginner/12-Shell使用.md#cmd-exit)、[which](docs/beginner/12-Shell使用.md#cmd-which)、[whereis](docs/beginner/12-Shell使用.md#cmd-whereis)、[alias](docs/beginner/12-Shell使用.md#cmd-alias)、[type](docs/beginner/12-Shell使用.md#cmd-type)、[help](docs/beginner/12-Shell使用.md#cmd-help)、[env](docs/beginner/12-Shell使用.md#cmd-env)、[export](docs/beginner/12-Shell使用.md#cmd-export)、[source](docs/beginner/12-Shell使用.md#cmd-source)、[xdg-open](docs/beginner/12-Shell使用.md#cmd-xdg-open) |

<!-- 分类目录结束 -->

## 总目录对应表（300 个）

按使用频率排名。点指令名直接跳到它所在分类文件中的位置；中级和高级写完后再归入分类。

<!-- 总目录开始 -->

### 初级指令（1–100）

| 排名 | 指令 | 用途 | 所在分类 | 状态 |
|---|---|---|---|---|
| 1 | [`ls`](docs/beginner/01-目录和文件操作.md#cmd-ls) | 列出目录内容 | [01 目录和文件操作](docs/beginner/01-目录和文件操作.md) | ✅ |
| 2 | [`cd`](docs/beginner/01-目录和文件操作.md#cmd-cd) | 切换目录 | [01 目录和文件操作](docs/beginner/01-目录和文件操作.md) | ✅ |
| 3 | [`pwd`](docs/beginner/01-目录和文件操作.md#cmd-pwd) | 显示当前目录 | [01 目录和文件操作](docs/beginner/01-目录和文件操作.md) | ✅ |
| 4 | [`sudo`](docs/beginner/05-用户和权限.md#cmd-sudo) | 以管理员身份执行 | [05 用户和权限](docs/beginner/05-用户和权限.md) | ✅ |
| 5 | [`cat`](docs/beginner/02-文件查看和编辑.md#cmd-cat) | 查看文件内容 | [02 文件查看和编辑](docs/beginner/02-文件查看和编辑.md) | ✅ |
| 6 | [`cp`](docs/beginner/01-目录和文件操作.md#cmd-cp) | 复制文件和目录 | [01 目录和文件操作](docs/beginner/01-目录和文件操作.md) | ✅ |
| 7 | [`mv`](docs/beginner/01-目录和文件操作.md#cmd-mv) | 移动或重命名 | [01 目录和文件操作](docs/beginner/01-目录和文件操作.md) | ✅ |
| 8 | [`rm`](docs/beginner/01-目录和文件操作.md#cmd-rm) | 删除文件和目录 | [01 目录和文件操作](docs/beginner/01-目录和文件操作.md) | ✅ |
| 9 | [`mkdir`](docs/beginner/01-目录和文件操作.md#cmd-mkdir) | 新建目录 | [01 目录和文件操作](docs/beginner/01-目录和文件操作.md) | ✅ |
| 10 | [`touch`](docs/beginner/01-目录和文件操作.md#cmd-touch) | 新建空文件或修改时间 | [01 目录和文件操作](docs/beginner/01-目录和文件操作.md) | ✅ |
| 11 | [`nano`](docs/beginner/02-文件查看和编辑.md#cmd-nano) | 简易文本编辑器 | [02 文件查看和编辑](docs/beginner/02-文件查看和编辑.md) | ✅ |
| 12 | [`apt`](docs/beginner/06-软件包管理.md#cmd-apt) | 安装、升级、卸载软件 | [06 软件包管理](docs/beginner/06-软件包管理.md) | ✅ |
| 13 | [`clear`](docs/beginner/12-Shell使用.md#cmd-clear) | 清屏 | [12 Shell使用](docs/beginner/12-Shell使用.md) | ✅ |
| 14 | [`echo`](docs/beginner/03-文本处理.md#cmd-echo) | 输出一行文本 | [03 文本处理](docs/beginner/03-文本处理.md) | ✅ |
| 15 | [`man`](docs/beginner/12-Shell使用.md#cmd-man) | 查看命令手册 | [12 Shell使用](docs/beginner/12-Shell使用.md) | ✅ |
| 16 | [`history`](docs/beginner/12-Shell使用.md#cmd-history) | 查看命令历史 | [12 Shell使用](docs/beginner/12-Shell使用.md) | ✅ |
| 17 | [`exit`](docs/beginner/12-Shell使用.md#cmd-exit) | 退出 Shell 或脚本 | [12 Shell使用](docs/beginner/12-Shell使用.md) | ✅ |
| 18 | [`less`](docs/beginner/02-文件查看和编辑.md#cmd-less) | 分页查看长文件 | [02 文件查看和编辑](docs/beginner/02-文件查看和编辑.md) | ✅ |
| 19 | [`head`](docs/beginner/02-文件查看和编辑.md#cmd-head) | 查看文件开头 | [02 文件查看和编辑](docs/beginner/02-文件查看和编辑.md) | ✅ |
| 20 | [`tail`](docs/beginner/02-文件查看和编辑.md#cmd-tail) | 查看文件末尾、跟踪日志 | [02 文件查看和编辑](docs/beginner/02-文件查看和编辑.md) | ✅ |
| 21 | [`grep`](docs/beginner/03-文本处理.md#cmd-grep) | 按关键词搜索文本 | [03 文本处理](docs/beginner/03-文本处理.md) | ✅ |
| 22 | [`find`](docs/beginner/01-目录和文件操作.md#cmd-find) | 按条件查找文件 | [01 目录和文件操作](docs/beginner/01-目录和文件操作.md) | ✅ |
| 23 | [`chmod`](docs/beginner/05-用户和权限.md#cmd-chmod) | 修改文件权限 | [05 用户和权限](docs/beginner/05-用户和权限.md) | ✅ |
| 24 | [`chown`](docs/beginner/05-用户和权限.md#cmd-chown) | 修改所有者和所属组 | [05 用户和权限](docs/beginner/05-用户和权限.md) | ✅ |
| 25 | [`df`](docs/beginner/07-磁盘管理.md#cmd-df) | 查看磁盘空间 | [07 磁盘管理](docs/beginner/07-磁盘管理.md) | ✅ |
| 26 | [`du`](docs/beginner/07-磁盘管理.md#cmd-du) | 查看目录占用的空间 | [07 磁盘管理](docs/beginner/07-磁盘管理.md) | ✅ |
| 27 | [`free`](docs/beginner/11-系统信息和硬件.md#cmd-free) | 查看内存 | [11 系统信息和硬件](docs/beginner/11-系统信息和硬件.md) | ✅ |
| 28 | [`top`](docs/beginner/08-进程管理.md#cmd-top) | 实时查看负载和进程 | [08 进程管理](docs/beginner/08-进程管理.md) | ✅ |
| 29 | [`ps`](docs/beginner/08-进程管理.md#cmd-ps) | 查看进程列表 | [08 进程管理](docs/beginner/08-进程管理.md) | ✅ |
| 30 | [`kill`](docs/beginner/08-进程管理.md#cmd-kill) | 结束进程 | [08 进程管理](docs/beginner/08-进程管理.md) | ✅ |
| 31 | [`ping`](docs/beginner/10-网络和远程连接.md#cmd-ping) | 测试网络连通 | [10 网络和远程连接](docs/beginner/10-网络和远程连接.md) | ✅ |
| 32 | [`ip`](docs/beginner/10-网络和远程连接.md#cmd-ip) | 查看 IP、网卡和路由 | [10 网络和远程连接](docs/beginner/10-网络和远程连接.md) | ✅ |
| 33 | [`wget`](docs/beginner/10-网络和远程连接.md#cmd-wget) | 下载文件 | [10 网络和远程连接](docs/beginner/10-网络和远程连接.md) | ✅ |
| 34 | [`curl`](docs/beginner/10-网络和远程连接.md#cmd-curl) | 发送 HTTP 请求 | [10 网络和远程连接](docs/beginner/10-网络和远程连接.md) | ✅ |
| 35 | [`tar`](docs/beginner/04-压缩和打包.md#cmd-tar) | 打包和解包 | [04 压缩和打包](docs/beginner/04-压缩和打包.md) | ✅ |
| 36 | [`zip`](docs/beginner/04-压缩和打包.md#cmd-zip) | 压缩成 zip | [04 压缩和打包](docs/beginner/04-压缩和打包.md) | ✅ |
| 37 | [`unzip`](docs/beginner/04-压缩和打包.md#cmd-unzip) | 解压 zip | [04 压缩和打包](docs/beginner/04-压缩和打包.md) | ✅ |
| 38 | [`reboot`](docs/beginner/09-系统服务和开关机.md#cmd-reboot) | 重启系统 | [09 系统服务和开关机](docs/beginner/09-系统服务和开关机.md) | ✅ |
| 39 | [`shutdown`](docs/beginner/09-系统服务和开关机.md#cmd-shutdown) | 关机或定时重启 | [09 系统服务和开关机](docs/beginner/09-系统服务和开关机.md) | ✅ |
| 40 | [`whoami`](docs/beginner/05-用户和权限.md#cmd-whoami) | 显示当前用户名 | [05 用户和权限](docs/beginner/05-用户和权限.md) | ✅ |
| 41 | [`uname`](docs/beginner/11-系统信息和硬件.md#cmd-uname) | 查看内核和系统信息 | [11 系统信息和硬件](docs/beginner/11-系统信息和硬件.md) | ✅ |
| 42 | [`date`](docs/beginner/11-系统信息和硬件.md#cmd-date) | 显示和计算日期时间 | [11 系统信息和硬件](docs/beginner/11-系统信息和硬件.md) | ✅ |
| 43 | [`cal`](docs/beginner/11-系统信息和硬件.md#cmd-cal) | 显示日历 | [11 系统信息和硬件](docs/beginner/11-系统信息和硬件.md) | ✅ |
| 44 | [`which`](docs/beginner/12-Shell使用.md#cmd-which) | 查命令对应的程序位置 | [12 Shell使用](docs/beginner/12-Shell使用.md) | ✅ |
| 45 | [`whereis`](docs/beginner/12-Shell使用.md#cmd-whereis) | 查程序和手册的位置 | [12 Shell使用](docs/beginner/12-Shell使用.md) | ✅ |
| 46 | [`file`](docs/beginner/01-目录和文件操作.md#cmd-file) | 判断文件类型 | [01 目录和文件操作](docs/beginner/01-目录和文件操作.md) | ✅ |
| 47 | [`wc`](docs/beginner/03-文本处理.md#cmd-wc) | 统计行数、字数 | [03 文本处理](docs/beginner/03-文本处理.md) | ✅ |
| 48 | [`sort`](docs/beginner/03-文本处理.md#cmd-sort) | 排序 | [03 文本处理](docs/beginner/03-文本处理.md) | ✅ |
| 49 | [`uniq`](docs/beginner/03-文本处理.md#cmd-uniq) | 去重和计数 | [03 文本处理](docs/beginner/03-文本处理.md) | ✅ |
| 50 | [`rmdir`](docs/beginner/01-目录和文件操作.md#cmd-rmdir) | 删除空目录 | [01 目录和文件操作](docs/beginner/01-目录和文件操作.md) | ✅ |
| 51 | [`ln`](docs/beginner/01-目录和文件操作.md#cmd-ln) | 创建链接 | [01 目录和文件操作](docs/beginner/01-目录和文件操作.md) | ✅ |
| 52 | [`more`](docs/beginner/02-文件查看和编辑.md#cmd-more) | 简单的分页查看 | [02 文件查看和编辑](docs/beginner/02-文件查看和编辑.md) | ✅ |
| 53 | [`passwd`](docs/beginner/05-用户和权限.md#cmd-passwd) | 修改密码 | [05 用户和权限](docs/beginner/05-用户和权限.md) | ✅ |
| 54 | [`su`](docs/beginner/05-用户和权限.md#cmd-su) | 切换到其他用户 | [05 用户和权限](docs/beginner/05-用户和权限.md) | ✅ |
| 55 | [`hostname`](docs/beginner/10-网络和远程连接.md#cmd-hostname) | 查看主机名和本机 IP | [10 网络和远程连接](docs/beginner/10-网络和远程连接.md) | ✅ |
| 56 | [`uptime`](docs/beginner/11-系统信息和硬件.md#cmd-uptime) | 查看运行时长和负载 | [11 系统信息和硬件](docs/beginner/11-系统信息和硬件.md) | ✅ |
| 57 | [`lsb_release`](docs/beginner/11-系统信息和硬件.md#cmd-lsb_release) | 查看 Ubuntu 版本 | [11 系统信息和硬件](docs/beginner/11-系统信息和硬件.md) | ✅ |
| 58 | [`vim`](docs/beginner/02-文件查看和编辑.md#cmd-vim) | 全功能的终端编辑器 | [02 文件查看和编辑](docs/beginner/02-文件查看和编辑.md) | ✅ |
| 59 | [`tree`](docs/beginner/01-目录和文件操作.md#cmd-tree) | 树状显示目录 | [01 目录和文件操作](docs/beginner/01-目录和文件操作.md) | ✅ |
| 60 | [`alias`](docs/beginner/12-Shell使用.md#cmd-alias) | 给命令起别名 | [12 Shell使用](docs/beginner/12-Shell使用.md) | ✅ |
| 61 | [`type`](docs/beginner/12-Shell使用.md#cmd-type) | 查看命令的真实身份 | [12 Shell使用](docs/beginner/12-Shell使用.md) | ✅ |
| 62 | [`help`](docs/beginner/12-Shell使用.md#cmd-help) | bash 内建命令的帮助 | [12 Shell使用](docs/beginner/12-Shell使用.md) | ✅ |
| 63 | [`apt-get`](docs/beginner/06-软件包管理.md#cmd-apt-get) | 脚本里用的包管理命令 | [06 软件包管理](docs/beginner/06-软件包管理.md) | ✅ |
| 64 | [`dpkg`](docs/beginner/06-软件包管理.md#cmd-dpkg) | 安装和查询 .deb 包 | [06 软件包管理](docs/beginner/06-软件包管理.md) | ✅ |
| 65 | [`snap`](docs/beginner/06-软件包管理.md#cmd-snap) | 管理 Snap 软件包 | [06 软件包管理](docs/beginner/06-软件包管理.md) | ✅ |
| 66 | [`systemctl`](docs/beginner/09-系统服务和开关机.md#cmd-systemctl) | 管理系统服务 | [09 系统服务和开关机](docs/beginner/09-系统服务和开关机.md) | ✅ |
| 67 | [`lsblk`](docs/beginner/07-磁盘管理.md#cmd-lsblk) | 列出磁盘和分区 | [07 磁盘管理](docs/beginner/07-磁盘管理.md) | ✅ |
| 68 | [`mount`](docs/beginner/07-磁盘管理.md#cmd-mount) | 挂载磁盘和分区 | [07 磁盘管理](docs/beginner/07-磁盘管理.md) | ✅ |
| 69 | [`umount`](docs/beginner/07-磁盘管理.md#cmd-umount) | 卸载磁盘和分区 | [07 磁盘管理](docs/beginner/07-磁盘管理.md) | ✅ |
| 70 | [`id`](docs/beginner/05-用户和权限.md#cmd-id) | 查看 UID 和所属组 | [05 用户和权限](docs/beginner/05-用户和权限.md) | ✅ |
| 71 | [`groups`](docs/beginner/05-用户和权限.md#cmd-groups) | 查看所属组 | [05 用户和权限](docs/beginner/05-用户和权限.md) | ✅ |
| 72 | [`who`](docs/beginner/05-用户和权限.md#cmd-who) | 查看谁登录了系统 | [05 用户和权限](docs/beginner/05-用户和权限.md) | ✅ |
| 73 | [`w`](docs/beginner/05-用户和权限.md#cmd-w) | 查看谁在线、在做什么 | [05 用户和权限](docs/beginner/05-用户和权限.md) | ✅ |
| 74 | [`last`](docs/beginner/05-用户和权限.md#cmd-last) | 查看登录历史 | [05 用户和权限](docs/beginner/05-用户和权限.md) | ✅ |
| 75 | [`env`](docs/beginner/12-Shell使用.md#cmd-env) | 查看环境变量 | [12 Shell使用](docs/beginner/12-Shell使用.md) | ✅ |
| 76 | [`export`](docs/beginner/12-Shell使用.md#cmd-export) | 设置环境变量 | [12 Shell使用](docs/beginner/12-Shell使用.md) | ✅ |
| 77 | [`source`](docs/beginner/12-Shell使用.md#cmd-source) | 在当前 Shell 里执行脚本 | [12 Shell使用](docs/beginner/12-Shell使用.md) | ✅ |
| 78 | [`diff`](docs/beginner/03-文本处理.md#cmd-diff) | 比较文件差异 | [03 文本处理](docs/beginner/03-文本处理.md) | ✅ |
| 79 | [`cut`](docs/beginner/03-文本处理.md#cmd-cut) | 按列截取文本 | [03 文本处理](docs/beginner/03-文本处理.md) | ✅ |
| 80 | [`tee`](docs/beginner/03-文本处理.md#cmd-tee) | 同时输出到屏幕和文件 | [03 文本处理](docs/beginner/03-文本处理.md) | ✅ |
| 81 | [`gzip`](docs/beginner/04-压缩和打包.md#cmd-gzip) | 压缩单个文件 | [04 压缩和打包](docs/beginner/04-压缩和打包.md) | ✅ |
| 82 | [`gunzip`](docs/beginner/04-压缩和打包.md#cmd-gunzip) | 解压 .gz 文件 | [04 压缩和打包](docs/beginner/04-压缩和打包.md) | ✅ |
| 83 | [`ssh`](docs/beginner/10-网络和远程连接.md#cmd-ssh) | 远程登录服务器 | [10 网络和远程连接](docs/beginner/10-网络和远程连接.md) | ✅ |
| 84 | [`scp`](docs/beginner/10-网络和远程连接.md#cmd-scp) | 远程复制文件 | [10 网络和远程连接](docs/beginner/10-网络和远程连接.md) | ✅ |
| 85 | [`htop`](docs/beginner/08-进程管理.md#cmd-htop) | 更好用的进程监控 | [08 进程管理](docs/beginner/08-进程管理.md) | ✅ |
| 86 | [`fastfetch`](docs/beginner/11-系统信息和硬件.md#cmd-fastfetch) | 一屏看清系统概况 | [11 系统信息和硬件](docs/beginner/11-系统信息和硬件.md) | ✅ |
| 87 | [`sleep`](docs/beginner/08-进程管理.md#cmd-sleep) | 暂停一段时间 | [08 进程管理](docs/beginner/08-进程管理.md) | ✅ |
| 88 | [`watch`](docs/beginner/08-进程管理.md#cmd-watch) | 定时重复执行命令 | [08 进程管理](docs/beginner/08-进程管理.md) | ✅ |
| 89 | [`time`](docs/beginner/08-进程管理.md#cmd-time) | 统计命令执行时间 | [08 进程管理](docs/beginner/08-进程管理.md) | ✅ |
| 90 | [`basename`](docs/beginner/01-目录和文件操作.md#cmd-basename) | 从路径取文件名 | [01 目录和文件操作](docs/beginner/01-目录和文件操作.md) | ✅ |
| 91 | [`dirname`](docs/beginner/01-目录和文件操作.md#cmd-dirname) | 从路径取目录 | [01 目录和文件操作](docs/beginner/01-目录和文件操作.md) | ✅ |
| 92 | [`stat`](docs/beginner/01-目录和文件操作.md#cmd-stat) | 查看文件详细属性 | [01 目录和文件操作](docs/beginner/01-目录和文件操作.md) | ✅ |
| 93 | [`locate`](docs/beginner/01-目录和文件操作.md#cmd-locate) | 按文件名快速查找 | [01 目录和文件操作](docs/beginner/01-目录和文件操作.md) | ✅ |
| 94 | [`updatedb`](docs/beginner/01-目录和文件操作.md#cmd-updatedb) | 更新 locate 的数据库 | [01 目录和文件操作](docs/beginner/01-目录和文件操作.md) | ✅ |
| 95 | [`lscpu`](docs/beginner/11-系统信息和硬件.md#cmd-lscpu) | 查看 CPU 信息 | [11 系统信息和硬件](docs/beginner/11-系统信息和硬件.md) | ✅ |
| 96 | [`lsusb`](docs/beginner/11-系统信息和硬件.md#cmd-lsusb) | 列出 USB 设备 | [11 系统信息和硬件](docs/beginner/11-系统信息和硬件.md) | ✅ |
| 97 | [`lspci`](docs/beginner/11-系统信息和硬件.md#cmd-lspci) | 列出 PCI 设备 | [11 系统信息和硬件](docs/beginner/11-系统信息和硬件.md) | ✅ |
| 98 | [`nproc`](docs/beginner/11-系统信息和硬件.md#cmd-nproc) | 显示 CPU 数量 | [11 系统信息和硬件](docs/beginner/11-系统信息和硬件.md) | ✅ |
| 99 | [`killall`](docs/beginner/08-进程管理.md#cmd-killall) | 按程序名结束进程 | [08 进程管理](docs/beginner/08-进程管理.md) | ✅ |
| 100 | [`xdg-open`](docs/beginner/12-Shell使用.md#cmd-xdg-open) | 用默认程序打开文件 | [12 Shell使用](docs/beginner/12-Shell使用.md) | ✅ |

### 中级指令（101–200）

| 排名 | 指令 | 用途 | 所在分类 | 状态 |
|---|---|---|---|---|
| 101 | `journalctl` | 查看 systemd 日志 | 待分类 | 📝 |
| 102 | `sed` | 流式文本替换 | 待分类 | 📝 |
| 103 | `awk` | 按列处理文本 | 待分类 | 📝 |
| 104 | `xargs` | 把输入转为参数 | 待分类 | 📝 |
| 105 | `tr` | 字符替换和删除 | 待分类 | 📝 |
| 106 | `ufw` | 简易防火墙 | 待分类 | 📝 |
| 107 | `useradd` | 新建用户（底层） | 待分类 | 📝 |
| 108 | `adduser` | 新建用户（交互式） | 待分类 | 📝 |
| 109 | `usermod` | 修改用户 | 待分类 | 📝 |
| 110 | `userdel` | 删除用户（底层） | 待分类 | 📝 |
| 111 | `deluser` | 删除用户（交互式） | 待分类 | 📝 |
| 112 | `groupadd` | 新建组 | 待分类 | 📝 |
| 113 | `visudo` | 安全编辑 sudoers | 待分类 | 📝 |
| 114 | `crontab` | 定时任务 | 待分类 | 📝 |
| 115 | `rsync` | 增量同步 | 待分类 | 📝 |
| 116 | `ss` | 查看端口和连接 | 待分类 | 📝 |
| 117 | `netstat` | 旧版端口查看 | 待分类 | 📝 |
| 118 | `dig` | DNS 查询 | 待分类 | 📝 |
| 119 | `nslookup` | DNS 查询 | 待分类 | 📝 |
| 120 | `traceroute` | 路由追踪 | 待分类 | 📝 |
| 121 | `tracepath` | 路由追踪（免 root） | 待分类 | 📝 |
| 122 | `nmcli` | NetworkManager 命令行 | 待分类 | 📝 |
| 123 | `netplan` | 网络配置 | 待分类 | 📝 |
| 124 | `resolvectl` | DNS 解析状态 | 待分类 | 📝 |
| 125 | `hostnamectl` | 主机名管理 | 待分类 | 📝 |
| 126 | `timedatectl` | 时区和时间同步 | 待分类 | 📝 |
| 127 | `localectl` | 语言和键盘布局 | 待分类 | 📝 |
| 128 | `git` | 版本控制 | 待分类 | 📝 |
| 129 | `docker` | 容器管理 | 待分类 | 📝 |
| 130 | `pgrep` | 按名称查进程号 | 待分类 | 📝 |
| 131 | `pkill` | 按名称结束进程 | 待分类 | 📝 |
| 132 | `nice` | 指定优先级启动 | 待分类 | 📝 |
| 133 | `renice` | 调整进程优先级 | 待分类 | 📝 |
| 134 | `nohup` | 退出终端后继续运行 | 待分类 | 📝 |
| 135 | `bg` | 作业转后台 | 待分类 | 📝 |
| 136 | `fg` | 作业转前台 | 待分类 | 📝 |
| 137 | `jobs` | 查看后台作业 | 待分类 | 📝 |
| 138 | `disown` | 让作业脱离 Shell | 待分类 | 📝 |
| 139 | `screen` | 终端会话保持 | 待分类 | 📝 |
| 140 | `tmux` | 终端复用 | 待分类 | 📝 |
| 141 | `lsof` | 查看打开的文件和端口 | 待分类 | 📝 |
| 142 | `fdisk` | 分区 | 待分类 | 📝 |
| 143 | `parted` | 分区（大磁盘） | 待分类 | 📝 |
| 144 | `mkfs` | 格式化 | 待分类 | 📝 |
| 145 | `blkid` | 查看分区 UUID | 待分类 | 📝 |
| 146 | `fsck` | 文件系统检查 | 待分类 | 📝 |
| 147 | `dmesg` | 内核消息 | 待分类 | 📝 |
| 148 | `vmstat` | 内存和 CPU 统计 | 待分类 | 📝 |
| 149 | `iostat` | 磁盘 IO 统计 | 待分类 | 📝 |
| 150 | `ssh-keygen` | 生成 SSH 密钥 | 待分类 | 📝 |
| 151 | `ssh-copy-id` | 上传公钥 | 待分类 | 📝 |
| 152 | `sftp` | 安全文件传输 | 待分类 | 📝 |
| 153 | `chgrp` | 修改所属组 | 待分类 | 📝 |
| 154 | `umask` | 默认权限掩码 | 待分类 | 📝 |
| 155 | `md5sum` | MD5 校验 | 待分类 | 📝 |
| 156 | `sha256sum` | SHA256 校验 | 待分类 | 📝 |
| 157 | `base64` | Base64 编解码 | 待分类 | 📝 |
| 158 | `xz` | xz 压缩 | 待分类 | 📝 |
| 159 | `bzip2` | bzip2 压缩 | 待分类 | 📝 |
| 160 | `7z` | 7-Zip 压缩 | 待分类 | 📝 |
| 161 | `zcat` | 直接查看 .gz | 待分类 | 📝 |
| 162 | `zgrep` | 在 .gz 中搜索 | 待分类 | 📝 |
| 163 | `column` | 按列对齐输出 | 待分类 | 📝 |
| 164 | `paste` | 按行合并文件 | 待分类 | 📝 |
| 165 | `join` | 按字段连接文件 | 待分类 | 📝 |
| 166 | `split` | 拆分大文件 | 待分类 | 📝 |
| 167 | `comm` | 比较两个有序文件 | 待分类 | 📝 |
| 168 | `nl` | 加行号 | 待分类 | 📝 |
| 169 | `rev` | 每行字符反转 | 待分类 | 📝 |
| 170 | `tac` | 行倒序输出 | 待分类 | 📝 |
| 171 | `printf` | 格式化输出 | 待分类 | 📝 |
| 172 | `read` | 读取输入 | 待分类 | 📝 |
| 173 | `test` | 条件判断 | 待分类 | 📝 |
| 174 | `seq` | 生成数字序列 | 待分类 | 📝 |
| 175 | `bc` | 计算器 | 待分类 | 📝 |
| 176 | `expr` | 表达式求值 | 待分类 | 📝 |
| 177 | `dd` | 块级复制、写镜像 | 待分类 | 📝 |
| 178 | `sync` | 把缓存写入磁盘 | 待分类 | 📝 |
| 179 | `apt-cache` | 查询软件包信息 | 待分类 | 📝 |
| 180 | `apt-mark` | 锁定或标记软件包 | 待分类 | 📝 |
| 181 | `add-apt-repository` | 添加软件源或 PPA | 待分类 | 📝 |
| 182 | `update-alternatives` | 多版本切换 | 待分类 | 📝 |
| 183 | `do-release-upgrade` | 升级发行版 | 待分类 | 📝 |
| 184 | `unattended-upgrade` | 自动安全更新 | 待分类 | 📝 |
| 185 | `flatpak` | Flatpak 包管理 | 待分类 | 📝 |
| 186 | `loginctl` | 登录会话管理 | 待分类 | 📝 |
| 187 | `chage` | 密码过期策略 | 待分类 | 📝 |
| 188 | `getent` | 查询用户、组、主机 | 待分类 | 📝 |
| 189 | `sudoedit` | 以 root 身份安全编辑 | 待分类 | 📝 |
| 190 | `systemd-analyze` | 开机耗时分析 | 待分类 | 📝 |
| 191 | `lsmod` | 已加载的内核模块 | 待分类 | 📝 |
| 192 | `modprobe` | 加载或卸载模块 | 待分类 | 📝 |
| 193 | `swapon` | 启用或查看交换区 | 待分类 | 📝 |
| 194 | `mkswap` | 创建交换区 | 待分类 | 📝 |
| 195 | `findmnt` | 挂载点树 | 待分类 | 📝 |
| 196 | `ncdu` | 交互式磁盘占用分析 | 待分类 | 📝 |
| 197 | `iotop` | 按进程看 IO | 待分类 | 📝 |
| 198 | `iftop` | 按连接看带宽 | 待分类 | 📝 |
| 199 | `ssh-agent` | SSH 密钥代理 | 待分类 | 📝 |
| 200 | `at` | 一次性定时任务 | 待分类 | 📝 |

### 高级指令（201–300）

| 排名 | 指令 | 用途 | 所在分类 | 状态 |
|---|---|---|---|---|
| 201 | `iptables` | 传统防火墙规则 | 待分类 | 📝 |
| 202 | `nft` | nftables 防火墙 | 待分类 | 📝 |
| 203 | `tcpdump` | 抓包 | 待分类 | 📝 |
| 204 | `strace` | 跟踪系统调用 | 待分类 | 📝 |
| 205 | `ltrace` | 跟踪库函数调用 | 待分类 | 📝 |
| 206 | `gdb` | 调试器 | 待分类 | 📝 |
| 207 | `perf` | 性能剖析 | 待分类 | 📝 |
| 208 | `lvcreate` | 创建 LVM 逻辑卷 | 待分类 | 📝 |
| 209 | `pvcreate` | 创建 LVM 物理卷 | 待分类 | 📝 |
| 210 | `vgcreate` | 创建 LVM 卷组 | 待分类 | 📝 |
| 211 | `lvextend` | 扩容逻辑卷 | 待分类 | 📝 |
| 212 | `resize2fs` | 调整 ext4 大小 | 待分类 | 📝 |
| 213 | `xfs_growfs` | 扩容 XFS | 待分类 | 📝 |
| 214 | `mdadm` | 软 RAID | 待分类 | 📝 |
| 215 | `cryptsetup` | LUKS 磁盘加密 | 待分类 | 📝 |
| 216 | `zfs` | ZFS 数据集管理 | 待分类 | 📝 |
| 217 | `zpool` | ZFS 存储池 | 待分类 | 📝 |
| 218 | `btrfs` | Btrfs 管理 | 待分类 | 📝 |
| 219 | `lxc` | LXD 容器和虚拟机 | 待分类 | 📝 |
| 220 | `virsh` | KVM 虚拟机管理 | 待分类 | 📝 |
| 221 | `qemu-img` | 虚拟磁盘镜像 | 待分类 | 📝 |
| 222 | `multipass` | 快速起 Ubuntu 虚拟机 | 待分类 | 📝 |
| 223 | `kubectl` | Kubernetes 管理 | 待分类 | 📝 |
| 224 | `microk8s` | 轻量 Kubernetes | 待分类 | 📝 |
| 225 | `podman` | 无守护进程容器 | 待分类 | 📝 |
| 226 | `docker compose` | 多容器编排 | 待分类 | 📝 |
| 227 | `openssl` | 证书和加密工具 | 待分类 | 📝 |
| 228 | `gpg` | 加密和签名 | 待分类 | 📝 |
| 229 | `certbot` | Let's Encrypt 证书 | 待分类 | 📝 |
| 230 | `aa-status` | AppArmor 状态 | 待分类 | 📝 |
| 231 | `aa-complain` | AppArmor 告警模式 | 待分类 | 📝 |
| 232 | `auditctl` | 审计规则 | 待分类 | 📝 |
| 233 | `ausearch` | 搜索审计日志 | 待分类 | 📝 |
| 234 | `fail2ban-client` | 防暴力破解 | 待分类 | 📝 |
| 235 | `nmap` | 端口扫描（仅限自有或授权主机） | 待分类 | 📝 |
| 236 | `setfacl` | 设置 ACL 权限 | 待分类 | 📝 |
| 237 | `getfacl` | 查看 ACL | 待分类 | 📝 |
| 238 | `chattr` | 文件特殊属性 | 待分类 | 📝 |
| 239 | `lsattr` | 查看特殊属性 | 待分类 | 📝 |
| 240 | `sysctl` | 内核参数 | 待分类 | 📝 |
| 241 | `ulimit` | 资源限制 | 待分类 | 📝 |
| 242 | `systemd-cgls` | cgroup 树 | 待分类 | 📝 |
| 243 | `systemd-run` | 临时单元运行命令 | 待分类 | 📝 |
| 244 | `systemd-cgtop` | 按 cgroup 看资源 | 待分类 | 📝 |
| 245 | `nsenter` | 进入命名空间 | 待分类 | 📝 |
| 246 | `unshare` | 创建新命名空间 | 待分类 | 📝 |
| 247 | `chroot` | 切换根目录 | 待分类 | 📝 |
| 248 | `debootstrap` | 构建最小 Ubuntu 根系统 | 待分类 | 📝 |
| 249 | `update-initramfs` | 重建 initramfs | 待分类 | 📝 |
| 250 | `update-grub` | 更新 GRUB 配置 | 待分类 | 📝 |
| 251 | `grub-install` | 安装引导器 | 待分类 | 📝 |
| 252 | `dkms` | 第三方内核模块管理 | 待分类 | 📝 |
| 253 | `mokutil` | 安全启动密钥 | 待分类 | 📝 |
| 254 | `efibootmgr` | UEFI 启动项 | 待分类 | 📝 |
| 255 | `smartctl` | 硬盘健康 | 待分类 | 📝 |
| 256 | `hdparm` | 磁盘参数和测速 | 待分类 | 📝 |
| 257 | `nvme` | NVMe 管理 | 待分类 | 📝 |
| 258 | `sar` | 历史性能数据 | 待分类 | 📝 |
| 259 | `pidstat` | 按进程统计性能 | 待分类 | 📝 |
| 260 | `bpftrace` | eBPF 动态追踪 | 待分类 | 📝 |
| 261 | `ethtool` | 网卡参数 | 待分类 | 📝 |
| 262 | `bridge` | 网桥管理 | 待分类 | 📝 |
| 263 | `tc` | 流量控制和限速 | 待分类 | 📝 |
| 264 | `wg` | WireGuard VPN | 待分类 | 📝 |
| 265 | `ipset` | IP 集合 | 待分类 | 📝 |
| 266 | `conntrack` | 连接跟踪表 | 待分类 | 📝 |
| 267 | `socat` | 多功能端口转发 | 待分类 | 📝 |
| 268 | `nc` | netcat 网络调试 | 待分类 | 📝 |
| 269 | `mtr` | ping 加 traceroute | 待分类 | 📝 |
| 270 | `iperf3` | 带宽测试 | 待分类 | 📝 |
| 271 | `dpkg-reconfigure` | 重新配置软件包 | 待分类 | 📝 |
| 272 | `dpkg-deb` | 构建和解包 .deb | 待分类 | 📝 |
| 273 | `dpkg-query` | 查询已装软件包 | 待分类 | 📝 |
| 274 | `debconf-set-selections` | 预置安装应答 | 待分类 | 📝 |
| 275 | `cloud-init` | 云主机初始化 | 待分类 | 📝 |
| 276 | `pro` | Ubuntu Pro 订阅和 ESM | 待分类 | 📝 |
| 277 | `ubuntu-drivers` | 驱动自动安装 | 待分类 | 📝 |
| 278 | `needrestart` | 检查需重启的服务 | 待分类 | 📝 |
| 279 | `logrotate` | 日志轮转 | 待分类 | 📝 |
| 280 | `logger` | 写入系统日志 | 待分类 | 📝 |
| 281 | `coredumpctl` | 崩溃转储 | 待分类 | 📝 |
| 282 | `busctl` | D-Bus 调试 | 待分类 | 📝 |
| 283 | `udevadm` | 设备规则和事件 | 待分类 | 📝 |
| 284 | `dmidecode` | BIOS 和硬件信息 | 待分类 | 📝 |
| 285 | `lshw` | 硬件清单 | 待分类 | 📝 |
| 286 | `sensors` | 温度和风扇 | 待分类 | 📝 |
| 287 | `cpupower` | CPU 频率策略 | 待分类 | 📝 |
| 288 | `numactl` | NUMA 绑定 | 待分类 | 📝 |
| 289 | `taskset` | CPU 亲和性 | 待分类 | 📝 |
| 290 | `ionice` | IO 优先级 | 待分类 | 📝 |
| 291 | `chrt` | 实时调度策略 | 待分类 | 📝 |
| 292 | `losetup` | 回环设备 | 待分类 | 📝 |
| 293 | `wipefs` | 清除分区签名 | 待分类 | 📝 |
| 294 | `sgdisk` | GPT 分区脚本化 | 待分类 | 📝 |
| 295 | `ansible` | 批量自动化运维 | 待分类 | 📝 |
| 296 | `sos` | 收集诊断报告 | 待分类 | 📝 |
| 297 | `apport-cli` | 崩溃报告 | 待分类 | 📝 |
| 298 | `snapcraft` | 构建 Snap 包 | 待分类 | 📝 |
| 299 | `kdump-config` | 内核崩溃转储配置 | 待分类 | 📝 |
| 300 | `bpftool` | eBPF 程序管理 | 待分类 | 📝 |


<!-- 总目录结束 -->
