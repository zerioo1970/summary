# Ubuntu 常用指令教程（300 个）

> 版本基线：**Ubuntu 26.04.1 LTS**（代号 resolute）｜bash 5.3.9｜Rust 版 coreutils（uutils）0.10.0，其中 cp、mv、rm、df 由 GNU coreutils 9.7 提供｜sudo-rs｜apt 3.2.0｜systemd 259｜OpenSSH 10.2p1
>
> 每个指令一页，固定结构：作用 · 基本格式 · 常用参数（表格）· 常用示例（带实测输出）· ⚠️ 踩坑警告 · 一句话总结。简单的指令省略用不上的部分。

---

## 这本教程写给谁

- 会写程序、熟悉数据库，但没有系统用过 Linux 命令行的人。
- 要在 Ubuntu 服务器上部署、排查问题，需要一份“查得到、看得懂、照着能跑”的指令手册。
- 不解释什么是文件、什么是进程；重点讲每个参数**什么时候用**、**容易错在哪**。

## 怎么读

| 路径 | 内容 | 时间 |
|---|---|---|
| 最短路径 | 初级 001–030：文件操作、看日志、装软件、查进程 | 约 3 小时 |
| 完整路径 | 300 个指令，按初级 → 中级 → 高级 | 每天 2 小时，约 6 周 |

排名按日常使用频率估算（没有官方统计数据），越靠前越常用。遇到具体问题时，直接按下面的目录查对应的指令即可，不必按顺序读。

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
| 基础命令（ls、cat、mkdir、chmod、chown 等）改由 Rust 重写的 uutils 提供 | 常用参数相同，部分报错文字和 GNU 版不同；个别行为有差异 | [chown](docs/beginner/024-chown.md)、[mkdir](docs/beginner/009-mkdir.md) |
| `sudo` 改由 sudo-rs 提供 | 输入密码时显示 `*`；没有 `-E` 参数 | [sudo](docs/beginner/004-sudo.md) |
| apt 3.x 的输出改为分块显示 | 安装前的摘要更清楚 | [apt](docs/beginner/012-apt.md) |
| ssh 默认由 `ssh.socket` 按需启动 | 改端口的步骤和以前不同 | [systemctl](docs/beginner/066-systemctl.md) |
| 登录记录改由 wtmpdb 保存 | `last` 读的是 `/var/log/wtmp.db` | [last](docs/beginner/074-last.md) |

## 进度

| 部分 | 状态 |
|---|---|
| **初级指令 001–100** | ✅ |
| 中级指令 101–200 | 📝 |
| 高级指令 201–300 | 📝 |

## 目录

<!-- 目录开始：由 tools/gen_readme.py 生成，请勿手改 -->

### 初级指令（1–100）

