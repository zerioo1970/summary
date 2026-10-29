# 095 lscpu：查看 CPU 信息

> 级别：初级 · 软件包：`util-linux`

## 1. 作用

`lscpu` 显示 CPU 的型号、核心数、线程数、架构、缓存大小等信息。评估服务器配置、给数据库或 Java 程序设置线程数之前，先用它看清楚有几个核。

## 2. 基本格式

```bash
lscpu [参数]
```

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-e` | 每个逻辑 CPU 一行，以表格显示 |
| `-J` | 输出 JSON |

## 4. 常用示例

> 以下输出来自测试环境（云服务器）。

```
$ lscpu
Architecture:                            x86_64
CPU op-mode(s):                          32-bit, 64-bit
Address sizes:                           46 bits physical, 48 bits virtual
Byte Order:                              Little Endian
CPU(s):                                  8
On-line CPU(s) list:                     0-7
Vendor ID:                               GenuineIntel
Model name:                              Intel(R) Xeon(R) Platinum 8488C
CPU family:                              6
Model:                                   143
Thread(s) per core:                      2
Core(s) per socket:                      4
Socket(s):                               1
...
Hypervisor vendor:                       KVM
Virtualization type:                     full
L1d cache:                               192 KiB (4 instances)
...
```

最常看的几项：

```
$ lscpu | grep -E "^(Model name|CPU\(s\)|Thread|Core|Socket)"
CPU(s):                                  8
Model name:                              Intel(R) Xeon(R) Platinum 8488C
Thread(s) per core:                      2
Core(s) per socket:                      4
Socket(s):                               1
```

| 项 | 含义 |
|---|---|
| Socket(s) | 物理 CPU 的个数 |
| Core(s) per socket | 每个 CPU 的物理核心数 |
| Thread(s) per core | 每个核心的线程数，2 表示开启了超线程 |
| CPU(s) | 逻辑 CPU 总数 = 1 × 4 × 2 = 8，`top`、`nproc` 看到的就是这个数 |
| Hypervisor vendor | 出现这一行说明是虚拟机 |

## 5. 一句话总结

`lscpu` 看 CPU 型号和核数，逻辑 CPU 数 = 插槽 × 核心 × 线程。

---

[← 上一个：updatedb](094-updatedb.md) · [返回目录](../../README.md) · [下一个：lsusb →](096-lsusb.md)
