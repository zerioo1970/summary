# 055 hostname：查看主机名和本机 IP

> 级别：初级 · 软件包：`hostname`

## 1. 作用

`hostname` 显示本机的主机名，加 `-I` 可以快速列出本机所有 IP 地址。主机名会显示在终端提示符里（`tom@ubuntu:~$` 中的 ubuntu），多台服务器同时操作时，靠它区分自己在哪台机器上。

永久修改主机名用 `hostnamectl`（中级篇讲）。

## 2. 基本格式

```bash
hostname [参数]
```

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-I` | 列出本机所有 IP 地址（不含 127.0.0.1） |
| `-f` | 完整域名（FQDN），如 `db01.example.com` |
| `-s` | 短主机名（第一个点之前的部分） |

## 4. 常用示例

> 以下输出来自测试环境。

```
$ hostname
ubuntu

$ hostname -I
172.17.0.1 169.254.255.194
```

`hostname -I` 比 `ip a` 简洁，只想知道“这台机器的 IP 是多少”时最快。多个地址用空格分隔，顺序没有固定含义。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `hostname -f` 报 `hostname: Name or service not known` | 主机名没有在 `/etc/hosts` 或 DNS 里登记 | 在 `/etc/hosts` 加一行 `127.0.1.1 主机名.域名 主机名` |
| `sudo hostname 新名字` 改了，重启后又变回去 | 这样只改了运行时的主机名 | 用 `sudo hostnamectl set-hostname 新名字` |
| 改了主机名之后，sudo 每次都很慢，提示 `unable to resolve host` | `/etc/hosts` 里还是旧主机名 | 同步修改 `/etc/hosts` |

第一条的实测：

```
$ hostname -f
hostname: Name or service not known
```

## 6. 一句话总结

`hostname` 看主机名，`hostname -I` 看本机 IP，永久修改用 `hostnamectl`。

---

[← 上一个：su](054-su.md) · [返回目录](../../README.md) · [下一个：uptime →](056-uptime.md)