| 排名 | 指令 | 用途 | 状态 |
|---|---|---|---|
| 1 | [`ls`](docs/beginner/001-ls.md) | 列出目录内容 | ✅ |
| 2 | [`cd`](docs/beginner/002-cd.md) | 切换目录 | ✅ |
| 3 | [`pwd`](docs/beginner/003-pwd.md) | 显示当前目录 | ✅ |
| 4 | [`sudo`](docs/beginner/004-sudo.md) | 以管理员身份执行 | ✅ |
| 5 | [`cat`](docs/beginner/005-cat.md) | 查看文件内容 | ✅ |
| 6 | [`cp`](docs/beginner/006-cp.md) | 复制文件和目录 | ✅ |
| 7 | [`mv`](docs/beginner/007-mv.md) | 移动或重命名 | ✅ |
| 8 | [`rm`](docs/beginner/008-rm.md) | 删除文件和目录 | ✅ |
| 9 | [`mkdir`](docs/beginner/009-mkdir.md) | 新建目录 | ✅ |
| 10 | [`touch`](docs/beginner/010-touch.md) | 新建空文件或修改时间 | ✅ |
| 11 | [`nano`](docs/beginner/011-nano.md) | 简易文本编辑器 | ✅ |
| 12 | [`apt`](docs/beginner/012-apt.md) | 安装、升级、卸载软件 | ✅ |
| 13 | [`clear`](docs/beginner/013-clear.md) | 清屏 | ✅ |
| 14 | [`echo`](docs/beginner/014-echo.md) | 输出一行文本 | ✅ |
| 15 | [`man`](docs/beginner/015-man.md) | 查看命令手册 | ✅ |
| 16 | [`history`](docs/beginner/016-history.md) | 查看命令历史 | ✅ |
| 17 | [`exit`](docs/beginner/017-exit.md) | 退出 Shell 或脚本 | ✅ |
| 18 | [`less`](docs/beginner/018-less.md) | 分页查看长文件 | ✅ |
| 19 | [`head`](docs/beginner/019-head.md) | 查看文件开头 | ✅ |
| 20 | [`tail`](docs/beginner/020-tail.md) | 查看文件末尾、跟踪日志 | ✅ |
| 21 | [`grep`](docs/beginner/021-grep.md) | 按关键词搜索文本 | ✅ |
| 22 | [`find`](docs/beginner/022-find.md) | 按条件查找文件 | ✅ |
| 23 | [`chmod`](docs/beginner/023-chmod.md) | 修改文件权限 | ✅ |
| 24 | [`chown`](docs/beginner/024-chown.md) | 修改所有者和所属组 | ✅ |
| 25 | [`df`](docs/beginner/025-df.md) | 查看磁盘空间 | ✅ |
| 26 | [`du`](docs/beginner/026-du.md) | 查看目录占用的空间 | ✅ |
| 27 | [`free`](docs/beginner/027-free.md) | 查看内存 | ✅ |
| 28 | [`top`](docs/beginner/028-top.md) | 实时查看负载和进程 | ✅ |
| 29 | [`ps`](docs/beginner/029-ps.md) | 查看进程列表 | ✅ |
| 30 | [`kill`](docs/beginner/030-kill.md) | 结束进程 | ✅ |
| 31 | [`ping`](docs/beginner/031-ping.md) | 测试网络连通 | ✅ |
| 32 | [`ip`](docs/beginner/032-ip.md) | 查看 IP、网卡和路由 | ✅ |
| 33 | [`wget`](docs/beginner/033-wget.md) | 下载文件 | ✅ |
| 34 | [`curl`](docs/beginner/034-curl.md) | 发送 HTTP 请求 | ✅ |
| 35 | [`tar`](docs/beginner/035-tar.md) | 打包和解包 | ✅ |
| 36 | [`zip`](docs/beginner/036-zip.md) | 压缩成 zip | ✅ |
| 37 | [`unzip`](docs/beginner/037-unzip.md) | 解压 zip | ✅ |
| 38 | [`reboot`](docs/beginner/038-reboot.md) | 重启系统 | ✅ |
| 39 | [`shutdown`](docs/beginner/039-shutdown.md) | 关机或定时重启 | ✅ |
| 40 | [`whoami`](docs/beginner/040-whoami.md) | 显示当前用户名 | ✅ |
| 41 | [`uname`](docs/beginner/041-uname.md) | 查看内核和系统信息 | ✅ |
| 42 | [`date`](docs/beginner/042-date.md) | 显示和计算日期时间 | ✅ |
| 43 | [`cal`](docs/beginner/043-cal.md) | 显示日历 | ✅ |
| 44 | [`which`](docs/beginner/044-which.md) | 查命令对应的程序位置 | ✅ |
| 45 | [`whereis`](docs/beginner/045-whereis.md) | 查程序和手册的位置 | ✅ |
| 46 | [`file`](docs/beginner/046-file.md) | 判断文件类型 | ✅ |
| 47 | [`wc`](docs/beginner/047-wc.md) | 统计行数、字数 | ✅ |
| 48 | [`sort`](docs/beginner/048-sort.md) | 排序 | ✅ |
| 49 | [`uniq`](docs/beginner/049-uniq.md) | 去重和计数 | ✅ |
| 50 | [`rmdir`](docs/beginner/050-rmdir.md) | 删除空目录 | ✅ |
| 51 | [`ln`](docs/beginner/051-ln.md) | 创建链接 | ✅ |
| 52 | [`more`](docs/beginner/052-more.md) | 简单的分页查看 | ✅ |
| 53 | [`passwd`](docs/beginner/053-passwd.md) | 修改密码 | ✅ |
| 54 | [`su`](docs/beginner/054-su.md) | 切换到其他用户 | ✅ |
| 55 | [`hostname`](docs/beginner/055-hostname.md) | 查看主机名和本机 IP | ✅ |
| 56 | [`uptime`](docs/beginner/056-uptime.md) | 查看运行时长和负载 | ✅ |
| 57 | [`lsb_release`](docs/beginner/057-lsb_release.md) | 查看 Ubuntu 版本 | ✅ |
| 58 | [`vim`](docs/beginner/058-vim.md) | 全功能的终端编辑器 | ✅ |
| 59 | [`tree`](docs/beginner/059-tree.md) | 树状显示目录 | ✅ |
| 60 | [`alias`](docs/beginner/060-alias.md) | 给命令起别名 | ✅ |
| 61 | [`type`](docs/beginner/061-type.md) | 查看命令的真实身份 | ✅ |
| 62 | [`help`](docs/beginner/062-help.md) | bash 内建命令的帮助 | ✅ |
| 63 | [`apt-get`](docs/beginner/063-apt-get.md) | 脚本里用的包管理命令 | ✅ |
| 64 | [`dpkg`](docs/beginner/064-dpkg.md) | 安装和查询 .deb 包 | ✅ |
| 65 | [`snap`](docs/beginner/065-snap.md) | 管理 Snap 软件包 | ✅ |
| 66 | [`systemctl`](docs/beginner/066-systemctl.md) | 管理系统服务 | ✅ |
| 67 | [`lsblk`](docs/beginner/067-lsblk.md) | 列出磁盘和分区 | ✅ |
| 68 | [`mount`](docs/beginner/068-mount.md) | 挂载磁盘和分区 | ✅ |
| 69 | [`umount`](docs/beginner/069-umount.md) | 卸载磁盘和分区 | ✅ |
| 70 | [`id`](docs/beginner/070-id.md) | 查看 UID 和所属组 | ✅ |
| 71 | [`groups`](docs/beginner/071-groups.md) | 查看所属组 | ✅ |
| 72 | [`who`](docs/beginner/072-who.md) | 查看谁登录了系统 | ✅ |
| 73 | [`w`](docs/beginner/073-w.md) | 查看谁在线、在做什么 | ✅ |
| 74 | [`last`](docs/beginner/074-last.md) | 查看登录历史 | ✅ |
| 75 | [`env`](docs/beginner/075-env.md) | 查看环境变量 | ✅ |
| 76 | [`export`](docs/beginner/076-export.md) | 设置环境变量 | ✅ |
| 77 | [`source`](docs/beginner/077-source.md) | 在当前 Shell 里执行脚本 | ✅ |
| 78 | [`diff`](docs/beginner/078-diff.md) | 比较文件差异 | ✅ |
| 79 | [`cut`](docs/beginner/079-cut.md) | 按列截取文本 | ✅ |
| 80 | [`tee`](docs/beginner/080-tee.md) | 同时输出到屏幕和文件 | ✅ |
| 81 | [`gzip`](docs/beginner/081-gzip.md) | 压缩单个文件 | ✅ |
| 82 | [`gunzip`](docs/beginner/082-gunzip.md) | 解压 .gz 文件 | ✅ |
| 83 | [`ssh`](docs/beginner/083-ssh.md) | 远程登录服务器 | ✅ |
| 84 | [`scp`](docs/beginner/084-scp.md) | 远程复制文件 | ✅ |
| 85 | [`htop`](docs/beginner/085-htop.md) | 更好用的进程监控 | ✅ |
| 86 | [`fastfetch`](docs/beginner/086-fastfetch.md) | 一屏看清系统概况 | ✅ |
| 87 | [`sleep`](docs/beginner/087-sleep.md) | 暂停一段时间 | ✅ |
| 88 | [`watch`](docs/beginner/088-watch.md) | 定时重复执行命令 | ✅ |
| 89 | [`time`](docs/beginner/089-time.md) | 统计命令执行时间 | ✅ |
| 90 | [`basename`](docs/beginner/090-basename.md) | 从路径取文件名 | ✅ |
| 91 | [`dirname`](docs/beginner/091-dirname.md) | 从路径取目录 | ✅ |
| 92 | [`stat`](docs/beginner/092-stat.md) | 查看文件详细属性 | ✅ |
| 93 | [`locate`](docs/beginner/093-locate.md) | 按文件名快速查找 | ✅ |
| 94 | [`updatedb`](docs/beginner/094-updatedb.md) | 更新 locate 的数据库 | ✅ |
| 95 | [`lscpu`](docs/beginner/095-lscpu.md) | 查看 CPU 信息 | ✅ |
| 96 | [`lsusb`](docs/beginner/096-lsusb.md) | 列出 USB 设备 | ✅ |
| 97 | [`lspci`](docs/beginner/097-lspci.md) | 列出 PCI 设备 | ✅ |
| 98 | [`nproc`](docs/beginner/098-nproc.md) | 显示 CPU 数量 | ✅ |
| 99 | [`killall`](docs/beginner/099-killall.md) | 按程序名结束进程 | ✅ |
| 100 | [`xdg-open`](docs/beginner/100-xdg-open.md) | 用默认程序打开文件 | ✅ |

