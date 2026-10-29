# 033 wget：从网上下载文件

> 级别：初级 · 软件包：`wget`（Ubuntu 26.04 自带 1.25 版）

## 1. 作用

`wget` 通过 HTTP、HTTPS、FTP 下载文件，保存到当前目录。服务器上没有浏览器，下载安装包、数据文件、脚本都靠它。它支持断点续传，网络断了可以接着下。

和 [curl](034-curl.md) 的分工：wget 专注“把文件下载下来”，用法简单；curl 更适合调试接口、发 POST 请求。

## 2. 基本格式

```bash
wget [参数] 网址
```

## 3. 常用参数

| 参数 | 长格式 | 作用 |
|---|---|---|
| `-O 文件名` | `--output-document` | 保存成指定的文件名（大写字母 O） |
| `-P 目录` | `--directory-prefix` | 保存到指定目录 |
| `-c` | `--continue` | 断点续传 |
| `-q` | `--quiet` | 不输出任何信息 |
| `-nv` | `--no-verbose` | 只输出一行结果 |
| `-b` | `--background` | 后台下载，日志写入 wget-log |
| `-i 文件` | `--input-file` | 从文件里读取一批网址逐个下载 |
| | `--limit-rate=1m` | 限速，避免占满带宽 |

## 4. 常用示例

### 4.1 直接下载

```
$ wget https://cdimage.ubuntu.com/ubuntu-base/releases/26.04.1/release/SHA256SUMS
...
     0K .                                                     100% 76.5M=0s

2026-10-29 06:55:38 (76.5 MB/s) - ‘SHA256SUMS’ saved [1244/1244]
```

下载大文件时，这里会显示进度条、速度和剩余时间。

### 4.2 `-O`：换个名字保存

```
$ wget -nv -O sums.txt https://cdimage.ubuntu.com/ubuntu-base/releases/26.04.1/release/SHA256SUMS
2026-10-29 06:55:37 URL:https://cdimage.ubuntu.com/ubuntu-base/releases/26.04.1/release/SHA256SUMS [1244/1244] -> "sums.txt" [1]
```

### 4.3 `-P`：保存到指定目录

```
$ wget -nv -P downloads https://cdimage.ubuntu.com/ubuntu-base/releases/26.04.1/release/SHA256SUMS
2026-10-29 06:55:38 URL:https://cdimage.ubuntu.com/ubuntu-base/releases/26.04.1/release/SHA256SUMS [1244/1244] -> "downloads/SHA256SUMS" [1]
```

目录不存在时 wget 会自动创建。

### 4.4 `-c`：断点续传

```bash
wget -c https://releases.ubuntu.com/26.04/ubuntu-26.04-live-server-amd64.iso
```

下载几 GB 的镜像中途断了，再执行同一条命令加 `-c`，会从断开的地方继续。文件已经完整时，`-c` 什么也不做。

### 4.5 下载失败的样子

```
$ wget -nv https://cdimage.ubuntu.com/nothere.txt
https://cdimage.ubuntu.com/nothere.txt:
2026-10-29 06:55:38 ERROR 404: Not Found.
```

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 同一个文件下载两次，第二次保存成了 `SHA256SUMS.1` | 文件已存在时，wget 不覆盖，而是加编号 | 用 `-O 文件名` 指定名字会直接覆盖；续传用 `-c` |
| 网址里带 `&`，下载的东西不对，终端还多出一个后台任务 | `&` 被 Shell 当成“放到后台运行” | 网址加引号：`wget "https://a.com/x?a=1&b=2"` |
| 下载的文件打不开，内容其实是一个网页 | 下载链接需要登录，或者会跳转到下载页 | 换成直链；需要带登录凭据时改用 curl |

第一条的实测：

```
$ ls
SHA256SUMS    big.iso   logs        run.sh  sums.txt
SHA256SUMS.1  link.txt  readme.txt  src
```

## 6. 一句话总结

`wget 网址` 下载，`-O` 改名，`-P` 指定目录，大文件加 `-c` 续传。

---

[← 上一个：ip](032-ip.md) · [返回目录](../../README.md) · [下一个：curl →](034-curl.md)
