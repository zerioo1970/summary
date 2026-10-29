# 074 last：查看登录历史

> 级别：初级 · 软件包：`wtmpdb`（Ubuntu 26.04 中 `last` 由 wtmpdb 提供）

## 1. 作用

`last` 列出过去的登录记录：谁、从哪个 IP、什么时候登录、什么时候退出，以及系统的开机和关机记录。服务器出了问题，想知道“出事前谁登录过”，或者怀疑有人非法登录时，用它查。

在 Ubuntu 26.04 上，登录记录由 wtmpdb 保存在 `/var/log/wtmp.db`（一个 SQLite 数据库），`last` 是指向 wtmpdb 的链接：

```
$ ls -l /usr/bin/last
lrwxrwxrwx 1 root root 6 Dec  2  2025 /usr/bin/last -> wtmpdb
```

## 2. 基本格式

```bash
last [参数] [用户名]
```

## 3. 常用参数

| 参数 | 长格式 | 作用 |
|---|---|---|
| `-n 数量` | `--limit` | 只显示最近 N 条 |
| `-x` | `--system` | 同时显示系统关机、开机记录 |
| `-F` | `--fulltimes` | 显示完整的日期时间 |
| `-i` | `--ip` | 来源显示成 IP 地址 |
| `-s 时间` | `--since` | 只看某个时间之后的记录 |
| `-j` | `--json` | 输出 JSON |

以上参数取自 Ubuntu 26.04 上 wtmpdb 的帮助信息。

## 4. 常用示例

```bash
last -n 10           # 最近 10 次登录
last tom             # tom 的登录记录
last -x -n 5         # 包括开机、关机在内的最近 5 条
last -s yesterday    # 昨天以来的记录
```

> ⚠️ 本节未实机验证：测试环境没有登录记录，`last` 报 `Cannot open database (/var/log/wtmp.db) read-only: unable to open database file`。

登录失败的记录 last 不显示。要查有没有人在暴力猜密码，看 ssh 的日志：

```bash
sudo journalctl -u ssh --since today | grep "Failed password"
```

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `last: command not found` | 精简安装可能没有 wtmpdb 包 | `sudo apt install wtmpdb` |
| 网上教程说读取 `/var/log/wtmp`，Ubuntu 26.04 上没有这个文件 | 26.04 改用了 wtmpdb，记录存在 `/var/log/wtmp.db` | 直接用 `last` 命令，不用管文件位置 |

## 6. 一句话总结

`last -n 10` 查最近谁登录过，加 `-x` 看开关机记录。

---

[← 上一个：w](073-w.md) · [返回目录](../../README.md) · [下一个：env →](075-env.md)