### 中级指令（101–200）

📝 待写。

| 排名 | 指令 | 用途 | 状态 |
|---|---|---|---|
| 101 | `journalctl` | 查看 systemd 日志 | 📝 |
| 102 | `sed` | 流式文本替换 | 📝 |
| 103 | `awk` | 按列处理文本 | 📝 |
| 104 | `xargs` | 把输入转为参数 | 📝 |
| 105 | `tr` | 字符替换和删除 | 📝 |
| 106 | `ufw` | 简易防火墙 | 📝 |
| 107 | `useradd` | 新建用户（底层） | 📝 |
| 108 | `adduser` | 新建用户（交互式） | 📝 |
| 109 | `usermod` | 修改用户 | 📝 |
| 110 | `userdel` | 删除用户（底层） | 📝 |
| 111 | `deluser` | 删除用户（交互式） | 📝 |
| 112 | `groupadd` | 新建组 | 📝 |
| 113 | `visudo` | 安全编辑 sudoers | 📝 |
| 114 | `crontab` | 定时任务 | 📝 |
| 115 | `rsync` | 增量同步 | 📝 |
| 116 | `ss` | 查看端口和连接 | 📝 |
| 117 | `netstat` | 旧版端口查看 | 📝 |
| 118 | `dig` | DNS 查询 | 📝 |
| 119 | `nslookup` | DNS 查询 | 📝 |
| 120 | `traceroute` | 路由追踪 | 📝 |
| 121 | `tracepath` | 路由追踪（免 root） | 📝 |
| 122 | `nmcli` | NetworkManager 命令行 | 📝 |
| 123 | `netplan` | 网络配置 | 📝 |
| 124 | `resolvectl` | DNS 解析状态 | 📝 |
| 125 | `hostnamectl` | 主机名管理 | 📝 |
| 126 | `timedatectl` | 时区和时间同步 | 📝 |
| 127 | `localectl` | 语言和键盘布局 | 📝 |
| 128 | `git` | 版本控制 | 📝 |
| 129 | `docker` | 容器管理 | 📝 |
| 130 | `pgrep` | 按名称查进程号 | 📝 |
| 131 | `pkill` | 按名称结束进程 | 📝 |
| 132 | `nice` | 指定优先级启动 | 📝 |
| 133 | `renice` | 调整进程优先级 | 📝 |
| 134 | `nohup` | 退出终端后继续运行 | 📝 |
| 135 | `bg` | 作业转后台 | 📝 |
| 136 | `fg` | 作业转前台 | 📝 |
| 137 | `jobs` | 查看后台作业 | 📝 |
| 138 | `disown` | 让作业脱离 Shell | 📝 |
| 139 | `screen` | 终端会话保持 | 📝 |
| 140 | `tmux` | 终端复用 | 📝 |
| 141 | `lsof` | 查看打开的文件和端口 | 📝 |
| 142 | `fdisk` | 分区 | 📝 |
| 143 | `parted` | 分区（大磁盘） | 📝 |
| 144 | `mkfs` | 格式化 | 📝 |
| 145 | `blkid` | 查看分区 UUID | 📝 |
| 146 | `fsck` | 文件系统检查 | 📝 |
| 147 | `dmesg` | 内核消息 | 📝 |
| 148 | `vmstat` | 内存和 CPU 统计 | 📝 |
| 149 | `iostat` | 磁盘 IO 统计 | 📝 |
| 150 | `ssh-keygen` | 生成 SSH 密钥 | 📝 |
| 151 | `ssh-copy-id` | 上传公钥 | 📝 |
| 152 | `sftp` | 安全文件传输 | 📝 |
| 153 | `chgrp` | 修改所属组 | 📝 |
| 154 | `umask` | 默认权限掩码 | 📝 |
| 155 | `md5sum` | MD5 校验 | 📝 |
| 156 | `sha256sum` | SHA256 校验 | 📝 |
| 157 | `base64` | Base64 编解码 | 📝 |
| 158 | `xz` | xz 压缩 | 📝 |
| 159 | `bzip2` | bzip2 压缩 | 📝 |
| 160 | `7z` | 7-Zip 压缩 | 📝 |
| 161 | `zcat` | 直接查看 .gz | 📝 |
| 162 | `zgrep` | 在 .gz 中搜索 | 📝 |
| 163 | `column` | 按列对齐输出 | 📝 |
| 164 | `paste` | 按行合并文件 | 📝 |
| 165 | `join` | 按字段连接文件 | 📝 |
| 166 | `split` | 拆分大文件 | 📝 |
| 167 | `comm` | 比较两个有序文件 | 📝 |
| 168 | `nl` | 加行号 | 📝 |
| 169 | `rev` | 每行字符反转 | 📝 |
| 170 | `tac` | 行倒序输出 | 📝 |
| 171 | `printf` | 格式化输出 | 📝 |
| 172 | `read` | 读取输入 | 📝 |
| 173 | `test` | 条件判断 | 📝 |
| 174 | `seq` | 生成数字序列 | 📝 |
| 175 | `bc` | 计算器 | 📝 |
| 176 | `expr` | 表达式求值 | 📝 |
| 177 | `dd` | 块级复制、写镜像 | 📝 |
| 178 | `sync` | 把缓存写入磁盘 | 📝 |
| 179 | `apt-cache` | 查询软件包信息 | 📝 |
| 180 | `apt-mark` | 锁定或标记软件包 | 📝 |
| 181 | `add-apt-repository` | 添加软件源或 PPA | 📝 |
| 182 | `update-alternatives` | 多版本切换 | 📝 |
| 183 | `do-release-upgrade` | 升级发行版 | 📝 |
| 184 | `unattended-upgrade` | 自动安全更新 | 📝 |
| 185 | `flatpak` | Flatpak 包管理 | 📝 |
| 186 | `loginctl` | 登录会话管理 | 📝 |
| 187 | `chage` | 密码过期策略 | 📝 |
| 188 | `getent` | 查询用户、组、主机 | 📝 |
| 189 | `sudoedit` | 以 root 身份安全编辑 | 📝 |
| 190 | `systemd-analyze` | 开机耗时分析 | 📝 |
| 191 | `lsmod` | 已加载的内核模块 | 📝 |
| 192 | `modprobe` | 加载或卸载模块 | 📝 |
| 193 | `swapon` | 启用或查看交换区 | 📝 |
| 194 | `mkswap` | 创建交换区 | 📝 |
| 195 | `findmnt` | 挂载点树 | 📝 |
| 196 | `ncdu` | 交互式磁盘占用分析 | 📝 |
| 197 | `iotop` | 按进程看 IO | 📝 |
| 198 | `iftop` | 按连接看带宽 | 📝 |
| 199 | `ssh-agent` | SSH 密钥代理 | 📝 |
| 200 | `at` | 一次性定时任务 | 📝 |

