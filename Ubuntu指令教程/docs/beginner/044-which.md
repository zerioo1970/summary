# 044 which：查命令对应的程序位置

> 级别：初级 · 软件包：`debianutils`

## 1. 作用

`which` 在 PATH 环境变量列出的目录里查找某个命令，告诉你执行这个命令时，实际运行的是哪个文件。装了多个版本的 Python、Java、Node 时，用它确认当前用的是哪一个。

## 2. 基本格式

```bash
which [参数] 命令名...
```

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-a` | 列出所有匹配的位置，而不只是第一个 |

## 4. 常用示例

### 4.1 查一个或多个命令

```
$ which ls
/usr/bin/ls

$ which python3 cp
/usr/bin/python3
/usr/bin/cp
```

### 4.2 `-a`：同名命令有几个

```
$ which -a ls
/usr/bin/ls
/bin/ls
```

在 Ubuntu 上 `/bin` 是指向 `/usr/bin` 的链接，所以这两个其实是同一个文件。如果你在 `/usr/local/bin` 装了另一个版本，它会排在前面，优先被执行。

### 4.3 找不到时

```
$ which nothere; echo $?
1
```

什么都不输出，退出码为 1。脚本里可以这样判断某个工具是否已安装：`which docker >/dev/null || echo "请先安装 docker"`。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `which cd` 什么都不输出 | cd 是 bash 内建命令，不是磁盘上的文件 | 用 `type cd`，见 [type](061-type.md) |
| `which ls` 显示 `/usr/bin/ls`，但 ls 的输出带颜色，和直接运行 `/usr/bin/ls` 不一样 | ls 被设成了别名 `ls --color=auto`，which 不看别名 | 用 `type ls` 查看命令最终会怎么执行 |
| 刚装好的程序，which 找不到 | 程序装在了 PATH 以外的目录，或者当前终端的 PATH 还没刷新 | 用 `echo $PATH` 检查；新开一个终端，或执行 `hash -r` |

```
$ which cd

$ type cd
cd is a shell builtin
```

## 6. 一句话总结

`which 命令` 查程序路径，多版本冲突时加 `-a`，内建命令和别名用 `type` 查。

---

[← 上一个：cal](043-cal.md) · [返回目录](../../README.md) · [下一个：whereis →](045-whereis.md)
