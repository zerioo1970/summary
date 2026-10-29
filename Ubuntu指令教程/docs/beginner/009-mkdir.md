# 009 mkdir：新建目录

> 级别：初级 · 软件包：`rust-coreutils`（系统自带）

## 1. 作用

`mkdir`（make directory）新建目录。部署程序前建好 `logs`、`data`、`backup` 这些目录，用的就是它。

## 2. 基本格式

```bash
mkdir [参数] 目录名...
```

## 3. 常用参数

| 参数 | 长格式 | 作用 |
|---|---|---|
| `-p` | `--parents` | 自动创建缺少的上级目录；目录已存在也不报错 |
| `-v` | `--verbose` | 显示创建了哪些目录 |
| `-m` | `--mode` | 创建时直接指定权限，如 `-m 700` |

## 4. 常用示例

### 4.1 新建一个目录

```
$ mkdir backup

$ mkdir backup
mkdir: backup: File exists
```

### 4.2 `-p`：一次建多层

```
$ mkdir a/b/c
mkdir: No such file or directory

$ mkdir -p a/b/c

$ mkdir -p a/b/c
```

不加 `-p`，上级目录 a/b 不存在就报错。加了 `-p`，目录已存在时再执行也不报错，所以部署脚本里一律写 `mkdir -p`。

### 4.3 `-pv` 加花括号：批量建目录

```
$ mkdir -pv 2026/{01,02,03}
mkdir: created directory '2026'
mkdir: created directory '2026/01'
mkdir: created directory '2026/02'
mkdir: created directory '2026/03'
```

`{01,02,03}` 由 bash 展开成三个名字。按月归档报表时很方便，`{01..12}` 一次建出 12 个月。

### 4.4 `-m`：创建时指定权限

```
$ mkdir -m 700 secret

$ ls -ld secret
drwx------ 2 tom tom 4096 Oct 29 06:41 secret
```

700 表示只有自己能进入。存放密钥、证书的目录适合这样建。权限数字的含义见 [chmod](023-chmod.md)。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `mkdir My Docs` 建出了 `My` 和 `Docs` 两个目录 | 空格把名字拆成了两个参数 | 加引号：`mkdir "My Docs"`。Linux 下的目录名最好不用空格 |
| 在 `/opt` 下建目录报 `mkdir: Permission denied` | 普通用户没有 /opt 的写权限 | `sudo mkdir /opt/app`，再用 `sudo chown tom:tom /opt/app` 把目录交给自己 |
| 脚本第二次运行时在 mkdir 处报 `File exists` | 目录在第一次运行时已经建好了 | 改用 `mkdir -p` |

Ubuntu 26.04 的 mkdir 由 Rust 版 coreutils 提供，报错信息比较简短，例如 `mkdir: No such file or directory` 不会写出是哪个路径。看到这类报错，回头检查命令里的路径即可。

## 6. 一句话总结

建目录一律用 `mkdir -p`，批量建配合 `{}`，私密目录加 `-m 700`。

---

[← 上一个：rm](008-rm.md) · [返回目录](../../README.md) · [下一个：touch →](010-touch.md)
