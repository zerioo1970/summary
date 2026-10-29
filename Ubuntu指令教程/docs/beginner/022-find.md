# 022 find：按条件查找文件

> 级别：初级 · 软件包：`findutils`（GNU 版）

## 1. 作用

`find` 从指定目录开始，逐层往下找出符合条件的文件和目录。条件可以是文件名、类型、大小、修改时间、所有者，还可以组合使用。找到之后还能直接对它们执行操作，比如删除 30 天前的日志。

grep 是在文件**内容**里找，find 是按文件**名字和属性**找。

## 2. 基本格式

```bash
find [起始目录] [条件...] [动作]
```

不写起始目录时从当前目录开始。不写动作时，默认打印找到的路径。

## 3. 常用参数

| 条件 / 动作 | 作用 |
|---|---|
| `-name "模式"` | 按文件名匹配，区分大小写，可用 `*` `?` 通配符 |
| `-iname "模式"` | 按文件名匹配，不区分大小写 |
| `-type f` / `-type d` | 只找普通文件 / 只找目录 |
| `-size +1M` | 大于 1 MB（`-1M` 小于，单位可用 `k` `M` `G`） |
| `-mtime -1` | 1 天内修改过（`+7` 表示 7 天以前修改的） |
| `-mmin -30` | 30 分钟内修改过 |
| `-newer 文件` | 比某个文件更新 |
| `-user 用户` | 属于某个用户 |
| `-empty` | 空文件或空目录 |
| `-maxdepth N` | 最多往下找 N 层 |
| `-exec 命令 {} \;` | 对每个结果执行命令，`{}` 代表找到的文件 |
| `-delete` | 删除找到的文件 |

## 4. 常用示例

### 4.1 按名字找

```
$ find . -name "*.log"
./logs/app.log

$ find . -name "*.LOG"

$ find . -iname "*.LOG"
./logs/app.log
```

### 4.2 按类型找

```
$ find . -type d
.
./src
./logs

$ find . -type f -name "*.csv"
./src/products.csv
```

### 4.3 按大小找：磁盘满了，找大文件

```
$ find . -size +1M
./big.iso
```

整台机器上找大于 1 GB 的文件：`sudo find / -type f -size +1G 2>/dev/null`。

### 4.4 按时间找

```
$ find . -mtime +7
./big.iso
./readme.txt
```

big.iso（Oct 20）和 readme.txt（Oct 1）都是 7 天前修改的。`-mtime -1` 找 24 小时内改过的文件，排查“昨天谁动了配置”时用。

### 4.5 限制深度

```
$ find . -maxdepth 1 -type f
./run.sh
./.env
./big.iso
./readme.txt
```

只看当前这一层，不进子目录。

### 4.6 `-exec`：对结果执行命令

```
$ find . -name "*.log" -exec ls -l {} \;
-rw-r--r-- 1 tom tom 404 Oct 28 11:30 ./logs/app.log
```

`{}` 会被替换成每个找到的文件，`\;` 表示命令到此结束（分号前的反斜杠不能省）。

### 4.7 清理旧日志

```bash
find /var/log/app -name "*.log" -mtime +30 -print     # 先看看会删哪些
find /var/log/app -name "*.log" -mtime +30 -delete    # 确认无误再删
```

### 4.8 屏蔽权限报错

```
$ find /etc -name "hosts"
/etc/hosts
find: ‘/etc/ssl/private’: Permission denied

$ find /etc -name "hosts" 2>/dev/null
/etc/hosts
```

`2>/dev/null` 把错误信息丢掉，只留结果。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `find: paths must precede expression` | `-name *.txt` 没加引号，`*.txt` 被 Shell 先展开成了多个文件名 | 模式一律加引号：`-name "*.txt"` |
| `-delete` 写在前面，把所有文件都删了 | find 按顺序处理，`find . -delete -name "*.tmp"` 会先删再判断名字 | `-delete` 必须放在最后；先用 `-print` 预览 |
| 搜索整个系统非常慢，还刷出一堆 `Permission denied` | 从 `/` 开始会遍历所有目录，包括 `/proc` | 缩小起始目录；加 `2>/dev/null`；只按文件名找可以用更快的 [locate](093-locate.md) |
| `-mtime +1` 没找到昨天的文件 | `-mtime` 按“整 24 小时”计算，`+1` 表示超过 48 小时 | 按分钟算更直观：`-mmin +1440` |

第一条的实测：

```
$ find . -name *.txt
find: paths must precede expression: `readme.txt'
find: possible unquoted pattern after predicate `-name'?
```

## 6. 一句话总结

`find 目录 -name "模式" -type f` 按名字找；删除前先 `-print` 预览，`-delete` 放最后。

---

[← 上一个：grep](021-grep.md) · [返回目录](../../README.md) · [下一个：chmod →](023-chmod.md)
