# 041 uname：查看内核和系统信息

> 级别：初级 · 软件包：`rust-coreutils`（系统自带）

## 1. 作用

`uname` 显示内核名称、内核版本、主机名、CPU 架构等信息。装驱动、下载软件安装包（区分 x86_64 还是 arm64）、排查内核相关问题时要用到。

要看 Ubuntu 的发行版版本号（26.04），用的是 [lsb_release](057-lsb_release.md)，不是 uname。

## 2. 基本格式

```bash
uname [参数]
```

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-a` | 显示全部信息 |
| `-r` | 内核版本 |
| `-m` | 硬件架构：`x86_64`（常见的 Intel/AMD）或 `aarch64`（ARM） |
| `-n` | 主机名 |
| `-s` | 内核名称（Linux） |

## 4. 常用示例

> 测试环境借用了宿主机的内核，所以版本号带 amzn2023。在真实的 Ubuntu 26.04 上，内核版本形如 `x.y.z-NN-generic`。

### 4.1 `-a`：全部信息

```
$ uname -a
Linux ubuntu 6.1.188-233.393.amzn2023.x86_64 #1 SMP PREEMPT_DYNAMIC Tue Oct  6 20:31:38 UTC 2026 x86_64 GNU/Linux
```

依次是：内核名、主机名、内核版本、内核编译信息、硬件架构、操作系统。

### 4.2 `-r` 和 `-m`

```
$ uname -r
6.1.188-233.393.amzn2023.x86_64

$ uname -m
x86_64
```

下载软件时，看清楚要 `amd64`/`x86_64` 版还是 `arm64`/`aarch64` 版，就靠 `uname -m`。

### 4.3 组合使用

```
$ uname -srm
Linux 6.1.188-233.393.amzn2023.x86_64 x86_64
```

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 升级了内核，`uname -r` 还是旧版本 | 新内核要重启后才生效 | 重启后再看 |
| 下载的程序执行报 `cannot execute binary file: Exec format error` | 下载了不匹配的架构，比如在 ARM 服务器上装了 x86_64 版 | 按 `uname -m` 重新下载对应版本 |

## 6. 一句话总结

`uname -r` 看内核版本，`uname -m` 看 CPU 架构，发行版版本要用 `lsb_release`。
