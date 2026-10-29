# 024 chown：修改文件的所有者和所属组

> 级别：初级 · 软件包：`rust-coreutils`（系统自带）

## 1. 作用

`chown`（change owner）修改文件或目录属于哪个用户、哪个组。典型场景：用 sudo 把程序拷到 `/opt/app` 后，文件都属于 root，程序以普通账号运行时写不了日志，这时要用 chown 把目录交给运行账号。

修改所有者需要 root 权限，所以基本都要加 sudo。

## 2. 基本格式

```bash
sudo chown [参数] 用户[:组] 文件...
```

| 写法 | 含义 |
|---|---|
| `chown tom 文件` | 只改所有者 |
| `chown tom:www-data 文件` | 同时改所有者和所属组 |
| `chown :tom 文件` | 只改所属组（等同 `chgrp tom 文件`） |

## 3. 常用参数

| 参数 | 长格式 | 作用 |
|---|---|---|
| `-R` | `--recursive` | 递归修改目录及其中所有内容 |
| `-v` | `--verbose` | 显示处理了哪些文件 |
| `-c` | `--changes` | 只显示真正有改动的文件 |
| | `--reference=文件` | 照搬另一个文件的所有者和组 |

## 4. 常用示例

以下以 root 身份执行（相当于普通用户加 sudo）。

### 4.1 改所有者

```
$ ls -l readme.txt
-rw-r--r-- 1 tom tom 6 Oct  1 09:00 readme.txt

$ sudo chown root readme.txt

$ ls -l readme.txt
-rw-r--r-- 1 root tom 6 Oct  1 09:00 readme.txt
```

### 4.2 同时改所有者和组

```
$ sudo chown tom:www-data readme.txt

$ ls -l readme.txt
-rw-r--r-- 1 tom www-data 6 Oct  1 09:00 readme.txt
```

网站文件常设成“自己是所有者，www-data 是所属组”，你能编辑，网站进程（www-data）能读。

### 4.3 `-R`：把整个目录交给某个账号

```
$ sudo chown -R www-data:www-data src

$ ls -l src
total 4
-rw-r--r-- 1 www-data www-data 152 Oct 29 06:50 products.csv
```

部署时常用：`sudo chown -R appuser:appuser /opt/app`。

### 4.4 `-v`：确认结果

```
$ sudo chown -v tom:tom readme.txt
ownership of 'readme.txt' retained as tom:tom
```

`retained` 表示本来就是这样，没有改动。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `chown: changing ownership of 'readme.txt': Operation not permitted (os error 1)` | 普通用户不能把文件交给别人 | 加 `sudo` |
| `chown: invalid user: 'nobodyx'` | 用户不存在或拼错 | 用 `id 用户名` 确认 |
| 在 Ubuntu 26.04 上 `chown tom: 文件` 没有改所属组 | 传统写法 `用户:`（冒号后留空）表示“组改成该用户的登录组”。26.04 默认的 Rust 版 chown 只改了所有者，组保持不变 | 把组写全：`chown tom:tom 文件`。需要传统行为时可以用 GNU 版 `gnuchown` |
| 对 `/` 或 `/usr` 执行了 `chown -R`，系统多处出错 | 系统文件的所有者被改乱了 | 执行 -R 前再三确认路径；这类事故通常只能重装或从备份恢复 |

第三条的实测（两个文件原来都是 `www-data:www-data`）：

```
$ sudo chown tom: readme.txt; ls -l readme.txt
-rw-r--r-- 1 tom www-data 6 Oct  1 09:00 readme.txt

$ sudo gnuchown tom: run.sh; ls -l run.sh
-rwxr-xr-x 1 tom tom 20 Oct 29 06:50 run.sh
```

## 6. 一句话总结

`sudo chown -R 用户:组 目录` 把目录交给运行账号；在 26.04 上组名要写全。

---

[← 上一个：chmod](023-chmod.md) · [返回目录](../../README.md) · [下一个：df →](025-df.md)
