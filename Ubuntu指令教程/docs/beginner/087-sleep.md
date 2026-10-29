# 087 sleep：暂停一段时间

> 级别：初级 · 软件包：`rust-coreutils`（系统自带）

## 1. 作用

`sleep` 什么也不做，只是等待指定的时间后退出。在脚本里，它用来让两个步骤之间隔开一段时间，比如启动服务后等几秒再检查状态，或者每隔一分钟轮询一次。

## 2. 基本格式

```bash
sleep 时长[单位]
```

| 单位 | 含义 | 例子 |
|---|---|---|
| 不写或 `s` | 秒 | `sleep 5`、`sleep 0.5` |
| `m` | 分钟 | `sleep 2m` |
| `h` | 小时 | `sleep 1h` |
| `d` | 天 | `sleep 1d` |

可以用小数。

## 3. 常用示例

### 3.1 等待后再执行

```
$ sleep 0.5; echo done
done
```

### 3.2 启动服务后等它就绪

```bash
sudo systemctl restart myapp
sleep 5
curl -fsS http://localhost:8080/health && echo "启动成功"
```

### 3.3 简单的轮询

```bash
while ! curl -fsS http://localhost:8080/health >/dev/null; do
    echo "还没好，3 秒后重试"
    sleep 3
done
echo "服务已就绪"
```

## 4. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 用 `sleep 3600` 加死循环做定时任务，脚本被意外结束后就再也不执行了 | sleep 循环没有人看管 | 定时任务用 crontab 或 systemd 定时器（中级篇讲） |
| 固定 `sleep 5` 等服务启动，机器忙的时候 5 秒不够 | 启动时间不固定 | 改成 3.3 的轮询写法，并设置最大重试次数 |

## 5. 一句话总结

`sleep 秒数` 在脚本里等一会儿；等服务就绪用轮询，定时任务用 cron。

---

[← 上一个：fastfetch](086-fastfetch.md) · [返回目录](../../README.md) · [下一个：watch →](088-watch.md)
