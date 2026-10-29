# 086 fastfetch：一屏看清系统概况

> 级别：初级 · 软件包：`fastfetch`（Ubuntu 26.04 为 2.57 版；没有就 `sudo apt install fastfetch`）

## 1. 作用

`fastfetch` 一次性显示系统的概要信息：发行版、内核、运行时间、CPU、内存、磁盘、Shell 等，左边还会配一个发行版的字符画 logo。拿到一台新服务器，或者要给别人报告“我的环境是什么”时，一条命令就够了。

它取代了以前常用的 neofetch（neofetch 已经停止维护）。

## 2. 基本格式

```bash
fastfetch [参数]
```

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `--logo none` | 不显示 logo，只显示信息（方便复制） |
| `--logo small` | 显示小尺寸的 logo |
| `--structure 模块:模块` | 只显示指定的几项，如 `OS:Kernel:Memory` |
| `--format json` | 输出 JSON，方便脚本处理 |

## 4. 常用示例

```
$ fastfetch --logo none
tom@ubuntu
----------
OS: Ubuntu 26.04.1 LTS (Resolute Raccoon) x86_64
Host: m7i-flex.2xlarge
Kernel: Linux 6.1.188-233.393.amzn2023.x86_64
Uptime: 1 hour, 35 mins
Packages: 259 (dpkg)
Shell: bash 5.3.9
Terminal: runuser
CPU: Intel(R) Xeon(R) Platinum 8488C (8) @ 2.40 GHz
GPU: Amazon.com, Inc. Device 1111 (VGA compatible)
Memory: 2.41 GiB / 30.82 GiB (8%)
Swap: 0 B / 2.00 GiB (0%)
Disk (/): 1.39 GiB / 86.81 GiB (2%) - xfs
...
```

> 以上输出来自测试环境（云服务器上的容器），所以 Host 是云主机型号，内核是宿主机的内核。

| 项 | 等同于哪条命令 |
|---|---|
| OS | `lsb_release -d` |
| Kernel | `uname -r` |
| Uptime | `uptime -p` |
| Packages | `dpkg -l` 的数量 |
| CPU | `lscpu` |
| Memory | `free -h` |
| Disk | `df -h /` |

## 5. 一句话总结

`fastfetch` 一条命令看系统全貌，复制给别人看时加 `--logo none`。

---

[← 上一个：htop](085-htop.md) · [返回目录](../../README.md) · [下一个：sleep →](087-sleep.md)
