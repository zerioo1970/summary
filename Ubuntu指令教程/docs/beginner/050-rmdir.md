# 050 rmdir：删除空目录

> 级别：初级 · 软件包：`rust-coreutils`（系统自带）

## 1. 作用

`rmdir`（remove directory）只删除**空**目录。目录里还有任何文件时它会拒绝执行，所以比 `rm -r` 安全：清理目录结构时用它，不用担心误删里面的文件。

## 2. 基本格式

```bash
rmdir [参数] 目录...
```

## 3. 常用参数

| 参数 | 长格式 | 作用 |
|---|---|---|
| `-p` | `--parents` | 连同上级目录一起删除（上级也必须变成空的） |
| `-v` | `--verbose` | 显示处理过程 |

## 4. 常用示例

### 4.1 删除空目录

```
$ mkdir empty; rmdir empty
```

### 4.2 目录不空时拒绝删除

```
$ rmdir logs
rmdir: failed to remove 'logs': Directory not empty
```

确认里面的东西都不要了，才改用 `rm -r logs`。

### 4.3 `-p`：一次删掉多层空目录

```
$ mkdir -p a/b/c; rmdir -p a/b/c; ls
big.iso  link.txt  logs  n.txt  readme.txt  run.sh  src
```

依次删除 a/b/c、a/b、a。

### 4.4 批量清理空目录

```bash
find . -type d -empty -delete
```

rmdir 只处理你点名的目录。要把整个目录树里所有空目录都清掉，用 find。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 目录看起来是空的，rmdir 仍报 `Directory not empty` | 里面有隐藏文件 | `ls -A 目录` 查看 |

## 6. 一句话总结

只删空目录用 `rmdir`，删不掉就说明里面还有东西。

---

[← 上一个：uniq](049-uniq.md) · [返回目录](../../README.md) · [下一个：ln →](051-ln.md)
