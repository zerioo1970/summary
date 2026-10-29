# 010 touch：新建空文件或修改文件时间

> 级别：初级 · 软件包：`rust-coreutils`（系统自带）

## 1. 作用

`touch` 有两个用途：

- 文件不存在时，新建一个空文件；
- 文件已存在时，把它的修改时间改成现在（或你指定的时间），内容不变。

第二种用法常用于测试：比如让某个按时间清理旧文件的脚本，误以为一个文件是很久以前的。

## 2. 基本格式

```bash
touch [参数] 文件...
```

## 3. 常用参数

| 参数 | 长格式 | 作用 |
|---|---|---|
| `-d` | `--date` | 指定时间，写法如 `"2026-01-01 08:00"` |
| `-t` | | 指定时间，格式为 `[[CC]YY]MMDDhhmm[.ss]`，如 `202605011230` |
| `-c` | `--no-create` | 文件不存在时不新建 |
| `-r` | `--reference` | 使用另一个文件的时间 |

## 4. 常用示例

### 4.1 新建空文件

```
$ touch new.txt

$ ls -l new.txt readme.txt
-rw-r--r-- 1 tom tom 0 Oct 29 06:41 new.txt
-rw-r--r-- 1 tom tom 6 Oct  1 09:00 readme.txt
```

### 4.2 把修改时间改成现在

```
$ touch readme.txt

$ ls -l readme.txt
-rw-r--r-- 1 tom tom 6 Oct 29 06:41 readme.txt
```

时间从 Oct 1 变成了现在，内容没变（大小还是 6）。

### 4.3 `-d` 和 `-t`：改成指定时间

```
$ touch -d "2026-01-01 08:00" readme.txt

$ ls -l readme.txt
-rw-r--r-- 1 tom tom 6 Jan  1  2026 readme.txt

$ touch -t 202605011230 readme.txt

$ ls -l --time-style=long-iso readme.txt
-rw-r--r-- 1 tom tom 6 2026-05-01 12:30 readme.txt
```

`-d` 的写法更直观，推荐用 `-d`。

### 4.4 批量新建

```
$ touch file{1..3}.log

$ ls
big.iso    file2.log  link.txt  new.txt     run.sh
file1.log  file3.log  logs      readme.txt  src
```

### 4.5 `-r`：照搬另一个文件的时间

```
$ touch -r big.iso new.txt

$ ls -l big.iso new.txt
-rw-r--r-- 1 tom tom 2500000 Oct 20 14:30 big.iso
-rw-r--r-- 1 tom tom       0 Oct 20 14:30 new.txt
```

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 想测试“文件不存在”的情况，touch 反而把文件建出来了 | touch 默认会新建 | 加 `-c` |
| `touch: cannot touch '/etc/x': Permission denied` | 没有目录的写权限 | 加 `sudo` |
| 用 touch 清空文件，文件内容还在 | touch 只改时间，不动内容 | 清空文件用 `: > 文件名` 或 `truncate -s 0 文件名` |

## 6. 一句话总结

`touch` 建空文件或刷新时间，指定时间用 `-d`，不想新建加 `-c`。

---

[← 上一个：mkdir](009-mkdir.md) · [返回目录](../../README.md) · [下一个：nano →](011-nano.md)
