# 053 passwd：修改密码

> 级别：初级 · 软件包：`passwd`

## 1. 作用

`passwd` 修改用户的登录密码。普通用户只能改自己的密码，并且要先输入旧密码；root（或加 sudo）可以直接重设任何用户的密码，不需要知道旧密码。

## 2. 基本格式

```bash
passwd                # 改自己的密码
sudo passwd 用户名    # 管理员重设别人的密码
```

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-S` | 查看密码状态 |
| `-l` | 锁定账号的密码（无法再用密码登录） |
| `-u` | 解锁 |
| `-e` | 让密码立即过期，用户下次登录时必须改密码 |
| `-d` | 删除密码（不推荐） |

## 4. 常用示例

### 4.1 改自己的密码

```
$ passwd
Changing password for tom.
Current password:
New password:
Retype new password:
passwd: password updated successfully
```

输入密码时屏幕上没有任何显示，输完按回车即可。

> ⚠️ 4.1 只验证了前两行：测试环境无法进行交互输入。后面几行为 Ubuntu 的标准提示。

### 4.2 `-S`：查看密码状态

以下以 root 身份执行（普通用户加 sudo）：

```
$ sudo passwd -S tom
tom P 2026-10-29 0 99999 7 -1

$ sudo passwd -S root
root L 2026-08-26 0 99999 7 -1
```

| 字段 | 含义 |
|---|---|
| 第 2 列 | `P` 已设密码，`L` 已锁定，`NP` 没有密码 |
| 第 3 列 | 最后一次修改密码的日期 |
| 后面 4 个数字 | 最短使用天数、最长有效天数（99999 表示永不过期）、提前几天提醒、过期后宽限天数 |

Ubuntu 默认 root 是 `L`（锁定），这就是为什么 Ubuntu 上用 sudo 而不用 root 登录。

### 4.3 新员工入职：重设密码并要求首次登录修改

```bash
sudo passwd zhang          # 设置一个临时密码
sudo passwd -e zhang       # 让它立即过期
```

### 4.4 员工离职：先锁定账号

```bash
sudo passwd -l zhang
```

锁定只是禁止密码登录。如果对方配置过 ssh 密钥，还要删除 `~/.ssh/authorized_keys` 或用 `sudo usermod -s /usr/sbin/nologin zhang` 禁止登录。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 输密码时屏幕没反应，以为卡住了 | passwd 输入密码时不显示任何字符 | 照常输入，输完回车 |
| 新密码被拒绝：`BAD PASSWORD: The password is shorter than 8 characters` 之类 | 系统的密码强度规则 | 用更长、更复杂的密码 |
| `passwd -l` 锁定后对方还能 ssh 登录 | 对方用的是 ssh 密钥，不走密码 | 同时处理 ssh 密钥，见 4.4 |

> ⚠️ 第二条的提示文字未实机验证，具体内容随密码强度模块而不同。

## 6. 一句话总结

`passwd` 改自己的，`sudo passwd 用户` 重设别人的，`-S` 查状态，`-l` 锁定。

---

[← 上一个：more](052-more.md) · [返回目录](../../README.md) · [下一个：su →](054-su.md)
