# 062 help：查看 bash 内建命令的帮助

> 级别：初级 · 来源：bash 内建命令

## 1. 作用

`help` 显示 bash 内建命令（cd、echo、export、history、alias 等）的用法。这些命令是 bash 自己的一部分，不是独立的程序，`man cd` 查不到它们，要用 help。

## 2. 基本格式

```bash
help [参数] [内建命令名]
```

不带参数时，列出所有内建命令。

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-d` | 只显示一句话简介 |
| `-s` | 只显示用法格式 |

## 4. 常用示例

### 4.1 列出所有内建命令

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

### 4.2 查某个内建命令

```
$ help cd
cd: cd [-L|[-P [-e]]] [-@] [dir]
    Change the shell working directory.

    Change the current directory to DIR.  The default DIR is the value of the
    HOME shell variable. If DIR is "-", it is converted to $OLDPWD.
...
```

### 4.3 `-d`：一句话简介

```
$ help -d cd pwd echo
cd - Change the shell working directory.
pwd - Print the name of the current working directory.
echo - Write arguments to the standard output.
```

### 4.4 不是内建命令时

```
$ help ls
bash: help: no help topics match `ls'.  Try `help help' or `man -k ls' or `info ls'.
```

ls 是独立程序，用 `man ls` 或 `ls --help`。

## 5. 查帮助用哪个

| 命令类型（用 `type` 判断） | 查帮助的方法 |
|---|---|
| 内建命令（`is a shell builtin`） | `help 命令` |
| 程序文件（`is /usr/bin/...`） | `man 命令` 或 `命令 --help` |

## 6. 一句话总结

内建命令查 `help`，程序查 `man`，分不清先 `type`。
