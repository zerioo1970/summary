# 099 killall：按程序名结束进程

> 级别：初级 · 软件包：`psmisc`

## 1. 作用

`killall` 按程序名结束进程，同名的进程会被**全部**结束。用 [kill](030-kill.md) 需要先查 PID，同一个程序开了很多个进程时，killall 一次就能处理完。

## 2. 基本格式

```bash
killall [参数] 程序名
```

程序名要和进程名完全一致（`ps` 的 CMD 列或 `pgrep -a` 看到的名字）。

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-信号` | 指定信号，如 `-9`、`-HUP`，默认 15（TERM） |
| `-i` | 每个进程都先询问 |
| `-v` | 显示结束了哪些进程 |
| `-u 用户` | 只结束某个用户的进程 |
| `-w` | 等进程真正退出后再返回 |

## 4. 常用示例

### 4.1 结束所有同名进程

```
$ sleep 600 & sleep 601 &

$ pgrep -a sleep
48 sleep 600
49 sleep 601

$ killall sleep

$ pgrep -a sleep; echo "剩余=$?"
剩余=1
```

两个 sleep 都被结束了。pgrep 找不到进程时退出码为 1。

### 4.2 `-v`：看结束了谁

```
$ killall -v sleep
Killed sleep(56) with signal 15
```

### 4.3 只结束某个用户的进程

```bash
sudo killall -u zhang python3
```

### 4.4 进程名不存在

```
$ killall nothere
nothere: no process found
```

## 5. killall 和 pkill 的区别

| | killall | pkill |
|---|---|---|
| 匹配方式 | 进程名必须完全一致 | 进程名包含关键词即可 |
| 按完整命令行匹配 | 不支持 | `pkill -f "关键词"` |

例如要结束 `java -jar price-service.jar`，killall 只能写 `killall java`（会把所有 Java 程序都结束），而 `pkill -f price-service` 可以精确地只结束这一个。

## 6. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `killall java` 把服务器上所有的 Java 服务都停了 | killall 会结束所有同名进程 | 先 `pgrep -a java` 看清楚有哪些，再用 `pkill -f 关键词` 或 `kill PID` |
| 在其他 Unix 系统（如 Solaris）上执行 killall，结束的是所有进程 | 不同系统上 killall 的含义不同 | 只在 Linux 上用 killall |

## 7. 一句话总结

`killall 程序名` 一次结束所有同名进程，执行前先 `pgrep -a` 确认范围。

---

[← 上一个：nproc](098-nproc.md) · [返回目录](../../README.md) · [下一个：xdg-open →](100-xdg-open.md)
