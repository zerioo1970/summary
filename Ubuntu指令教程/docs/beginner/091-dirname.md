# 091 dirname：从路径里取目录

> 级别：初级 · 软件包：`rust-coreutils`（系统自带）

## 1. 作用

`dirname` 去掉路径中最后一部分，只留下所在的目录。它只做字符串处理，不检查目录是否真的存在。

## 2. 基本格式

```bash
dirname 路径...
```

## 3. 常用示例

### 3.1 取目录

```
$ dirname /home/tom/demo/logs/app.log
/home/tom/demo/logs

$ dirname app.log
.
```

路径里没有目录部分时，返回 `.`（当前目录）。

### 3.2 和 basename 一起拆路径

```
$ f=/var/log/nginx/access.log; echo "目录=$(dirname $f) 文件=$(basename $f)"
目录=/var/log/nginx 文件=access.log
```

### 3.3 脚本里找到“脚本自己所在的目录”

```bash
#!/bin/bash
SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
cat "$SCRIPT_DIR/config.ini"
```

`$0` 是脚本自己的路径。这样写，无论从哪个目录调用这个脚本，都能找到和它放在一起的 config.ini。

## 4. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 路径里有空格，dirname 结果被拆开了 | `$f` 没加引号，被 Shell 按空格拆分 | 写成 `dirname "$f"` |

## 5. 一句话总结

`dirname 路径` 取所在目录；脚本里用 `$(dirname "$0")` 定位脚本自身的目录。

---

[← 上一个：basename](090-basename.md) · [返回目录](../../README.md) · [下一个：stat →](092-stat.md)
