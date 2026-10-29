# 046 file：判断文件类型

> 级别：初级 · 软件包：`file`

## 1. 作用

`file` 读取文件开头的内容，判断它实际上是什么类型：文本、脚本、压缩包、图片、可执行程序等。Linux 不靠扩展名决定文件类型，一个叫 `data.txt` 的文件完全可能是个压缩包。拿到来历不明的文件，先用 file 看一眼。

## 2. 基本格式

```bash
file [参数] 文件...
```

## 3. 常用参数

| 参数 | 长格式 | 作用 |
|---|---|---|
| `-i` | `--mime` | 输出 MIME 类型和字符编码 |
| `-b` | `--brief` | 不显示文件名，只显示类型 |
| `-L` | `--dereference` | 对符号链接，判断它指向的文件 |

## 4. 常用示例

### 4.1 判断多个文件

```
$ file run.sh big.iso logs link.txt
run.sh:   Bourne-Again shell script, ASCII text executable
big.iso:  data
logs:     directory
link.txt: symbolic link to readme.txt
```

`data` 表示 file 认不出具体格式（演示用的 big.iso 只是一堆 0）。

### 4.2 文本文件和编码

```
$ file src/products.csv
src/products.csv: CSV Unicode text, UTF-8 text

$ file -i readme.txt
readme.txt: text/plain; charset=us-ascii
```

从 Windows 导出的 CSV 在 Linux 上显示乱码时，先用 `file` 看编码。下面是一个 GBK 编码的文件：

```
$ file gbk.csv
gbk.csv: ISO-8859 text

$ iconv -f GBK -t UTF-8 gbk.csv
探头,主机
```

file 不认识 GBK，会显示成 `ISO-8859` 或 `Non-ISO extended-ASCII`。看到这两种，多半就是 GBK，用 `iconv -f GBK -t UTF-8 原文件 > 新文件` 转换。

### 4.3 扩展名会骗人

```
$ cp a.tgz a.txt; file a.txt
a.txt: gzip compressed data, from Unix, original size modulo 2^32 10240
```

文件叫 a.txt，实际是 gzip 压缩包。

### 4.4 可执行程序

```
$ file /usr/bin/grep
/usr/bin/grep: ELF 64-bit LSB pie executable, x86-64, version 1 (SYSV), dynamically linked, interpreter /lib64/ld-linux-x86-64.so.2, BuildID[sha1]=9601f9b5d319a231b14e6c6289e998da074aa543, for GNU/Linux 3.2.0, stripped
```

`x86-64` 说明是给 Intel/AMD 64 位 CPU 编译的程序。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| CSV 是 GBK 编码，file 却显示成别的编码 | 文件里中文很少时，file 的判断依据不足 | 用 `iconv -f GBK -t UTF-8` 试着转换，看结果是否正常 |
| 对链接只显示 `symbolic link to ...` | file 默认不跟随链接 | 加 `-L` |

## 6. 一句话总结

不确定文件是什么就 `file` 一下，查编码加 `-i`。
