# 017 exit：退出 Shell 或脚本

> 级别：初级 · 来源：bash 内建命令

## 1. 作用

`exit` 结束当前的 Shell。具体效果看你在哪里执行：

| 场景 | 效果 |
|---|---|
| 本地终端窗口 | 关闭这个终端会话 |
| ssh 登录的远程服务器 | 断开连接，回到本机 |
| `sudo -i` 或 `su` 切换出来的 root | 退回到原来的普通用户 |
| 脚本里 | 结束脚本，并返回一个退出码 |

快捷键 `Ctrl+D` 在空的命令行上按下，效果和 exit 相同。

## 2. 基本格式

```bash
exit [退出码]
```

退出码是 0 到 255 的整数。0 表示成功，非 0 表示失败。不写时，使用最后一条命令的退出码。

## 3. 常用示例

### 3.1 脚本里返回失败

```
$ bash -c "exit 3"; echo $?
3
```

脚本里写法：

```bash
#!/bin/bash
if [ ! -f /etc/app.conf ]; then
    echo "配置文件不存在"
    exit 1        # 返回 1，调用方（cron、CI）就知道这一步失败了
fi
```

### 3.2 不写退出码

```
$ bash -c "ls nothere; exit"; echo $?
ls: cannot access 'nothere': No such file or directory
2

$ bash -c "exit"; echo $?
0
```

第一个例子，exit 前最后一条命令 ls 失败了（退出码 2），所以脚本也返回 2。

## 4. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 用 `source 脚本` 执行，脚本里的 exit 把整个终端关了 | source 让脚本在当前 Shell 里运行，exit 退出的就是当前 Shell | 被 source 的脚本里用 `return` 代替 exit |
| 在 `sudo -i` 里执行 exit，以为断开了服务器，其实只是退回了普通用户 | 每层 Shell 要各自 exit | 看提示符：`#` 是 root，`$` 是普通用户；主机名变回本机才是真正断开 |

## 5. 一句话总结

`exit` 退出当前一层 Shell；脚本里用 `exit 1` 告诉调用方失败了。
