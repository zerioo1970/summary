# 093 locate：按文件名快速查找

> 级别：初级 · 软件包：`plocate`（`locate` 是指向 plocate 的链接）

## 1. 作用

`locate` 按文件名在整个系统里查找，几乎瞬间出结果。它快的原因是：并不现场搜索磁盘，而是查一个事先建好的文件名数据库（`/var/lib/plocate/plocate.db`）。这个数据库每天自动更新一次。

| | locate | [find](022-find.md) |
|---|---|---|
| 速度 | 很快 | 目录多时很慢 |
| 结果是否最新 | 只到上次更新数据库为止 | 实时 |
| 能按大小、时间等条件找 | 不能，只按名字 | 能 |

## 2. 基本格式

```bash
locate [参数] 模式
```

模式里不带通配符时，会匹配路径中任何位置包含这段文字的文件。

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-i` | 忽略大小写 |
| `-c` | 只输出匹配的数量 |
| `-n 数量` | 最多输出 N 条 |
| `-e` | 只显示现在仍然存在的文件 |
| `-b` | 只匹配文件名部分，不匹配目录名 |

## 4. 常用示例

以下以 root 身份执行（测试环境限制，普通用户在真实系统上同样可以使用）。

### 4.1 找配置文件在哪

```
$ locate sshd_config
/etc/ssh/sshd_config
/etc/ssh/sshd_config.d
/usr/share/man/man5/sshd_config.5.gz
/usr/share/openssh/sshd_config
/usr/share/openssh/sshd_config.md5sum
/var/lib/ucf/cache/:etc:ssh:sshd_config
```

### 4.2 `-i`、`-c`、`-n`

```
$ locate -i "SSHD_CONFIG" | head -2
/etc/ssh/sshd_config
/etc/ssh/sshd_config.d

$ locate -c "*.conf"
131
```

### 4.3 新文件找不到，删掉的还在

```
$ touch /opt/new-report.xlsx; locate new-report.xlsx; echo "退出码=$?"
退出码=1

$ sudo updatedb; locate new-report.xlsx
/opt/new-report.xlsx

$ rm /opt/new-report.xlsx; locate new-report.xlsx
/opt/new-report.xlsx

$ locate -e new-report.xlsx; echo "退出码=$?"
退出码=1
```

刚创建的文件要等 [updatedb](094-updatedb.md) 更新数据库后才能找到；删掉的文件在下次更新前还会出现，加 `-e` 可以过滤掉。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 刚下载的文件 locate 找不到 | 数据库还没更新 | `sudo updatedb` 后再找，或用 find |
| `/tmp`、U 盘里的文件永远找不到 | 这些位置在 `/etc/updatedb.conf` 里被设为不收录 | 用 find |
| `locate: command not found` | 没装 plocate | `sudo apt install plocate` |

## 6. 一句话总结

按名字找文件先用 `locate`，找不到再 `sudo updatedb` 或改用 find。

---

[← 上一个：stat](092-stat.md) · [返回目录](../../README.md) · [下一个：updatedb →](094-updatedb.md)
