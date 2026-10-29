# 057 lsb_release：查看 Ubuntu 版本

> 级别：初级 · 软件包：`lsb-release`

## 1. 作用

`lsb_release` 显示发行版的名称、版本号和代号。网上的安装教程、第三方软件源往往按 Ubuntu 版本区分（24.04、26.04），装之前先用它确认自己的版本。

## 2. 基本格式

```bash
lsb_release [参数]
```

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-a` | 显示全部信息 |
| `-r` | 版本号 |
| `-c` | 代号 |
| `-d` | 完整描述 |
| `-s` | 只输出值，不输出标题（和其他参数组合使用） |

## 4. 常用示例

```
$ lsb_release -a
Distributor ID: Ubuntu
Description:    Ubuntu 26.04.1 LTS
Release:        26.04
Codename:       resolute
```

| 字段 | 含义 |
|---|---|
| Description | `26.04.1` 是 26.04 的第一个小版本更新 |
| LTS | 长期支持版（Long Term Support），标准支持 5 年 |
| Codename | 代号 resolute（Resolute Raccoon）。软件源地址里用的就是它 |

脚本里只取值：

```
$ lsb_release -cs
resolute

$ lsb_release -rs
26.04
```

很多第三方软件源的安装说明里会出现 `$(lsb_release -cs)`，就是为了自动填入代号。

## 5. 另一种方法：/etc/os-release

```
$ cat /etc/os-release
PRETTY_NAME="Ubuntu 26.04.1 LTS"
NAME="Ubuntu"
VERSION_ID="26.04"
VERSION="26.04.1 LTS (Resolute Raccoon)"
VERSION_CODENAME=resolute
ID=ubuntu
ID_LIKE=debian
...
```

`/etc/os-release` 所有现代 Linux 发行版都有，lsb_release 在部分精简系统里可能没装。

## 6. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 按网上教程添加的软件源报 404 | 教程是给其他版本写的，代号不对 | 用 `lsb_release -cs` 确认代号，换成对应版本的源 |
| 想看内核版本，却用了 lsb_release | lsb_release 只显示发行版版本 | 内核版本用 `uname -r` |

## 7. 一句话总结

`lsb_release -a` 看 Ubuntu 版本，脚本里用 `-cs` 取代号。

---

[← 上一个：uptime](056-uptime.md) · [返回目录](../../README.md) · [下一个：vim →](058-vim.md)
