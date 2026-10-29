# 061 type：查看命令的真实身份

> 级别：初级 · 来源：bash 内建命令

## 1. 作用

`type` 告诉你：输入一个命令名时，bash 实际会执行什么。它可能是别名（alias）、bash 内建命令（builtin）、函数，也可能是磁盘上的某个程序文件。

[which](044-which.md) 只会去 PATH 里找文件，看不到别名和内建命令；type 能把这几种情况都区分出来，所以排查“这个命令为什么和我想的不一样”时，type 更准确。

## 2. 基本格式

```bash
type [参数] 命令名...
```

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-a` | 列出所有同名的定义（别名、内建、文件），按执行优先级排列 |
| `-t` | 只输出类型：`alias`、`builtin`、`file`、`function`、`keyword` |

## 4. 常用示例

### 4.1 四种不同的结果

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

### 4.2 `-a`：所有同名定义

```
$ type -a ls
ls is aliased to `ls --color=auto'
ls is /usr/bin/ls
ls is /bin/ls
```

排在最前面的会被执行。

### 4.3 `-t`：只要类型

```
$ type -t ls cd grep echo nothere
alias
builtin
alias
builtin
```

找不到的命令不输出任何内容。

## 5. 为什么要区分

| 类型 | 特点 |
|---|---|
| 别名 | 只在交互终端里有效，脚本里不生效 |
| 内建命令 | 没有独立的手册，要用 `help 命令` 查用法 |
| 文件 | 用 `man 命令` 查手册 |

## 6. 一句话总结

`type 命令` 看它是别名、内建还是文件，比 which 更准确。

---

[← 上一个：alias](060-alias.md) · [返回目录](../../README.md) · [下一个：help →](062-help.md)
