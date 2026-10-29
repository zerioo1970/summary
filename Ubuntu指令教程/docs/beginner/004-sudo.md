# 004 sudo：以管理员身份执行命令

> 级别：初级 · 软件包：`sudo-rs`（Ubuntu 26.04 默认的 sudo 实现）

## 1. 作用

`sudo`（superuser do）让普通用户临时以 root（管理员）身份执行一条命令。装软件、改 `/etc` 下的配置、重启服务，都需要在命令前加 sudo。

Ubuntu 默认不给 root 设密码，也不建议直接登录 root。日常用普通账号，需要管理员权限时才加 sudo，这样每一次提权都有记录，误操作的范围也限定在这一条命令里。

从 Ubuntu 25.10 起，`sudo` 由 Rust 重写的 sudo-rs 提供。在 Ubuntu 26.04 上可以这样确认：

```
$ ls -l /usr/bin/sudo /etc/alternatives/sudo
lrwxrwxrwx 1 root root 23 Oct 29 06:36 /etc/alternatives/sudo -> /usr/lib/cargo/bin/sudo
lrwxrwxrwx 1 root root 22 Oct  6 13:13 /usr/bin/sudo -> /etc/alternatives/sudo
```

## 2. 基本格式

```bash
sudo [参数] 命令 [命令的参数]
```

谁能用 sudo，由 `/etc/sudoers` 决定。Ubuntu 默认的规则是 `sudo` 组的成员可以执行任何命令：

```
%sudo	ALL=(ALL:ALL) ALL
```

安装系统时创建的第一个用户自动在 sudo 组里。用 `groups` 可以确认：

```
$ groups
tom sudo
```

## 3. 常用参数

| 参数 | 长格式 | 作用 |
|---|---|---|
| `-i` | `--login` | 切换成 root 的登录 Shell，连续执行多条管理命令 |
| `-u 用户` | `--user` | 以指定用户（不一定是 root）的身份执行 |
| `-l` | `--list` | 列出自己被允许执行哪些命令 |
| `-k` | `--reset-timestamp` | 让密码缓存立即失效，下次 sudo 重新问密码 |
| `-e` | `--edit` | 以 root 权限编辑文件，等同 `sudoedit` |
| `-s` | `--shell` | 以 root 身份启动当前的 Shell（不读取 root 的登录配置） |

## 4. 常用示例

> ⚠️ 本节未实机验证：测试环境的容器禁止提权（见第 5 节最后一条），sudo 无法真正执行。命令写法已对照 sudo-rs 手册核实。

### 4.1 执行一条管理命令

```bash
sudo apt update
```

第一次执行时要输入**你自己的密码**（不是 root 的密码）。Ubuntu 26.04 的 sudo-rs 默认开启了 `pwfeedback`，输入时每个字符显示一个 `*`。旧版 sudo 输入时屏幕上没有任何显示。

验证通过后，**同一个终端** 15 分钟内再用 sudo 不会再问密码。sudo-rs 按终端分别计时，在另一个终端窗口里还是要输入一次。

### 4.2 `sudo -i`：连续执行多条管理命令

```bash
sudo -i          # 提示符从 $ 变成 #，表示现在是 root
apt update
apt upgrade
exit             # 退回普通用户
```

需要连着做十几步管理操作时，比每条都加 sudo 方便。做完一定要 `exit`，不要一直停在 root 下干日常的事。

### 4.3 `sudo -u`：以其他用户身份执行

```bash
sudo -u www-data ls /var/www/html
```

排查“网站进程读不到某个文件”时，用网站运行账号（www-data）的身份试一下，就能确认是不是权限问题。

### 4.4 忘了加 sudo，补上

```bash
apt install tree        # 报权限错误
sudo !!                 # !! 代表上一条命令，相当于 sudo apt install tree
```

`!!` 是 bash 的历史展开功能，不是 sudo 的参数。

### 4.5 写入受保护的文件

```bash
echo "192.168.1.10 db01" | sudo tee -a /etc/hosts
```

不能写成 `sudo echo "..." >> /etc/hosts`，原因见下面的踩坑表。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `sudo echo "x" >> /etc/hosts` 报 `Permission denied` | 重定向 `>>` 是当前 Shell（普通用户）做的，sudo 只提升了 echo 的权限 | 用 `echo "x" \| sudo tee -a /etc/hosts` |
| 网上教程里的 `sudo -E` 用不了 | sudo-rs 手册里没有 `-E`（保留全部环境变量），只有 `--preserve-env=变量列表` | 改用 `sudo --preserve-env=HTTP_PROXY 命令`，或在命令前写 `sudo 变量=值 命令` |
| 在 `sudo` 下运行的程序找不到自己装的命令 | sudoers 里的 `secure_path` 会重设 PATH，只包含 `/usr/sbin`、`/usr/bin`、`/snap/bin` 等系统目录 | 写命令的完整路径，例如 `sudo /opt/app/bin/tool` |
| 直接用 `vi /etc/sudoers` 改错一个字符，之后所有 sudo 都失败 | sudoers 语法错误会让 sudo 拒绝工作 | 一律用 `sudo visudo` 编辑，它会在保存前检查语法 |
| 容器里报 `The "no new privileges" flag is set, which prevents sudo from running as root.` | 容器启动时设置了禁止提权的标志 | 在容器里直接以 root 运行命令，或调整容器配置 |

## 6. 一句话总结

管理操作前加 `sudo`，连续操作用 `sudo -i`，写系统文件用 `sudo tee`。
