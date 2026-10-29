# 082 gunzip：解压 .gz 文件

> 级别：初级 · 软件包：`gzip`

## 1. 作用

`gunzip` 解压 `.gz` 文件，还原出原文件，并删除 .gz 文件。它等同于 `gzip -d`。

`.tar.gz` 文件不要用 gunzip，用 `tar -xzf` 一步解开。

## 2. 基本格式

```bash
gunzip [参数] 文件.gz...
```

## 3. 常用参数

| 参数 | 长格式 | 作用 |
|---|---|---|
| `-k` | `--keep` | 保留 .gz 文件 |
| `-c` | `--stdout` | 解压到屏幕（标准输出），.gz 文件不动 |
| `-r` | `--recursive` | 解压目录里所有的 .gz |
| `-f` | `--force` | 目标文件已存在时强制覆盖 |

## 4. 常用示例

### 4.1 解压

```
$ gunzip app.log.gz; ls -l app.log*
-rw-r--r-- 1 tom tom 404 Oct 29 07:08 app.log
```

### 4.2 `-c`：解压成另一个文件，保留 .gz

```
$ gunzip -c app.log.gz > copy.log; ls app.log* copy.log
app.log.gz  copy.log
```

### 4.3 `gzip -d`：效果相同

```
$ gzip -d app.log.gz; ls app.log*
app.log
```

### 4.4 `-r`：解压整个目录

```
$ gunzip -r logs; ls logs
app.log
```

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `gzip: nothere.gz: No such file or directory` | 文件名写错 | 用 ls 确认，输入时按 Tab 补全 |
| 对 .tar.gz 用了 gunzip，得到一个 .tar 文件 | gunzip 只去掉 gzip 那一层 | 继续 `tar -xf 文件.tar`；以后直接 `tar -xzf 文件.tar.gz` |
| 只是想查一下旧日志，解压后占满了磁盘 | 大日志解压后体积会变成好几倍 | 用 `zgrep 关键词 文件.gz` 或 `zless` 直接查 |

## 6. 一句话总结

`gunzip 文件.gz` 解压，`-k` 保留压缩包，只是查看就用 `zcat`。

---

[← 上一个：gzip](081-gzip.md) · [返回目录](../../README.md) · [下一个：ssh →](083-ssh.md)
