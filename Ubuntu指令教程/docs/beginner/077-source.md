# 077 source：在当前 Shell 里执行脚本

> 级别：初级 · 来源：bash 内建命令

## 1. 作用

`source` 读取一个脚本文件，在**当前 Shell** 里逐行执行。脚本里设置的变量、定义的别名和函数、`cd` 切换的目录，执行完都会留在当前 Shell 里。

普通的 `bash 脚本.sh` 或 `./脚本.sh` 是启动一个新的子 Shell 来执行，执行完子 Shell 就退出了，它做的设置也随之消失。

最常见的用法：改完 `~/.bashrc` 后执行 `source ~/.bashrc`，不用重开终端就能生效。

## 2. 基本格式

```bash
source 文件
. 文件        # 点号是 source 的简写，效果完全相同
```

## 3. 常用示例

### 3.1 加载变量文件

演示文件 app.env 的内容：

```
APP_PORT=8080
export APP_NAME=price
```

```
$ bash app.env; echo "[$APP_PORT]"
[]

$ source app.env; echo "[$APP_PORT] [$APP_NAME]"
[8080] [price]
```

用 bash 执行，变量设置在子 Shell 里，当前 Shell 拿不到；用 source 执行，变量留在了当前 Shell。

### 3.2 脚本里的 cd

go.sh 只有一行 `cd /tmp`：

```
$ bash go.sh; pwd
/home/tom/demo

$ source go.sh; pwd
/tmp
```

### 3.3 让 .bashrc 的修改立即生效

```bash
source ~/.bashrc
```

### 3.4 激活 Python 虚拟环境

```bash
source .venv/bin/activate
```

activate 脚本要修改当前 Shell 的 PATH 和提示符，所以必须用 source 执行。

## 4. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| source 一个脚本后，终端窗口直接关闭了 | 脚本里有 `exit`，source 时它退出的是当前 Shell | 给 source 用的脚本里用 `return`，不用 exit |
| 用 `./activate` 执行，虚拟环境没有激活 | 这样是在子 Shell 里执行的 | 用 `source` |
| source 一个不信任的脚本后，环境被改乱了 | 脚本里的任何命令都直接作用于当前 Shell | 只 source 自己了解内容的文件 |

## 5. 一句话总结

要让脚本里的变量、cd 留在当前终端，就用 `source 文件`（或 `. 文件`）。
