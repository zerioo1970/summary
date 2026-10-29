# 031 ping：测试网络是否连通

> 级别：初级 · 软件包：`iputils-ping`

## 1. 作用

`ping` 向目标主机发送 ICMP 回显请求（一种很小的探测包），看对方是否回应、往返花了多长时间。网络出问题时，用它判断“到底是不通，还是慢，还是域名解析不了”。

## 2. 基本格式

```bash
ping [参数] 主机名或IP
```

Linux 上的 ping 默认会一直发下去，按 `Ctrl+C` 停止。

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-c 次数` | 发送指定次数后自动停止 |
| `-i 秒` | 每次发送的间隔，默认 1 秒 |
| `-W 秒` | 等待每个回应的超时时间 |
| `-q` | 只显示最后的统计结果 |
| `-4` / `-6` | 强制使用 IPv4 / IPv6 |

## 4. 常用示例

### 4.1 `-c`：发 4 次

```
$ ping -c 4 127.0.0.1
PING 127.0.0.1 (127.0.0.1) 56(84) bytes of data.
64 bytes from 127.0.0.1: icmp_seq=1 ttl=127 time=0.036 ms
64 bytes from 127.0.0.1: icmp_seq=2 ttl=127 time=0.044 ms
64 bytes from 127.0.0.1: icmp_seq=3 ttl=127 time=0.042 ms
64 bytes from 127.0.0.1: icmp_seq=4 ttl=127 time=0.034 ms

--- 127.0.0.1 ping statistics ---
4 packets transmitted, 4 received, 0% packet loss, time 3146ms
rtt min/avg/max/mdev = 0.034/0.039/0.044/0.004 ms
```

| 内容 | 含义 |
|---|---|
| `icmp_seq` | 第几个包。序号不连续说明中间丢了包 |
| `time=0.036 ms` | 往返时间。局域网一般小于 1 ms，国内跨城几十 ms |
| `0% packet loss` | 丢包率。偶尔丢 1% 可以接受，持续丢包说明线路有问题 |
| `rtt min/avg/max/mdev` | 往返时间的最小 / 平均 / 最大 / 波动 |

### 4.2 不通的样子

```
$ ping -c 2 -W 1 10.255.255.1
PING 10.255.255.1 (10.255.255.1) 56(84) bytes of data.

--- 10.255.255.1 ping statistics ---
2 packets transmitted, 0 received, 100% packet loss, time 1039ms
```

没有任何回应，丢包 100%。

### 4.3 域名解析失败

```
$ ping -c 1 nothere.invalid
ping: nothere.invalid: Name or service not known
```

连第一行的 `PING ...` 都没有出现，说明问题在 DNS（域名解析），还没开始发包。这时可以直接 ping IP，如果 IP 能通，就去查 DNS 设置。

### 4.4 `-q`：只看统计

```
$ ping -c 3 -i 0.5 -q 127.0.0.1
PING 127.0.0.1 (127.0.0.1) 56(84) bytes of data.

--- 127.0.0.1 ping statistics ---
3 packets transmitted, 3 received, 0% packet loss, time 1033ms
rtt min/avg/max/mdev = 0.018/0.030/0.038/0.008 ms
```

## 5. 排查思路

按从近到远的顺序 ping，哪一步不通，问题就在哪一段：

| 顺序 | 命令 | 不通说明 |
|---|---|---|
| 1 | `ping -c 2 127.0.0.1` | 本机网络协议栈有问题（极少见） |
| 2 | `ping -c 2 网关IP`（用 `ip route` 查） | 网线、Wi-Fi 或本机网卡配置有问题 |
| 3 | `ping -c 2 223.5.5.5`（公共 DNS 的 IP） | 出不了外网 |
| 4 | `ping -c 2 www.baidu.com` | 外网通但域名解析有问题 |

## 6. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| ping 一个网站 100% 丢包，但浏览器能打开 | 很多服务器和云防火墙禁止 ICMP，ping 不通不代表服务不可用 | 用 `curl -I https://网站` 测试 HTTP 是否通 |
| 在脚本里 ping 一直不结束 | 没加 `-c` | 脚本里一定写 `-c 次数` 和 `-W 超时` |
| 能 ping 通数据库服务器，程序还是连不上 | ping 只说明主机在线，不代表端口开放 | 用 `nc -zv 主机 端口` 测端口 |

本教程测试环境就禁止了出外网的 ICMP，下面这个结果就是第一条的实例（archive.ubuntu.com 的网站本身可以正常访问）：

```
$ ping -c 2 archive.ubuntu.com
PING archive.ubuntu.com (91.189.91.82) 56(84) bytes of data.

--- archive.ubuntu.com ping statistics ---
2 packets transmitted, 0 received, 100% packet loss, time 1087ms
```

## 7. 一句话总结

`ping -c 4 主机` 测连通；ping 不通不等于服务不可用，测端口用 `nc`。
