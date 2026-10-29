# 094 updatedb：更新 locate 的数据库

> 级别：初级 · 软件包：`plocate`

## 1. 作用

`updatedb` 扫描整个文件系统，重建 [locate](093-locate.md) 使用的文件名数据库。系统每天会自动执行一次，平时只有在“刚放进去的文件 locate 找不到”时才需要手动执行。

## 2. 基本格式

```bash
sudo updatedb
```

需要 root 权限，因为它要读取所有目录。执行成功不会有输出。

## 3. 自动更新的时间

Ubuntu 26.04 用 systemd 定时器 `plocate-updatedb.timer` 每天执行一次，单元文件里的设置是：

```
OnCalendar=daily
RandomizedDelaySec=1h
```

即每天零点之后的 1 小时内随机执行。

## 4. 哪些位置不收录

设置在 `/etc/updatedb.conf` 里：

```
$ grep -E "^PRUNEPATHS" /etc/updatedb.conf
PRUNEPATHS="/tmp /var/spool /media /var/lib/os-prober /var/lib/ceph /home/.ecryptfs /var/lib/schroot"
```

| 设置项 | 作用 |
|---|---|
| `PRUNEPATHS` | 不收录的目录，如 /tmp、/media（U 盘挂载的位置） |
| `PRUNEFS` | 不收录的文件系统类型，如 nfs、cifs（网络共享）、tmpfs |
| `PRUNE_BIND_MOUNTS` | 不重复收录绑定挂载的目录 |

不收录网络共享，是为了避免每天扫描一遍远程的海量文件。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 普通用户执行报 `/var/lib/plocate/: Permission denied` | 数据库目录只有 root 能写 | 加 `sudo` |
| 执行了 updatedb，NAS 共享目录里的文件还是找不到 | 网络文件系统在 PRUNEFS 里被排除 | 在共享目录里用 find |

## 6. 一句话总结

locate 找不到新文件时 `sudo updatedb`，平时系统每天自动更新。

---

[← 上一个：locate](093-locate.md) · [返回目录](../../README.md) · [下一个：lscpu →](095-lscpu.md)
