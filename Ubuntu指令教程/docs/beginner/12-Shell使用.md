# 初级 · 12 Shell使用（13 个）

> 清屏、历史、别名、环境变量、查帮助。每个指令的结构：作用 · 基本格式 · 常用参数 · 常用示例 · ⚠️ 踩坑警告 · 一句话总结。标题前的三位数字是使用频率排名。演示环境见 [总目录](../../README.md#演示环境)。

<a id="本类目录"></a>

## 本类目录

| 排名 | 指令 | 用途 |
|---|---|---|
| 13 | [`clear`](#cmd-clear) | 清屏 |
| 15 | [`man`](#cmd-man) | 查看命令手册 |
| 16 | [`history`](#cmd-history) | 查看命令历史 |
| 17 | [`exit`](#cmd-exit) | 退出 Shell 或脚本 |
| 44 | [`which`](#cmd-which) | 查命令对应的程序位置 |
| 45 | [`whereis`](#cmd-whereis) | 查程序和手册的位置 |
| 60 | [`alias`](#cmd-alias) | 给命令起别名 |
| 61 | [`type`](#cmd-type) | 查看命令的真实身份 |
| 62 | [`help`](#cmd-help) | bash 内建命令的帮助 |
| 75 | [`env`](#cmd-env) | 查看环境变量 |
| 76 | [`export`](#cmd-export) | 设置环境变量 |
| 77 | [`source`](#cmd-source) | 在当前 Shell 里执行脚本 |
| 100 | [`xdg-open`](#cmd-xdg-open) | 用默认程序打开文件 |

---

<a id="cmd-clear"></a>

## 013 clear：清屏

> 级别：初级 · 软件包：`ncurses-bin`（系统自带）

### 1. 作用

`clear` 清掉终端屏幕上的内容，光标回到左上角。屏幕被大量输出刷满、想从干净的界面开始下一步操作时用它。它只影响显示，不会删除命令历史，也不影响正在运行的程序。

### 2. 基本格式

```bash
clear [参数]
```

### 3. 常用参数和快捷键

| 写法 | 作用 |
|---|---|
| `clear` | 清屏，同时清掉往上滚动能看到的历史输出 |
| `clear -x` | 只清当前屏幕，往上滚动还能看到之前的输出 |
| `Ctrl+L` | 快捷键，效果和 `clear -x` 相同，正在输入的命令不会丢 |
| `reset` | 终端显示乱码、按键没反应时，把终端恢复到初始状态 |

### 4. 常用示例

```bash
clear
```

执行后屏幕变空，只剩一个提示符。

不小心用 `cat` 打开了一个二进制文件，屏幕满是乱码、输入的字也显示成怪字符，这时 clear 不够用，要输入 `reset` 回车（即使看不清输入的内容，照样敲完回车）。

> ⚠️ 本页未实机验证：清屏效果需要真实终端才能看到，测试环境只能确认命令存在（`/usr/bin/clear`）。

### 5. 一句话总结

清屏用 `Ctrl+L`，终端乱码用 `reset`。

[↑ 回到本类目录](#本类目录)

---

<a id="cmd-man"></a>

## 015 man：查看命令手册

> 级别：初级 · 软件包：`man-db`

### 1. 作用

`man`（manual）打开一条命令的官方手册，里面列出了全部参数和用法说明。本教程每条命令只讲常用参数，需要查不常用的参数时，就用 man。

手册是英文的。打开后用的是 [less](02-文件查看和编辑.md#cmd-less) 的翻页方式：空格翻页，`/关键词` 搜索，`q` 退出。

### 2. 基本格式

```bash
man [章节号] 命令名
```

### 3. 常用参数

| 写法 | 作用 |
|---|---|
| `man 命令` | 打开手册 |
| `man 5 文件名` | 打开第 5 章（配置文件格式）的手册 |
| `man -f 命令` | 一行说明这条命令是做什么的，等同 `whatis` |
| `man -k 关键词` | 按关键词搜索相关命令，等同 `apropos` |
| `命令 --help` | 不是 man 的参数，但大多数命令都支持，输出一份简短的参数说明 |

手册分章节，常用的是这三章：

| 章节 | 内容 | 例子 |
|---|---|---|
| 1 | 普通命令 | `man 1 passwd`：passwd 命令怎么用 |
| 5 | 配置文件格式 | `man 5 passwd`：/etc/passwd 文件每一列是什么 |
| 8 | 系统管理命令 | `man 8 useradd` |

### 4. 常用示例

#### 4.1 打开手册

```
$ man ls
LS(1)                       General Commands Manual                       LS(1)

NAME
       ls  -  List  directory  contents.  Ignore files and directories starting
       with a '.' by default

SYNOPSIS
       ls [--help] [--format] [-C ] [-l|--long]  [-x  ]  [-T|--tabsize]  [-m  ]
...
```

| 手册里的小节 | 内容 |
|---|---|
| NAME | 一句话说明 |
| SYNOPSIS | 用法格式，`[]` 里是可选项 |
| DESCRIPTION / OPTIONS | 每个参数的说明 |
| EXAMPLES | 示例（不是每个手册都有） |

在手册里按 `/` 输入 `-h` 回车，可以直接跳到 `-h` 参数的说明，按 `n` 找下一处。

#### 4.2 `-f`：一句话查用途

```
$ man -f passwd
passwd (1)           - change user password
passwd (5)           - the password file
```

同名的有命令也有配置文件，所以出现了两个章节。

#### 4.3 `-k`：忘了命令名，按功能找

```
$ man -k "copy files"
gnucp (1)            - copy files and directories
gnuinstall (1)       - copy files and set attributes
```

#### 4.4 `--help`：快速看参数

```
$ ls --help | head -6
List directory contents.
Ignore files and directories starting with a '.' by default

Usage: ls [OPTION]... [FILE]...

Options:
```

比打开整本手册快，适合“我记得有个参数，但忘了是哪个字母”。

### 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| man 输出 `This system has been minimized by removing packages and content that are not required on a system that users do not log into.` | 云服务器、容器常用最小化镜像，删掉了手册 | 执行 `sudo unminimize` 恢复，或者直接用 `命令 --help` |
| `No manual entry for xxx` | 命令没装，或者它是 bash 内建命令（如 cd） | 内建命令用 `help cd` 查，见 [help](#cmd-help) |
| `man ls` 显示的是 `rust-ls` 的手册 | Ubuntu 26.04 的 ls 等基础命令由 Rust 版 coreutils 提供 | 这是正常的，参数说明以显示的手册为准 |

### 6. 一句话总结

查全部参数用 `man`，查常用参数用 `--help`，忘了命令名用 `man -k`。

[↑ 回到本类目录](#本类目录)

---

<a id="cmd-history"></a>

## 016 history：查看命令历史

> 级别：初级 · 来源：bash 内建命令

### 1. 作用

`history` 列出你在终端里执行过的命令，每条带一个编号。想不起昨天那条长命令是怎么写的，或者要重复执行某条命令，就用它。

历史记录保存在 `~/.bash_history`。Ubuntu 默认的 `~/.bashrc` 里设置了：

| 设置 | 值 | 含义 |
|---|---|---|
| `HISTSIZE` | 1000 | 当前终端里最多记住 1000 条 |
| `HISTFILESIZE` | 2000 | 历史文件最多保存 2000 条 |
| `HISTCONTROL` | ignoreboth | 连续重复的命令只记一次；以空格开头的命令不记录 |

### 2. 基本格式

```bash
history [数量]
```

### 3. 常用参数和用法

| 写法 | 作用 |
|---|---|
| `history` | 列出全部历史 |
| `history 10` | 只看最近 10 条 |
| `history -c` | 清空当前终端的历史 |
| `!编号` | 重新执行指定编号的命令，如 `!42` |
| `!!` | 重新执行上一条命令 |
| `!字符串` | 重新执行最近一条以该字符串开头的命令，如 `!ssh` |
| `Ctrl+R` | 交互式搜索历史：按下后输入关键词，再按 `Ctrl+R` 找更早的一条，回车执行 |

### 4. 常用示例

#### 4.1 查看历史

```
$ history
    1  ls -l
    2  cd logs
    3  grep ERROR app.log

$ history 2
    2  cd logs
    3  grep ERROR app.log
```

#### 4.2 配合 grep 找命令

```bash
history | grep ssh
```

找出所有执行过的 ssh 命令，再用 `!编号` 重新执行。

#### 4.3 `Ctrl+R`：最快的找法

按 `Ctrl+R`，提示符变成 `(reverse-i-search)`。输入 `docker`，就会显示最近一条含 docker 的命令。按回车执行，按 `→` 把它放到命令行上修改后再执行。

> ⚠️ 4.3 未实机验证：交互式按键需要真实终端，测试环境无法录制。

#### 4.4 不想被记录的命令

```bash
 export DB_PASS=secret123
```

命令前面加一个空格，因为 `HISTCONTROL=ignoreboth`，这条命令不会写进历史。输入密码之类的命令时这样做。

### 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 开了两个终端窗口，一个窗口的命令在另一个里查不到 | 每个终端在退出时才把历史写进文件 | 想立即同步，在终端里执行 `history -a`（写入）和 `history -n`（读入） |
| 历史里留下了明文密码 | 命令行参数里带了密码 | 用 `history -d 编号` 删掉那一条；以后在命令前加空格 |
| `!` 开头的字符串在双引号里被当成历史命令展开，报 `event not found` | bash 的历史展开功能 | 包含 `!` 的文本用单引号括起来 |

### 6. 一句话总结

找旧命令用 `Ctrl+R`，重复上一条用 `!!`，敏感命令前面加空格。

[↑ 回到本类目录](#本类目录)

---

<a id="cmd-exit"></a>

## 017 exit：退出 Shell 或脚本

> 级别：初级 · 来源：bash 内建命令

### 1. 作用

`exit` 结束当前的 Shell。具体效果看你在哪里执行：

| 场景 | 效果 |
|---|---|
| 本地终端窗口 | 关闭这个终端会话 |
| ssh 登录的远程服务器 | 断开连接，回到本机 |
| `sudo -i` 或 `su` 切换出来的 root | 退回到原来的普通用户 |
| 脚本里 | 结束脚本，并返回一个退出码 |

快捷键 `Ctrl+D` 在空的命令行上按下，效果和 exit 相同。

### 2. 基本格式

```bash
exit [退出码]
```

退出码是 0 到 255 的整数。0 表示成功，非 0 表示失败。不写时，使用最后一条命令的退出码。

### 3. 常用示例

#### 3.1 脚本里返回失败

```
$ bash -c "exit 3"; echo $?
3
```

脚本里写法：

```bash
#!/bin/bash
if [ ! -f /etc/app.conf ]; then
    echo "配置文件不存在"
    exit 1        # 返回 1，调用方（cron、CI）就知道这一步失败了
fi
```

#### 3.2 不写退出码

```
$ bash -c "ls nothere; exit"; echo $?
ls: cannot access 'nothere': No such file or directory
2

$ bash -c "exit"; echo $?
0
```

第一个例子，exit 前最后一条命令 ls 失败了（退出码 2），所以脚本也返回 2。

### 4. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 用 `source 脚本` 执行，脚本里的 exit 把整个终端关了 | source 让脚本在当前 Shell 里运行，exit 退出的就是当前 Shell | 被 source 的脚本里用 `return` 代替 exit |
| 在 `sudo -i` 里执行 exit，以为断开了服务器，其实只是退回了普通用户 | 每层 Shell 要各自 exit | 看提示符：`#` 是 root，`$` 是普通用户；主机名变回本机才是真正断开 |

### 5. 一句话总结

`exit` 退出当前一层 Shell；脚本里用 `exit 1` 告诉调用方失败了。

[↑ 回到本类目录](#本类目录)

---

<a id="cmd-which"></a>

## 044 which：查命令对应的程序位置

> 级别：初级 · 软件包：`debianutils`

### 1. 作用

`which` 在 PATH 环境变量列出的目录里查找某个命令，告诉你执行这个命令时，实际运行的是哪个文件。装了多个版本的 Python、Java、Node 时，用它确认当前用的是哪一个。

### 2. 基本格式

```bash
which [参数] 命令名...
```

### 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-a` | 列出所有匹配的位置，而不只是第一个 |

### 4. 常用示例

#### 4.1 查一个或多个命令

```
$ which ls
/usr/bin/ls

$ which python3 cp
/usr/bin/python3
/usr/bin/cp
```

#### 4.2 `-a`：同名命令有几个

```
$ which -a ls
/usr/bin/ls
/bin/ls
```

在 Ubuntu 上 `/bin` 是指向 `/usr/bin` 的链接，所以这两个其实是同一个文件。如果你在 `/usr/local/bin` 装了另一个版本，它会排在前面，优先被执行。

#### 4.3 找不到时

```
$ which nothere; echo $?
1
```

什么都不输出，退出码为 1。脚本里可以这样判断某个工具是否已安装：`which docker >/dev/null || echo "请先安装 docker"`。

### 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `which cd` 什么都不输出 | cd 是 bash 内建命令，不是磁盘上的文件 | 用 `type cd`，见 [type](#cmd-type) |
| `which ls` 显示 `/usr/bin/ls`，但 ls 的输出带颜色，和直接运行 `/usr/bin/ls` 不一样 | ls 被设成了别名 `ls --color=auto`，which 不看别名 | 用 `type ls` 查看命令最终会怎么执行 |
| 刚装好的程序，which 找不到 | 程序装在了 PATH 以外的目录，或者当前终端的 PATH 还没刷新 | 用 `echo $PATH` 检查；新开一个终端，或执行 `hash -r` |

```
$ which cd

$ type cd
cd is a shell builtin
```

### 6. 一句话总结

`which 命令` 查程序路径，多版本冲突时加 `-a`，内建命令和别名用 `type` 查。

[↑ 回到本类目录](#本类目录)

---

<a id="cmd-whereis"></a>

## 045 whereis：查程序、手册和源码的位置

> 级别：初级 · 软件包：`util-linux`

### 1. 作用

`whereis` 在系统的标准目录里查找一个命令的程序文件、手册文件和源码文件。和 which 相比，它不依赖 PATH，而是直接到 `/usr/bin`、`/usr/sbin`、`/usr/share/man` 这些固定位置去找，所以能找到不在 PATH 里的程序。

### 2. 基本格式

```bash
whereis [参数] 名称...
```

### 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-b` | 只查程序文件 |
| `-m` | 只查手册文件 |
| `-s` | 只查源码文件 |

### 4. 常用示例

```
$ whereis ls
ls: /usr/bin/ls

$ whereis bash
bash: /usr/bin/bash /usr/share/man/man1/bash.1.gz

$ whereis -b nginx
nginx:
```

bash 的结果里有两项：程序 `/usr/bin/bash` 和手册 `bash.1.gz`。nginx 后面是空的，说明系统里没有装。

### 5. 和 which 的区别

| | which | whereis |
|---|---|---|
| 查找范围 | PATH 里的目录 | 系统固定的标准目录 |
| 能找到手册 | 不能 | 能 |
| 回答的问题 | “我执行这个命令时，运行的是哪个文件” | “这个东西装在系统哪里” |

日常确认“当前用的是哪个版本”用 which；确认“系统里装没装、装在哪”用 whereis。

### 6. 一句话总结

`whereis 名称` 查程序和手册装在哪，后面为空就是没装。

[↑ 回到本类目录](#本类目录)

---

<a id="cmd-alias"></a>

## 060 alias：给命令起别名

> 级别：初级 · 来源：bash 内建命令

### 1. 作用

`alias` 给一条较长的命令起一个短名字，以后输入短名字就等于输入那条长命令。经常要敲的命令，比如 `git status`、`docker ps`、登录某台服务器的 ssh 命令，都可以设成别名。

### 2. 基本格式

```bash
alias                  # 列出所有别名
alias 名字='命令'      # 定义别名，等号两边不能有空格
unalias 名字           # 删除别名
```

### 3. 常用示例

#### 3.1 查看 Ubuntu 自带的别名

```
$ alias
alias alert='notify-send --urgency=low -i "$([ $? = 0 ] && echo terminal || echo error)" "$(history|tail -n1|sed -e '\''s/^\s*[0-9]\+\s*//;s/[;&|]\s*alert$//'\'')"'
alias egrep='egrep --color=auto'
alias fgrep='fgrep --color=auto'
alias grep='grep --color=auto'
alias l='ls -CF'
alias la='ls -A'
alias ll='ls -alF'
alias ls='ls --color=auto'

$ alias ll
alias ll='ls -alF'
```

#### 3.2 定义和删除

```
$ alias rm='rm -i'

$ type rm
rm is aliased to `rm -i'

$ unalias rm

$ type rm
rm is /usr/bin/rm
```

#### 3.3 永久保存

直接在终端里定义的别名，关掉终端就没了。要永久生效，写进 `~/.bashrc` 末尾：

```bash
echo "alias gs='git status'" >> ~/.bashrc
source ~/.bashrc          # 让当前终端立即生效
```

Ubuntu 的 `~/.bashrc` 还会自动读取 `~/.bash_aliases`（如果存在），别名多的话可以单独放在那个文件里。

#### 3.4 临时绕过别名

```bash
\ls
```

命令前加反斜杠，就会执行原始命令，不使用别名。

### 4. 实用别名示例

```bash
alias ..='cd ..'
alias ports='ss -tulnp'
alias myip='hostname -I'
alias dps='docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"'
alias db01='ssh tom@192.168.1.10'
```

### 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `alias gs = 'git status'` 报错 `alias: =: not found` | 等号两边有空格 | 写成 `alias gs='git status'` |
| 别名在脚本里不起作用 | 非交互式的 Shell（脚本）默认不展开别名 | 脚本里写完整命令，或者改写成函数 |
| 别名里用了 `$1`，参数没有按预期替换 | 别名只做文字替换，不支持参数 | 改用函数，例如 `mkcd() { mkdir -p "$1" && cd "$1"; }` |
| 新开的终端里别名没有了 | 只在当前终端里定义过 | 写进 `~/.bashrc` |

### 6. 一句话总结

常敲的长命令设成别名，写进 `~/.bashrc` 永久生效，需要参数时改用函数。

[↑ 回到本类目录](#本类目录)

---

<a id="cmd-type"></a>

## 061 type：查看命令的真实身份

> 级别：初级 · 来源：bash 内建命令

### 1. 作用

`type` 告诉你：输入一个命令名时，bash 实际会执行什么。它可能是别名（alias）、bash 内建命令（builtin）、函数，也可能是磁盘上的某个程序文件。

[which](#cmd-which) 只会去 PATH 里找文件，看不到别名和内建命令；type 能把这几种情况都区分出来，所以排查“这个命令为什么和我想的不一样”时，type 更准确。

### 2. 基本格式

```bash
type [参数] 命令名...
```

### 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-a` | 列出所有同名的定义（别名、内建、文件），按执行优先级排列 |
| `-t` | 只输出类型：`alias`、`builtin`、`file`、`function`、`keyword` |

### 4. 常用示例

#### 4.1 四种不同的结果

```
$ type ls
ls is aliased to `ls --color=auto'

$ type cd
cd is a shell builtin

$ type rm
rm is /usr/bin/rm

$ type nothere
bash: type: nothere: not found
```

#### 4.2 `-a`：所有同名定义

```
$ type -a ls
ls is aliased to `ls --color=auto'
ls is /usr/bin/ls
ls is /bin/ls
```

排在最前面的会被执行。

#### 4.3 `-t`：只要类型

```
$ type -t ls cd grep echo nothere
alias
builtin
alias
builtin
```

找不到的命令不输出任何内容。

### 5. 为什么要区分

| 类型 | 特点 |
|---|---|
| 别名 | 只在交互终端里有效，脚本里不生效 |
| 内建命令 | 没有独立的手册，要用 `help 命令` 查用法 |
| 文件 | 用 `man 命令` 查手册 |

### 6. 一句话总结

`type 命令` 看它是别名、内建还是文件，比 which 更准确。

[↑ 回到本类目录](#本类目录)

---

<a id="cmd-help"></a>

## 062 help：查看 bash 内建命令的帮助

> 级别：初级 · 来源：bash 内建命令

### 1. 作用

`help` 显示 bash 内建命令（cd、echo、export、history、alias 等）的用法。这些命令是 bash 自己的一部分，不是独立的程序，`man cd` 查不到它们，要用 help。

### 2. 基本格式

```bash
help [参数] [内建命令名]
```

不带参数时，列出所有内建命令。

### 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-d` | 只显示一句话简介 |
| `-s` | 只显示用法格式 |

### 4. 常用示例

#### 4.1 列出所有内建命令

```
$ help
GNU bash, version 5.3.9(1)-release (x86_64-pc-linux-gnu)
These shell commands are defined internally.  Type `help' to see this list.
Type `help name' to find out more about the function `name'.
Use `info bash' to find out more about the shell in general.
Use `man -k' or `info' to find out more about commands not in this list.
...
```

Ubuntu 26.04 自带的 bash 是 5.3 版。

#### 4.2 查某个内建命令

```
$ help cd
cd: cd [-L|[-P [-e]]] [-@] [dir]
    Change the shell working directory.

    Change the current directory to DIR.  The default DIR is the value of the
    HOME shell variable. If DIR is "-", it is converted to $OLDPWD.
...
```

#### 4.3 `-d`：一句话简介

```
$ help -d cd pwd echo
cd - Change the shell working directory.
pwd - Print the name of the current working directory.
echo - Write arguments to the standard output.
```

#### 4.4 不是内建命令时

```
$ help ls
bash: help: no help topics match `ls'.  Try `help help' or `man -k ls' or `info ls'.
```

ls 是独立程序，用 `man ls` 或 `ls --help`。

### 5. 查帮助用哪个

| 命令类型（用 `type` 判断） | 查帮助的方法 |
|---|---|
| 内建命令（`is a shell builtin`） | `help 命令` |
| 程序文件（`is /usr/bin/...`） | `man 命令` 或 `命令 --help` |

### 6. 一句话总结

内建命令查 `help`，程序查 `man`，分不清先 `type`。

[↑ 回到本类目录](#本类目录)

---

<a id="cmd-env"></a>

## 075 env：查看环境变量，或带着变量运行命令

> 级别：初级 · 软件包：`rust-coreutils`（系统自带）

### 1. 作用

环境变量（environment variable）是一组“名字=值”的设置，每个程序启动时都会从启动它的 Shell 那里继承一份。程序靠它们知道家目录在哪（HOME）、去哪里找命令（PATH）、用什么语言显示（LANG）。

`env` 有两个用途：

- 不带参数时，列出当前所有的环境变量；
- `env 变量=值 命令`：只为这一条命令临时设置变量，不影响当前 Shell。

### 2. 基本格式

```bash
env                          # 列出全部
env 变量=值 [变量=值...] 命令  # 临时设置变量后运行命令
```

### 3. 常用参数和相关命令

| 写法 | 作用 |
|---|---|
| `env` | 列出全部环境变量 |
| `env \| grep 关键词` | 查找某几个变量 |
| `printenv 变量名` | 只显示一个变量的值 |
| `env -u 变量 命令` | 去掉某个变量后运行命令 |
| `env -i 命令` | 在完全空白的环境里运行命令 |

### 4. 常用示例

#### 4.1 查看常用变量

```
$ env | grep -E "^(HOME|USER|SHELL|LANG|PATH)="
HOME=/home/tom
LANG=C.UTF-8
USER=tom
PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin

$ printenv HOME
/home/tom
```

| 变量 | 含义 |
|---|---|
| HOME | 家目录 |
| USER | 当前用户名 |
| PATH | 执行命令时，按顺序去这些目录里找程序，用冒号分隔 |
| LANG | 语言和字符编码 |
| SHELL | 登录 Shell |

> 测试环境里没有 SHELL 变量，所以上面的结果少了一行。普通登录会话里会有 `SHELL=/bin/bash`。

#### 4.2 临时设置变量运行命令

```
$ env LANG=C date
Thu Oct 29 07:03:58 UTC 2026
```

系统是中文环境时，报错信息是中文的，在网上不好搜。用 `env LANG=C 命令` 临时切成英文，拿英文报错去搜索。

bash 里也可以省略 env，直接写 `LANG=C date`，效果相同。

### 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `env` 输出一屏看不完，其中有一个超长的 `LS_COLORS` | 这是 ls 的配色设置，内容很长 | 用 `env \| grep 关键词` 或 `printenv 变量名` |
| 在终端里能运行的程序，放进 cron 定时任务就报找不到命令 | cron 运行时的环境变量很少，PATH 很短 | 脚本里写命令的完整路径，或在脚本开头设置 PATH |

### 6. 一句话总结

`env | grep` 查环境变量，`env 变量=值 命令` 临时改变量运行一次。

[↑ 回到本类目录](#本类目录)

---

<a id="cmd-export"></a>

## 076 export：设置环境变量

> 级别：初级 · 来源：bash 内建命令

### 1. 作用

在 bash 里写 `变量=值`，只是定义了一个 **Shell 变量**，只有当前 Shell 自己能用。`export` 把它变成**环境变量**，之后从这个 Shell 启动的所有程序、脚本都能读到它。

常见用途：设置 `JAVA_HOME`、把自己的程序目录加进 `PATH`、给程序传配置（如 `APP_ENV=prod`）。

### 2. 基本格式

```bash
export 变量名=值
export 已有的变量名
```

等号两边不能有空格。

### 3. 常用参数

| 写法 | 作用 |
|---|---|
| `export 变量=值` | 定义并导出 |
| `export -p` | 列出所有已导出的变量 |
| `export -n 变量` | 取消导出（变量还在，只是子进程看不到了） |
| `unset 变量` | 删除变量 |

### 4. 常用示例

#### 4.1 有没有 export 的区别

```
$ APP_ENV=test; bash -c 'echo "子进程看到: [$APP_ENV]"'
子进程看到: []

$ export APP_ENV=test; bash -c 'echo "子进程看到: [$APP_ENV]"'
子进程看到: [test]
```

`bash -c` 启动了一个子进程。没有 export 时，子进程看不到 APP_ENV。

#### 4.2 把目录加进 PATH

```
$ export PATH="$HOME/bin:$PATH"; echo $PATH
/home/tom/bin:/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin
```

`$HOME/bin` 放在最前面，同名命令会优先使用这里的版本。一定要保留后面的 `:$PATH`。

#### 4.3 永久生效

在终端里 export 的变量，关掉终端就没了。要永久生效，写进 `~/.bashrc`：

```bash
echo 'export JAVA_HOME=/usr/lib/jvm/java-21-openjdk-amd64' >> ~/.bashrc
echo 'export PATH="$JAVA_HOME/bin:$PATH"' >> ~/.bashrc
source ~/.bashrc
```

用单引号，是为了让 `$JAVA_HOME`、`$PATH` 原样写进文件，等每次登录时再展开。

| 文件 | 作用范围 |
|---|---|
| `~/.bashrc` | 只对自己，每开一个终端都会读取 |
| `/etc/environment` | 对所有用户，格式是 `变量=值`，不能写 export，也不展开 `$变量` |

> ⚠️ 4.3 未实机验证：JAVA_HOME 的路径以你实际安装的 JDK 为准，用 `ls /usr/lib/jvm` 确认。

### 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `export PATH=/opt/app/bin` 之后，ls、cd 以外的命令全都找不到了 | 把 PATH 整个覆盖了，系统目录丢了 | 写成 `export PATH="/opt/app/bin:$PATH"`；改坏了就关掉终端重开 |
| `export A = 1` 报 ``bash: export: `=': not a valid identifier`` | 等号两边有空格 | 写成 `export A=1` |
| 写进 .bashrc 了，ssh 执行远程命令时还是读不到 | 非交互式的 Shell 可能不读取 .bashrc | 程序的配置用配置文件或 systemd 的 `Environment=`，不要依赖 .bashrc |
| `sudo 命令` 读不到自己 export 的变量 | sudo 默认会重置环境变量 | 写成 `sudo 变量=值 命令`；见 [sudo](05-用户和权限.md#cmd-sudo) |

### 6. 一句话总结

子程序要用的变量要 `export`；改 PATH 一定带上 `:$PATH`；永久生效写进 `~/.bashrc`。

[↑ 回到本类目录](#本类目录)

---

<a id="cmd-source"></a>

## 077 source：在当前 Shell 里执行脚本

> 级别：初级 · 来源：bash 内建命令

### 1. 作用

`source` 读取一个脚本文件，在**当前 Shell** 里逐行执行。脚本里设置的变量、定义的别名和函数、`cd` 切换的目录，执行完都会留在当前 Shell 里。

普通的 `bash 脚本.sh` 或 `./脚本.sh` 是启动一个新的子 Shell 来执行，执行完子 Shell 就退出了，它做的设置也随之消失。

最常见的用法：改完 `~/.bashrc` 后执行 `source ~/.bashrc`，不用重开终端就能生效。

### 2. 基本格式

```bash
source 文件
. 文件        # 点号是 source 的简写，效果完全相同
```

### 3. 常用示例

#### 3.1 加载变量文件

演示文件 app.env 的内容：

```
APP_PORT=8080
export APP_NAME=price
```

```
$ bash app.env; echo "[$APP_PORT]"
[]

$ source app.env; echo "[$APP_PORT] [$APP_NAME]"
[8080] [price]
```

用 bash 执行，变量设置在子 Shell 里，当前 Shell 拿不到；用 source 执行，变量留在了当前 Shell。

#### 3.2 脚本里的 cd

go.sh 只有一行 `cd /tmp`：

```
$ bash go.sh; pwd
/home/tom/demo

$ source go.sh; pwd
/tmp
```

#### 3.3 让 .bashrc 的修改立即生效

```bash
source ~/.bashrc
```

#### 3.4 激活 Python 虚拟环境

```bash
source .venv/bin/activate
```

activate 脚本要修改当前 Shell 的 PATH 和提示符，所以必须用 source 执行。

### 4. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| source 一个脚本后，终端窗口直接关闭了 | 脚本里有 `exit`，source 时它退出的是当前 Shell | 给 source 用的脚本里用 `return`，不用 exit |
| 用 `./activate` 执行，虚拟环境没有激活 | 这样是在子 Shell 里执行的 | 用 `source` |
| source 一个不信任的脚本后，环境被改乱了 | 脚本里的任何命令都直接作用于当前 Shell | 只 source 自己了解内容的文件 |

### 5. 一句话总结

要让脚本里的变量、cd 留在当前终端，就用 `source 文件`（或 `. 文件`）。

[↑ 回到本类目录](#本类目录)

---

<a id="cmd-xdg-open"></a>

## 100 xdg-open：用默认程序打开文件或网址

> 级别：初级 · 软件包：`xdg-utils`（Ubuntu 26.04 为 1.2.1 版）

### 1. 作用

`xdg-open` 用系统默认的程序打开文件或网址：PDF 用文档查看器打开，图片用图片查看器，网址用默认浏览器，目录用文件管理器。效果和在桌面上双击文件一样。

它只在 **Ubuntu 桌面版**（有图形界面）上有用。服务器没有图形界面，xdg-open 打不开任何东西。

### 2. 基本格式

```bash
xdg-open 文件、目录或网址
```

一次只能打开一个。

### 3. 常用示例

```bash
xdg-open report.pdf            # 用文档查看器打开 PDF
xdg-open .                     # 在文件管理器里打开当前目录
xdg-open https://ubuntu.com    # 用默认浏览器打开网址
```

在终端里处理完一批文件，想用图形界面看一眼结果时，`xdg-open .` 最方便。

> ⚠️ 本节未实机验证：测试环境没有图形界面。

### 4. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 在服务器或 ssh 会话里报 `xdg-open: no method available for opening 'readme.txt'` | 没有图形界面，找不到可以打开文件的程序 | 文本文件用 `less`；其他文件下载到本机再打开 |
| 打开的程序不是你想要的 | 默认程序设置不同 | 在桌面的“设置 → 应用 → 默认应用”里修改；或直接用程序名打开，如 `evince report.pdf` |

第一条的实测：

```
$ xdg-open readme.txt
/usr/bin/xdg-open: 1032: www-browser: not found
/usr/bin/xdg-open: 1032: links2: not found
/usr/bin/xdg-open: 1032: elinks: not found
/usr/bin/xdg-open: 1032: links: not found
/usr/bin/xdg-open: 1032: lynx: not found
/usr/bin/xdg-open: 1032: w3m: not found
xdg-open: no method available for opening 'readme.txt'
```

没有图形界面时，xdg-open 会依次尝试几个终端里的文字浏览器，都没装就报错。

### 5. 一句话总结

桌面版上 `xdg-open 文件` 相当于双击打开，服务器上用不了。

[↑ 回到本类目录](#本类目录)

---

[← 上一类：系统信息和硬件](11-系统信息和硬件.md) · [返回总目录](../../README.md)
