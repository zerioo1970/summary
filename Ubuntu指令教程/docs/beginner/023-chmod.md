# 023 chmod：修改文件权限

> 级别：初级 · 软件包：`rust-coreutils`（系统自带）

## 1. 作用

`chmod`（change mode）修改文件或目录的权限：谁可以读、谁可以写、谁可以执行。脚本不能运行、网站读不到文件、ssh 密钥被拒绝使用，很多时候都是权限问题。

## 2. 权限的两种写法

`ls -l` 里的 `rwxr-xr-x` 分三组：所有者（u）、所属组（g）、其他人（o）。每组三位，`r` 读、`w` 写、`x` 执行。

**数字写法**：r=4、w=2、x=1，每组相加得到一位数字。

| 数字 | 权限 | 含义 |
|---|---|---|
| 7 | rwx | 4+2+1，读写执行 |
| 6 | rw- | 4+2，读写 |
| 5 | r-x | 4+1，读和执行 |
| 4 | r-- | 只读 |
| 0 | --- | 无权限 |

所以 `755` = `rwxr-xr-x`，`644` = `rw-r--r--`，`600` = `rw-------`。

**符号写法**：`谁 操作 权限`。

| 部分 | 可选值 |
|---|---|
| 谁 | `u` 所有者，`g` 所属组，`o` 其他人，`a` 所有人 |
| 操作 | `+` 增加，`-` 去掉，`=` 设为 |
| 权限 | `r` `w` `x` |

例如 `u+x`（给所有者加执行权限）、`go-w`（去掉组和其他人的写权限）。

## 3. 基本格式和常用参数

```bash
chmod [参数] 权限 文件...
```

| 参数 | 长格式 | 作用 |
|---|---|---|
| `-R` | `--recursive` | 递归修改目录及其中所有内容 |
| `-v` | `--verbose` | 显示每个文件改动前后的权限 |
| `-c` | `--changes` | 只显示真正有改动的文件 |

## 4. 常用示例

### 4.1 `+x`：让脚本可以执行

```
$ chmod +x readme.txt; ls -l readme.txt
-rwxr-xr-x 1 tom tom 6 Oct  1 09:00 readme.txt

$ chmod -x readme.txt; ls -l readme.txt
-rw-r--r-- 1 tom tom 6 Oct  1 09:00 readme.txt
```

写好的 `deploy.sh` 要先 `chmod +x deploy.sh` 才能用 `./deploy.sh` 运行。

### 4.2 数字写法：常见的几个组合

```
$ chmod 755 run.sh; ls -l run.sh
-rwxr-xr-x 1 tom tom 20 Oct 29 06:37 run.sh

$ chmod 600 .env; ls -l .env
-rw------- 1 tom tom 12 Oct 29 06:37 .env
```

| 权限 | 用在哪里 |
|---|---|
| `755` | 脚本、程序、目录 |
| `644` | 普通文件、网页文件、大部分配置文件 |
| `600` | 只有自己能读写：密码文件、`.env`、ssh 私钥 |
| `700` | 只有自己能进入的目录：`~/.ssh` |

### 4.3 符号写法：精确设置

```
$ chmod u=rw,g=r,o= readme.txt; ls -l readme.txt
-rw-r----- 1 tom tom 6 Oct  1 09:00 readme.txt
```

### 4.4 `-v`：看清楚改了什么

```
$ chmod -v 644 readme.txt
mode of 'readme.txt' changed from 0640 (rw-r-----) to 0644 (rw-r--r--)
```

### 4.5 `-R`：递归修改

```
$ chmod -R 750 src; ls -ld src src/products.csv
drwxr-x--- 2 tom tom 4096 Oct 29 06:37 src
-rwxr-x--- 1 tom tom  152 Oct 29 06:37 src/products.csv
```

注意 products.csv 也被加上了执行权限，这通常不是你想要的，见下方踩坑表。

## 5. 目录的 x 权限

对目录来说，三个权限的含义和文件不同：

| 权限 | 对目录的含义 |
|---|---|
| `r` | 可以列出目录里有哪些文件（ls） |
| `w` | 可以在目录里新建、删除、改名文件 |
| `x` | 可以进入目录（cd），访问里面的文件 |

目录没有 `x`，即使里面的文件是 644，别人也读不到。

## 6. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `./readme.txt` 报 `Permission denied` | 文件没有执行权限 | `chmod +x 文件` |
| `chmod -R 755` 之后所有文件都变成了可执行 | -R 对文件和目录一视同仁 | 分开设置：`find 目录 -type d -exec chmod 755 {} +` 和 `find 目录 -type f -exec chmod 644 {} +` |
| 网站报 403，文件明明是 644 | 上级某个目录缺少 `x` 权限，网站进程进不去 | 用 `namei -l 文件完整路径` 查看路径上每一级目录的权限 |
| 为了省事 `chmod 777`，问题“解决”了 | 777 让任何用户都能修改，是安全隐患 | 找出真正需要访问的用户，用 chown 改所有者或所属组 |
| ssh 报 `UNPROTECTED PRIVATE KEY FILE!` 并拒绝使用私钥 | 私钥权限太宽 | `chmod 600 ~/.ssh/id_ed25519` |
| `chmod 999 文件` 报 `chmod: invalid digit found in string` | 每一位只能是 0–7 | 按第 2 节的表重新计算 |

## 7. 一句话总结

脚本加 `+x`，普通文件 644，目录 755，私密文件 600，不要用 777。
