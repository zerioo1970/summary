# 030 kill：结束进程

> 级别：初级 · 来源：bash 内建命令（另有同名程序 `/usr/bin/kill`，用法相同）

## 1. 作用

`kill` 向进程发送信号（signal，系统通知进程的一种方式）。最常见的用途是结束进程：程序卡死、端口被旧进程占用、要停掉一个手动启动的后台任务时用它。

kill 默认发送的是 TERM（15 号）信号，意思是“请你退出”，程序可以先保存数据、关闭连接再退出。只有程序不理会时，才用 KILL（9 号）信号强制结束。

## 2. 基本格式

```bash
kill [-信号] PID...
kill %作业号            # 结束当前终端的后台作业
```

PID 用 [ps](029-ps.md) 或 `pgrep` 查。

## 3. 常用信号

| 信号 | 编号 | 写法 | 作用 |
|---|---|---|---|
| TERM | 15 | `kill PID` | 默认信号，请求进程正常退出 |
| KILL | 9 | `kill -9 PID` | 强制结束，进程无法拒绝，也来不及做清理 |
| HUP | 1 | `kill -HUP PID` | 很多服务（如 nginx）收到后会重新读取配置 |
| INT | 2 | `kill -INT PID` | 等同于在终端里按 `Ctrl+C` |

```
$ kill -l 9 15
KILL
TERM
```

`kill -l` 列出所有信号，后面跟编号则显示对应的名字。

## 4. 常用示例

### 4.1 结束后台作业

```
$ sleep 600 &
[1] 197

$ jobs
[1]+  Running                    sleep 600 &

$ kill %1

$ jobs
[1]+  Terminated                 sleep 600
```

### 4.2 按 PID 结束

```
$ pgrep -a sleep
199 sleep 601

$ kill 199
```

也可以写成一行：`kill $(pgrep -x sleep)`。

### 4.3 正常结束不了，再用 `-9`

下面模拟一个忽略 TERM 信号的进程：

```
$ bash -c 'trap "" TERM; sleep 602' &
[2] 203

$ kill 203; sleep 0.5; ps -p 203 -o pid,stat,cmd
    PID STAT CMD
    203 S    sleep 602

$ kill -9 203; sleep 0.3; ps -p 203 -o pid,cmd
    PID CMD
```

第一次 kill 后进程还在；`kill -9` 之后，ps 已经查不到它了。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `bash: kill: (99999) - No such process` | PID 写错，或进程已经退出 | 重新用 `pgrep` 查 |
| `bash: kill: (1) - Operation not permitted` | 不能结束其他用户（包括 root）的进程 | 确认确实要结束后，用 `sudo kill PID` |
| 一上来就 `kill -9` 数据库，重启后数据损坏或要长时间恢复 | `-9` 不给程序保存数据的机会 | 先 `kill PID` 等几秒，不行再 `-9`；systemd 管理的服务用 `sudo systemctl stop 服务名` |
| kill 掉进程，几秒后它又出现了 | 进程由 systemd 或其他守护程序管理，退出后会被自动拉起 | 用 `systemctl stop` 停止服务 |
| 想按名字结束进程 | kill 只认 PID | 用 `pkill 名字` 或 [killall](099-killall.md) |

## 6. 一句话总结

先 `kill PID` 礼貌请求，没反应再 `kill -9`；服务要用 `systemctl stop`。
