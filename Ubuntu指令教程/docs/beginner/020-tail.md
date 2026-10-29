# 020 tail：查看文件末尾，实时跟踪日志

> 级别：初级 · 软件包：`rust-coreutils`（系统自带）

## 1. 作用

`tail` 显示文件的最后几行，默认 10 行。日志是不断往末尾追加的，最新的内容在最后，所以查日志最常用的是 tail。

加 `-f` 后 tail 不退出，文件一有新内容就马上显示，用来盯着程序的实时日志。

## 2. 基本格式

```bash
tail [参数] [文件...]
```

## 3. 常用参数

| 参数 | 长格式 | 作用 |
|---|---|---|
| `-n 数字` | `--lines` | 显示最后 N 行，可简写成 `-数字` |
| `-n +数字` | | 从第 N 行开始显示到末尾 |
| `-f` | `--follow` | 持续跟踪文件新增的内容，按 `Ctrl+C` 退出 |
| `-F` | | 和 `-f` 一样，而且文件被删除重建（日志轮转）后会继续跟踪新文件 |
| `-c 数字` | `--bytes` | 显示最后 N 个字节 |

## 4. 常用示例

### 4.1 看最后 2 行

```
$ tail -n 2 logs/app.log
2026-10-28 11:30:40 ERROR 连接数据库失败
2026-10-28 11:30:45 INFO  连接数据库成功
```

### 4.2 `-n +7`：从第 7 行开始

```
$ tail -n +7 logs/app.log
2026-10-28 11:30:40 ERROR 连接数据库失败
2026-10-28 11:30:45 INFO  连接数据库成功
```

`tail -n +2 products.csv` 能去掉 CSV 的第一行表头，只留数据。

### 4.3 `-f`：实时跟踪日志

```
$ tail -f logs/app.log
2026-10-28 09:00:01 INFO  服务启动
2026-10-28 09:00:05 INFO  连接数据库成功
...（先显示最后 10 行，然后停在这里等待）
```

另开一个终端操作程序，新日志一写入，这里马上就会显示出来。按 `Ctrl+C` 退出。

### 4.4 跟踪时只看错误

```bash
tail -f logs/app.log | grep ERROR
```

### 4.5 跟踪多个文件

```bash
tail -f /var/log/nginx/access.log /var/log/nginx/error.log
```

tail 会在每段输出前用 `==> 文件名 <==` 标出来自哪个文件。

### 4.6 `-f` 和 `-F` 的区别

下面模拟日志轮转：1 秒后把 t.log 改名为 t.log.1，再新建一个 t.log 写入 `new`。

```
$ tail -F t.log
old
tail: 't.log' has become inaccessible: No such file or directory
tail: 't.log' has appeared;  following new file
new

$ tail -f u.log
old
```

`-F` 发现文件换了，自动跟上新文件，显示出了 `new`。`-f` 还盯着改名后的旧文件，新内容一直看不到。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `tail -f` 跟踪了一夜，第二天突然不再有新日志 | 日志在半夜被轮转（改名成 app.log.1，再新建 app.log），`-f` 还在跟踪改名后的旧文件 | 改用 `tail -F` |
| `tail -f 文件 \| grep xxx \| 另一个命令` 迟迟没有输出 | grep 的输出接到管道时会攒满一块再输出 | 给 grep 加 `--line-buffered` |
| 用 systemd 管理的服务找不到日志文件 | 很多服务的日志不写文件，而是交给 journald | 用 `journalctl -u 服务名 -f` 跟踪 |

## 6. 一句话总结

`tail -n N` 看最后 N 行，盯实时日志用 `tail -F`。

---

[← 上一个：head](019-head.md) · [返回目录](../../README.md) · [下一个：grep →](021-grep.md)
