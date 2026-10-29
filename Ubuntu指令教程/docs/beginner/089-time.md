# 089 time：统计命令的执行时间

> 级别：初级 · 来源：bash 关键字（另有程序版 `/usr/bin/time`，需要单独安装）

## 1. 作用

`time` 放在任何命令前面，命令执行完后报告它花了多长时间。对比两种写法哪个更快、确认一个备份脚本要跑多久，都用它。

## 2. 基本格式

```bash
time 命令
```

## 3. 读懂输出

```
$ time sleep 1

real    0m1.002s
user    0m0.002s
sys     0m0.000s
```

| 行 | 含义 |
|---|---|
| real | 从开始到结束实际经过的时间（墙上时钟） |
| user | 程序自身代码占用 CPU 的时间 |
| sys | 程序请求系统内核做事（读写文件、网络）占用 CPU 的时间 |

sleep 1 的 real 是 1 秒，但 user 和 sys 几乎为 0：它一直在等待，没有使用 CPU。

## 4. 常用示例

### 4.1 统计一组命令的时间

```
$ time (find / -name "*.conf" 2>/dev/null | wc -l)
131

real    0m0.075s
user    0m0.010s
sys     0m0.067s
```

多条命令或带管道时，用括号括起来，统计的是整组命令。

### 4.2 根据结果判断瓶颈

| 现象 | 说明 |
|---|---|
| real 远大于 user + sys | 大部分时间在等待：等磁盘、等网络、等数据库 |
| user 接近 real | 程序在密集计算 |
| sys 偏高 | 大量读写文件或系统调用 |

导出报表的脚本跑了 10 分钟，`time` 一看 user 只有 5 秒，说明问题出在等数据库查询，而不是脚本本身。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `time` 的输出没法用 `> 文件` 重定向保存 | bash 的 time 把结果写到 Shell 的错误输出，不属于被测命令 | `{ time 命令 ; } 2> time.txt` |
| 网上教程里的 `time -v` 报错 | `-v` 是程序版 `/usr/bin/time` 的参数，bash 的 time 不支持 | `sudo apt install time`，然后用 `/usr/bin/time -v 命令` |

## 6. 一句话总结

`time 命令` 看耗时；real 远大于 user+sys，说明时间花在了等待上。

---

[← 上一个：watch](088-watch.md) · [返回目录](../../README.md) · [下一个：basename →](090-basename.md)
