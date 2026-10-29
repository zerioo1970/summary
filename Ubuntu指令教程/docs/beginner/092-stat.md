# 092 stat：查看文件的详细属性

> 级别：初级 · 软件包：`rust-coreutils`（系统自带）

## 1. 作用

`stat` 显示文件的全部元数据：精确大小、占用的块数、inode 号、权限（同时给出数字和字母两种写法）、所有者，以及四个时间。`ls -l` 只显示一个修改时间，排查“这个文件到底什么时候被动过”时要用 stat。

## 2. 基本格式

```bash
stat [参数] 文件...
```

## 3. 常用参数

| 参数 | 长格式 | 作用 |
|---|---|---|
| `-c 格式` | `--format` | 只输出指定的字段 |
| `-L` | `--dereference` | 对符号链接，显示它指向的文件 |

`-c` 常用的格式符：

| 格式符 | 含义 |
|---|---|
| `%n` | 文件名 |
| `%s` | 大小（字节） |
| `%U` / `%G` | 所有者 / 所属组 |
| `%a` | 权限的数字写法，如 644 |
| `%y` | 修改时间 |

## 4. 常用示例

### 4.1 完整信息

```
$ stat readme.txt
  File: readme.txt
  Size: 6               Blocks: 8          IO Block: 4096   regular file
Device: 0,118   Inode: 9           Links: 1
Access: (0644/-rw-r--r--)  Uid: ( 1000/     tom)   Gid: ( 1000/     tom)
Access: 2026-10-01 09:00:00.000000000 +0000
Modify: 2026-10-01 09:00:00.000000000 +0000
Change: 2026-10-29 07:08:33.509863663 +0000
 Birth: 2026-10-29 07:08:33.489863744 +0000
```

四个时间的含义：

| 时间 | 什么时候会变 |
|---|---|
| Access | 文件内容被读取 |
| Modify | 文件**内容**被修改，`ls -l` 显示的就是它 |
| Change | 文件**属性**被修改（权限、所有者、改名），内容修改也会更新它 |
| Birth | 文件创建的时间 |

演示文件的 Modify 被 touch 改成了 Oct 1，但 Change 记录了真实的操作时间（Oct 29）。touch 可以伪造 Modify，伪造不了 Change。

### 4.2 `-c`：只取需要的字段

```
$ stat -c "%n %s %U %a %y" readme.txt
readme.txt 6 tom 644 2026-10-01 09:00:00.000000000 +0000

$ stat -c %s big.iso
2500000
```

脚本里判断文件大小，`stat -c %s` 比解析 `ls -l` 的输出可靠。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| Access 时间一直不怎么变 | Ubuntu 默认用 relatime 方式挂载，读取时不是每次都更新 Access | 不要用 Access 时间判断文件是否被读过 |
| `stat: cannot statx 'nothere': No such file or directory` | 文件不存在 | 检查文件名 |

## 6. 一句话总结

`stat` 看四个时间和数字权限，脚本里取大小用 `stat -c %s`。

---

[← 上一个：dirname](091-dirname.md) · [返回目录](../../README.md) · [下一个：locate →](093-locate.md)
