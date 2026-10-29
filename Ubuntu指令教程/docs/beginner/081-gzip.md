# 081 gzip：压缩单个文件

> 级别：初级 · 软件包：`gzip`

## 1. 作用

`gzip` 把一个文件压缩成 `.gz` 格式。日志轮转后的旧日志（`syslog.2.gz`）、`.tar.gz` 里的压缩层，用的都是 gzip。文本类文件（日志、CSV、SQL 导出）通常能压到原来的 10%–50%。

gzip 只压缩**单个文件**，并且默认会**用压缩后的文件替换原文件**。要把多个文件或目录打成一个包，用 [tar](035-tar.md)。

## 2. 基本格式

```bash
gzip [参数] 文件...
```

## 3. 常用参数

| 参数 | 长格式 | 作用 |
|---|---|---|
| `-k` | `--keep` | 保留原文件 |
| `-d` | `--decompress` | 解压，等同 gunzip |
| `-l` | `--list` | 查看压缩率 |
| `-c` | `--stdout` | 输出到屏幕（标准输出），原文件不动 |
| `-r` | `--recursive` | 把目录里的每个文件分别压缩 |
| `-v` | `--verbose` | 显示压缩率 |
| `-1` … `-9` | | 压缩级别：1 最快，9 压得最小，默认 6 |

## 4. 常用示例

### 4.1 压缩

```
$ gzip app.log; ls -l app.log*
-rw-r--r-- 1 tom tom 234 Oct 29 07:08 app.log.gz
```

原来的 app.log 不见了，只剩 app.log.gz。

### 4.2 `-l`：看压缩率

```
$ gzip -l app.log.gz
         compressed        uncompressed  ratio uncompressed_name
                234                 404  48.5% app.log
```

### 4.3 `-k`：保留原文件

```
$ gzip -k app.log; ls app.log*
app.log  app.log.gz
```

### 4.4 `-9 -v`：最大压缩并显示结果

```
$ gzip -9 -v app.log
app.log:         48.5% -- replaced with app.log.gz
```

### 4.5 不解压直接查看

```
$ zcat app.log.gz | head -2
2026-10-28 09:00:01 INFO  服务启动
2026-10-28 09:00:05 INFO  连接数据库成功
```

`zcat`、`zless`、`zgrep` 可以直接读 .gz 文件，查旧日志时不用先解压。

### 4.6 `-r`：逐个压缩目录里的文件

```
$ gzip -r logs; ls logs
app.log.gz
```

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 压缩后原文件没了 | gzip 默认替换原文件 | 需要保留就加 `-k` |
| `gzip: app.log.gz already exists;        not overwritten` | 同名的 .gz 已经存在 | 删掉旧的 .gz，或加 `-f` 强制覆盖 |
| `gzip: logs is a directory -- ignored` | gzip 不能把目录压成一个文件 | 用 `tar -czf logs.tar.gz logs` |
| 压缩正在写入的日志，程序继续写的内容丢了 | 程序仍往原来那个（已被删除的）文件里写 | 先让程序切换日志文件（logrotate 会处理），再压缩旧文件 |

## 6. 一句话总结

`gzip 文件` 压缩并替换，`-k` 保留原文件，查看 .gz 用 `zcat`、`zgrep`。

---

[← 上一个：tee](080-tee.md) · [返回目录](../../README.md) · [下一个：gunzip →](082-gunzip.md)
