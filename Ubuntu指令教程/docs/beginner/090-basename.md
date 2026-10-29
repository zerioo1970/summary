# 090 basename：从路径里取文件名

> 级别：初级 · 软件包：`rust-coreutils`（系统自带）

## 1. 作用

`basename` 去掉路径中的目录部分，只留下最后的文件名；还可以顺便去掉扩展名。主要在脚本里用：批量处理文件时，拼出新的文件名。

和它对应的是 [dirname](091-dirname.md)，只取目录部分。

## 2. 基本格式

```bash
basename 路径 [要去掉的后缀]
basename -s 后缀 路径...
```

## 3. 常用参数

| 参数 | 长格式 | 作用 |
|---|---|---|
| `-s 后缀` | `--suffix` | 去掉指定后缀 |
| `-a` | `--multiple` | 一次处理多个路径 |

## 4. 常用示例

### 4.1 取文件名

```
$ basename /home/tom/demo/logs/app.log
app.log
```

### 4.2 顺便去掉扩展名

```
$ basename /home/tom/demo/logs/app.log .log
app

$ basename -s .csv src/products.csv
products
```

### 4.3 脚本里批量改名备份

```
$ for f in logs/*.log; do cp "$f" "/tmp/$(basename "$f" .log)-$(date +%Y%m%d).log"; done; ls /tmp
app-20261029.log
```

logs/app.log 被复制成了 /tmp/app-20261029.log。

### 4.4 bash 自带的写法

```
$ f=/var/log/nginx/access.log; echo "${f##*/} ${f%/*}"
access.log /var/log/nginx
```

`${f##*/}` 等于 basename，`${f%/*}` 等于 dirname。在循环里处理大量文件时，这种写法不用启动新进程，速度更快。

## 5. 一句话总结

`basename 路径 .扩展名` 取不带扩展名的文件名，批量改名时常用。

---

[← 上一个：time](089-time.md) · [返回目录](../../README.md) · [下一个：dirname →](091-dirname.md)
