# 037 unzip：解压 zip 文件

> 级别：初级 · 软件包：`unzip`

## 1. 作用

`unzip` 解压 `.zip` 文件，也可以只查看或测试压缩包的内容。从 Windows 同事那里收到的压缩包、从网上下载的 zip 软件包，都用它解。

## 2. 基本格式

```bash
unzip [参数] 压缩包.zip [要解出的文件...] [-d 目标目录]
```

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-d 目录` | 解压到指定目录（不存在会自动创建） |
| `-l` | 只列出内容，不解压 |
| `-o` | 遇到同名文件直接覆盖，不询问 |
| `-n` | 遇到同名文件一律跳过 |
| `-q` | 不显示过程 |
| `-t` | 测试压缩包是否完整 |
| `-O 编码` | 指定文件名的编码，解决中文文件名乱码，如 `-O GBK` |

## 4. 常用示例

### 4.1 `-l`：先看内容

```
$ unzip -l pkg.zip
Archive:  pkg.zip
  Length      Date    Time    Name
---------  ---------- -----   ----
        0  2026-10-29 06:55   logs/
      404  2026-10-28 11:30   logs/app.log
        0  2026-10-29 06:55   src/
      152  2026-10-29 06:55   src/products.csv
        6  2026-10-01 09:00   readme.txt
---------                     -------
      562                     5 files
```

### 4.2 `-d`：解压到指定目录

```
$ unzip pkg.zip -d out
Archive:  pkg.zip
   creating: out/logs/
  inflating: out/logs/app.log
   creating: out/src/
  inflating: out/src/products.csv
 extracting: out/readme.txt
```

### 4.3 遇到同名文件

再解压一次到同一个目录，unzip 会逐个询问：

```
replace out/logs/app.log? [y]es, [n]o, [A]ll, [N]one, [r]ename:
```

| 输入 | 效果 |
|---|---|
| `y` / `n` | 覆盖 / 跳过这一个 |
| `A` | 后面全部覆盖 |
| `N` | 后面全部跳过 |
| `r` | 改个名字保存 |

不想被询问：覆盖用 `unzip -o`，跳过用 `unzip -n`。

### 4.4 只解出一个文件

```
$ unzip pkg.zip readme.txt -d one
Archive:  pkg.zip
 extracting: one/readme.txt
```

### 4.5 `-t`：检查压缩包是否损坏

```
$ unzip -t pkg.zip
Archive:  pkg.zip
    testing: logs/                    OK
    testing: logs/app.log             OK
    testing: src/                     OK
    testing: src/products.csv         OK
    testing: readme.txt               OK
No errors detected in compressed data of pkg.zip.
```

下载的大文件解压报错时，先用 `-t` 确认是不是下载不完整。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| Windows 上打包的 zip，解压后中文文件名是乱码 | Windows 中文版的 zip 工具用 GBK 编码保存文件名，Linux 默认按 UTF-8 解读 | `unzip -O GBK 文件.zip` |
| 解压后当前目录多了一堆散乱的文件 | 压缩包里没有外层目录 | 先 `unzip -l` 看结构，再用 `-d 新目录` 解压 |
| `unzip: cannot find or open nothere.zip, nothere.zip.zip or nothere.zip.ZIP.` | 文件名或路径写错 | 用 `ls` 确认文件名，输入时按 Tab 补全 |
| `unzip: command not found` | 部分最小化安装没有 unzip | `sudo apt install unzip` |

> ⚠️ 第一条未实机验证：测试环境没有 Windows 生成的压缩包。`-O` 参数已在 Ubuntu 26.04 的 `unzip -h` 中确认存在。

## 6. 一句话总结

先 `unzip -l` 看结构，再 `unzip 包 -d 目录`；中文乱码加 `-O GBK`。

---

[← 上一个：zip](036-zip.md) · [返回目录](../../README.md) · [下一个：reboot →](038-reboot.md)