### 高级指令（201–300）

📝 待写。

| 排名 | 指令 | 用途 | 状态 |
|---|---|---|---|
| 201 | `iptables` | 传统防火墙规则 | 📝 |
| 202 | `nft` | nftables 防火墙 | 📝 |
| 203 | `tcpdump` | 抓包 | 📝 |
| 204 | `strace` | 跟踪系统调用 | 📝 |
| 205 | `ltrace` | 跟踪库函数调用 | 📝 |
| 206 | `gdb` | 调试器 | 📝 |
| 207 | `perf` | 性能剖析 | 📝 |
| 208 | `lvcreate` | 创建 LVM 逻辑卷 | 📝 |
| 209 | `pvcreate` | 创建 LVM 物理卷 | 📝 |
| 210 | `vgcreate` | 创建 LVM 卷组 | 📝 |
| 211 | `lvextend` | 扩容逻辑卷 | 📝 |
| 212 | `resize2fs` | 调整 ext4 大小 | 📝 |
| 213 | `xfs_growfs` | 扩容 XFS | 📝 |
| 214 | `mdadm` | 软 RAID | 📝 |
| 215 | `cryptsetup` | LUKS 磁盘加密 | 📝 |
| 216 | `zfs` | ZFS 数据集管理 | 📝 |
| 217 | `zpool` | ZFS 存储池 | 📝 |
| 218 | `btrfs` | Btrfs 管理 | 📝 |
| 219 | `lxc` | LXD 容器和虚拟机 | 📝 |
| 220 | `virsh` | KVM 虚拟机管理 | 📝 |
| 221 | `qemu-img` | 虚拟磁盘镜像 | 📝 |
| 222 | `multipass` | 快速起 Ubuntu 虚拟机 | 📝 |
| 223 | `kubectl` | Kubernetes 管理 | 📝 |
| 224 | `microk8s` | 轻量 Kubernetes | 📝 |
| 225 | `podman` | 无守护进程容器 | 📝 |
| 226 | `docker compose` | 多容器编排 | 📝 |
| 227 | `openssl` | 证书和加密工具 | 📝 |
| 228 | `gpg` | 加密和签名 | 📝 |
| 229 | `certbot` | Let's Encrypt 证书 | 📝 |
| 230 | `aa-status` | AppArmor 状态 | 📝 |
| 231 | `aa-complain` | AppArmor 告警模式 | 📝 |
| 232 | `auditctl` | 审计规则 | 📝 |
| 233 | `ausearch` | 搜索审计日志 | 📝 |
| 234 | `fail2ban-client` | 防暴力破解 | 📝 |
| 235 | `nmap` | 端口扫描（仅限自有或授权主机） | 📝 |
| 236 | `setfacl` | 设置 ACL 权限 | 📝 |
| 237 | `getfacl` | 查看 ACL | 📝 |
| 238 | `chattr` | 文件特殊属性 | 📝 |
| 239 | `lsattr` | 查看特殊属性 | 📝 |
| 240 | `sysctl` | 内核参数 | 📝 |
| 241 | `ulimit` | 资源限制 | 📝 |
| 242 | `systemd-cgls` | cgroup 树 | 📝 |
| 243 | `systemd-run` | 临时单元运行命令 | 📝 |
| 244 | `systemd-cgtop` | 按 cgroup 看资源 | 📝 |
| 245 | `nsenter` | 进入命名空间 | 📝 |
| 246 | `unshare` | 创建新命名空间 | 📝 |
| 247 | `chroot` | 切换根目录 | 📝 |
| 248 | `debootstrap` | 构建最小 Ubuntu 根系统 | 📝 |
| 249 | `update-initramfs` | 重建 initramfs | 📝 |
| 250 | `update-grub` | 更新 GRUB 配置 | 📝 |
| 251 | `grub-install` | 安装引导器 | 📝 |
| 252 | `dkms` | 第三方内核模块管理 | 📝 |
| 253 | `mokutil` | 安全启动密钥 | 📝 |
| 254 | `efibootmgr` | UEFI 启动项 | 📝 |
| 255 | `smartctl` | 硬盘健康 | 📝 |
| 256 | `hdparm` | 磁盘参数和测速 | 📝 |
| 257 | `nvme` | NVMe 管理 | 📝 |
| 258 | `sar` | 历史性能数据 | 📝 |
| 259 | `pidstat` | 按进程统计性能 | 📝 |
| 260 | `bpftrace` | eBPF 动态追踪 | 📝 |
| 261 | `ethtool` | 网卡参数 | 📝 |
| 262 | `bridge` | 网桥管理 | 📝 |
| 263 | `tc` | 流量控制和限速 | 📝 |
| 264 | `wg` | WireGuard VPN | 📝 |
| 265 | `ipset` | IP 集合 | 📝 |
| 266 | `conntrack` | 连接跟踪表 | 📝 |
| 267 | `socat` | 多功能端口转发 | 📝 |
| 268 | `nc` | netcat 网络调试 | 📝 |
| 269 | `mtr` | ping 加 traceroute | 📝 |
| 270 | `iperf3` | 带宽测试 | 📝 |
| 271 | `dpkg-reconfigure` | 重新配置软件包 | 📝 |
| 272 | `dpkg-deb` | 构建和解包 .deb | 📝 |
| 273 | `dpkg-query` | 查询已装软件包 | 📝 |
| 274 | `debconf-set-selections` | 预置安装应答 | 📝 |
| 275 | `cloud-init` | 云主机初始化 | 📝 |
| 276 | `pro` | Ubuntu Pro 订阅和 ESM | 📝 |
| 277 | `ubuntu-drivers` | 驱动自动安装 | 📝 |
| 278 | `needrestart` | 检查需重启的服务 | 📝 |
| 279 | `logrotate` | 日志轮转 | 📝 |
| 280 | `logger` | 写入系统日志 | 📝 |
| 281 | `coredumpctl` | 崩溃转储 | 📝 |
| 282 | `busctl` | D-Bus 调试 | 📝 |
| 283 | `udevadm` | 设备规则和事件 | 📝 |
| 284 | `dmidecode` | BIOS 和硬件信息 | 📝 |
| 285 | `lshw` | 硬件清单 | 📝 |
| 286 | `sensors` | 温度和风扇 | 📝 |
| 287 | `cpupower` | CPU 频率策略 | 📝 |
| 288 | `numactl` | NUMA 绑定 | 📝 |
| 289 | `taskset` | CPU 亲和性 | 📝 |
| 290 | `ionice` | IO 优先级 | 📝 |
| 291 | `chrt` | 实时调度策略 | 📝 |
| 292 | `losetup` | 回环设备 | 📝 |
| 293 | `wipefs` | 清除分区签名 | 📝 |
| 294 | `sgdisk` | GPT 分区脚本化 | 📝 |
| 295 | `ansible` | 批量自动化运维 | 📝 |
| 296 | `sos` | 收集诊断报告 | 📝 |
| 297 | `apport-cli` | 崩溃报告 | 📝 |
| 298 | `snapcraft` | 构建 Snap 包 | 📝 |
| 299 | `kdump-config` | 内核崩溃转储配置 | 📝 |
| 300 | `bpftool` | eBPF 程序管理 | 📝 |

<!-- 目录结束 -->
