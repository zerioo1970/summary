# 014 echo：输出一行文本

> 级别：初级 · 来源：bash 内建命令

## 1. 作用

`echo` 把后面的文字原样输出到屏幕。它最常见的三种用途：

- 查看变量的值，比如 `echo $PATH`；
- 在脚本里打印提示信息；
- 配合 `>`、`>>` 把一行内容写进文件。

## 2. 基本格式

```bash
echo [参数] [文本...]
```

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-n` | 输出后不换行 |
| `-e` | 解释转义字符：`\n` 换行，`\t` 制表符 |
| `-E` | 不解释转义字符（默认行为） |

## 4. 常用示例

### 4.1 输出文本和变量

```
$ echo hello
hello

$ echo $SHELL $USER
/bin/bash tom
```

### 4.2 双引号和单引号的区别

```
$ echo "价格：$HOME"
价格：/home/tom

$ echo '价格：$HOME'
价格：$HOME
```

双引号里的 `$变量` 会被替换成值；单引号里的内容完全原样输出。要输出 `$` 本身，用单引号。

### 4.3 `-e`：换行和制表符

```
$ echo -e "第一行\n第二行\t制表"
第一行
第二行	制表

$ echo "第一行\n第二行"
第一行\n第二行
```

不加 `-e`，`\n` 会原样输出。

### 4.4 `-n`：不换行

```
$ echo -n "no newline"; echo "|"
no newline|
```

### 4.5 写入文件

```
$ echo "log line" > out.txt; cat out.txt
log line

$ echo "line2" >> out.txt; cat out.txt
log line
line2
```

`>` 覆盖，`>>` 追加。往配置文件末尾加一行时，一定要用 `>>`。

### 4.6 `echo $?`：看上一条命令是否成功

```
$ ls nothere; echo $?
ls: cannot access 'nothere': No such file or directory
2
```

`$?` 是上一条命令的退出码：0 表示成功，非 0 表示失败。写脚本判断某一步是否成功，就靠它。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 想往配置文件追加一行，结果整个文件只剩这一行 | 把 `>>` 写成了 `>` | 追加用 `>>`；改重要文件前先备份 |
| `echo *.txt` 输出的是一串文件名，不是 `*.txt` | `*` 被 Shell 展开成匹配的文件名 | 要原样输出就加引号：`echo "*.txt"` |
| `echo "x" >> /etc/hosts` 加了 sudo 还是 `Permission denied` | 重定向是当前 Shell 做的，不受 sudo 影响 | 用 `echo "x" \| sudo tee -a /etc/hosts`，见 [sudo](004-sudo.md) |
| 需要精确控制格式（补零、对齐小数） | echo 不支持格式化 | 改用 `printf`，如 `printf "%05d\n" 42` |

第二条的实测：

```
$ echo *.txt
link.txt out.txt readme.txt

$ echo "*.txt"
*.txt
```

## 6. 一句话总结

看变量用 `echo $变量`，写文件分清 `>` 和 `>>`，`$?` 查上一步成没成功。

---

[← 上一个：clear](013-clear.md) · [返回目录](../../README.md) · [下一个：man →](015-man.md)
