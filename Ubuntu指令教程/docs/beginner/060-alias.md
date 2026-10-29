# 060 alias：给命令起别名

> 级别：初级 · 来源：bash 内建命令

## 1. 作用

`alias` 给一条较长的命令起一个短名字，以后输入短名字就等于输入那条长命令。经常要敲的命令，比如 `git status`、`docker ps`、登录某台服务器的 ssh 命令，都可以设成别名。

## 2. 基本格式

```bash
alias                  # 列出所有别名
alias 名字='命令'      # 定义别名，等号两边不能有空格
unalias 名字           # 删除别名
```

## 3. 常用示例

### 3.1 查看 Ubuntu 自带的别名

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

### 3.2 定义和删除

```
$ alias rm='rm -i'

$ type rm
rm is aliased to `rm -i'

$ unalias rm

$ type rm
rm is /usr/bin/rm
```

### 3.3 永久保存

直接在终端里定义的别名，关掉终端就没了。要永久生效，写进 `~/.bashrc` 末尾：

```bash
echo "alias gs='git status'" >> ~/.bashrc
source ~/.bashrc          # 让当前终端立即生效
```

Ubuntu 的 `~/.bashrc` 还会自动读取 `~/.bash_aliases`（如果存在），别名多的话可以单独放在那个文件里。

### 3.4 临时绕过别名

```bash
\ls
```

命令前加反斜杠，就会执行原始命令，不使用别名。

## 4. 实用别名示例

```bash
alias ..='cd ..'
alias ports='ss -tulnp'
alias myip='hostname -I'
alias dps='docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"'
alias db01='ssh tom@192.168.1.10'
```

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `alias gs = 'git status'` 报错 `alias: =: not found` | 等号两边有空格 | 写成 `alias gs='git status'` |
| 别名在脚本里不起作用 | 非交互式的 Shell（脚本）默认不展开别名 | 脚本里写完整命令，或者改写成函数 |
| 别名里用了 `$1`，参数没有按预期替换 | 别名只做文字替换，不支持参数 | 改用函数，例如 `mkcd() { mkdir -p "$1" && cd "$1"; }` |
| 新开的终端里别名没有了 | 只在当前终端里定义过 | 写进 `~/.bashrc` |

## 6. 一句话总结

常敲的长命令设成别名，写进 `~/.bashrc` 永久生效，需要参数时改用函数。
