# 027 free：查看内存使用情况

> 级别：初级 · 软件包：`procps`

## 1. 作用

`free` 显示物理内存和交换空间（swap，内存不够时临时借用的一块磁盘空间）的使用情况。服务器变慢、程序被系统杀掉时，先用 free 看内存是否吃紧。

## 2. 基本格式

```bash
free [参数]
```

## 3. 常用参数

| 参数 | 长格式 | 作用 |
|---|---|---|
| `-h` | `--human` | 自动选单位（Mi、Gi） |
| `-m` | `--mebi` | 以 MB 为单位 |
| `-g` | `--gibi` | 以 GB 为单位 |
| `-s 秒` | `--seconds` | 每隔 N 秒刷新一次 |
| `-c 次数` | `--count` | 和 `-s` 配合，刷新 N 次后退出 |

## 4. 常用示例

> 以下输出来自测试环境（约 32 GB 内存），数值和你的机器不同。

### 4.1 `free -h`

```
$ free -h
               total        used        free      shared  buff/cache   available
Mem:            30Gi       2.2Gi        20Gi        49Mi       8.1Gi        28Gi
Swap:          2.0Gi          0B       2.0Gi
```

| 列 | 含义 |
|---|---|
| total | 总内存 |
| used | 程序正在使用的内存 |
| free | 完全没被使用的内存 |
| buff/cache | 系统拿来做磁盘缓存的内存，程序需要时可以随时让出来 |
| available | **程序还能用多少**：大约等于 free 加上可回收的 buff/cache |

判断内存够不够，看 **available**，不看 free。

### 4.2 `-m`：精确到 MB

```
$ free -m
               total        used        free      shared  buff/cache   available
Mem:           31560        2288       21432          49        8338       29271
Swap:           2047           0        2047
```

### 4.3 `-s`：连续观察

```
$ free -h -s 2 -c 2
               total        used        free      shared  buff/cache   available
Mem:            30Gi       2.2Gi        20Gi        49Mi       8.1Gi        28Gi
Swap:          2.0Gi          0B       2.0Gi

               total        used        free      shared  buff/cache   available
Mem:            30Gi       2.2Gi        20Gi        49Mi       8.1Gi        28Gi
Swap:          2.0Gi          0B       2.0Gi
```

每 2 秒输出一次，共 2 次。压测或跑批处理时，可以用 `free -h -s 5` 盯着内存变化，按 `Ctrl+C` 结束。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| free 列很小，以为内存快用完了 | Linux 会把空闲内存拿去做磁盘缓存（buff/cache），这是正常的 | 看 available 列 |
| Swap 的 used 一直在增长，系统明显变卡 | 物理内存不够，系统频繁在内存和磁盘之间交换数据 | 用 `top` 按内存排序找出占内存最多的进程；长期不够就加内存 |
| 程序突然消失，日志里没有报错 | 内存耗尽时内核的 OOM Killer 会杀掉占内存最多的进程 | `sudo dmesg -T \| grep -i "out of memory"` 确认 |

## 6. 一句话总结

`free -h` 看内存，判断够不够看 available 列。

---

[← 上一个：du](026-du.md) · [返回目录](../../README.md) · [下一个：top →](028-top.md)
