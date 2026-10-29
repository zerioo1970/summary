# 040 whoami：显示当前用户名

> 级别：初级 · 软件包：`rust-coreutils`（系统自带）

## 1. 作用

`whoami` 打印当前是以哪个用户身份在操作。用 `su`、`sudo -i` 切换过身份，或者在脚本里要确认“我现在是不是 root”时用它。

## 2. 基本格式

```bash
whoami
```

没有常用参数。

## 3. 常用示例

### 3.1 查看当前用户

```
$ whoami
tom
```

`id -un` 输出的结果相同：

```
$ id -un
tom
```

### 3.2 脚本里要求必须用 root 运行

```bash
#!/bin/bash
if [ "$(whoami)" != "root" ]; then
    echo "请用 sudo 运行本脚本"
    exit 1
fi
```

## 4. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `sudo whoami` 输出 root，`whoami` 输出 tom | sudo 让这一条命令以 root 身份执行 | 这是正常的。想知道“是谁用 sudo 提的权”，在 sudo 下的程序里查环境变量 `$SUDO_USER` |
| 用 `whoami` 判断权限，结果被普通用户改了 PATH 绕过 | PATH 里放了一个假的 whoami | 判断是否 root 更可靠的写法是 `[ "$(id -u)" -eq 0 ]`（root 的 UID 固定为 0） |

## 5. 一句话总结

`whoami` 看当前身份，脚本里判断 root 用 `id -u` 等于 0。

---

[← 上一个：shutdown](039-shutdown.md) · [返回目录](../../README.md) · [下一个：uname →](041-uname.md)
