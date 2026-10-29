# 049 uniq：去重和计数

> 级别：初级 · 软件包：`rust-coreutils`（系统自带）

## 1. 作用

`uniq` 去掉**相邻的**重复行，加 `-c` 可以统计每一行出现了几次。它几乎总是和 `sort` 一起用：先排序让相同的行挨在一起，再交给 uniq。`sort | uniq -c` 相当于 SQL 的 `GROUP BY ... COUNT(*)`。

## 2. 基本格式

```bash
sort 文件 | uniq [参数]
```

## 3. 常用参数

| 参数 | 长格式 | 作用 |
|---|---|---|
| `-c` | `--count` | 每行前面加上出现次数 |
| `-d` | `--repeated` | 只显示有重复的行 |
| `-u` | `--unique` | 只显示没有重复的行 |
| `-i` | `--ignore-case` | 比较时忽略大小写 |

## 4. 常用示例

### 4.1 为什么要先 sort

```
$ printf "b\na\nb\n" | uniq
b
a
b

$ printf "b\na\nb\n" | sort | uniq
a
b
```

两个 b 不相邻，uniq 不会去重。

### 4.2 `-c`：统计每种日志级别各有多少条

```
$ awk '{print $3}' logs/app.log | sort | uniq -c
      2 ERROR
      5 INFO
      1 WARN
```

`awk '{print $3}'` 取出每行的第 3 列（日志级别），见中级篇 awk。

### 4.3 统计并按次数排序

```
$ cut -d, -f2 src/products.csv | tail -n +2 | sort | uniq -c | sort -nr
      2 探头A
      1 探头B
      1 凝胶
      1 主机X
```

这一串命令是：取第 2 列（品名）→ 去掉表头 → 排序 → 计数 → 按次数从高到低排。分析访问日志里“哪个 IP 访问最多”，用的也是这个套路：

```bash
awk '{print $1}' access.log | sort | uniq -c | sort -nr | head
```

### 4.4 `-d` 和 `-u`：找重复、找唯一

```
$ cut -d, -f2 src/products.csv | tail -n +2 | sort | uniq -d
探头A

$ cut -d, -f2 src/products.csv | tail -n +2 | sort | uniq -u
主机X
凝胶
探头B
```

导入数据前用 `uniq -d` 查一下编码列有没有重复。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| uniq 之后还有重复行 | 重复的行不相邻 | 先 `sort` 再 `uniq` |
| 看起来一样的两行没被合并 | 其中一行末尾多了空格，或者有 Windows 换行符 `\r` | 用 `cat -A` 检查不可见字符 |

## 6. 一句话总结

`sort | uniq -c | sort -nr` 是命令行里的“分组计数并排序”。
