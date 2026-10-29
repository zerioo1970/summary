# 078 diff：比较两个文件的差异

> 级别：初级 · 软件包：`diffutils`

## 1. 作用

`diff` 逐行比较两个文件，列出哪些行不同。改配置之前备份了一份，改完后用 diff 和备份对比，就能确认到底改了什么；两台服务器的配置不一致，也用它找差别。

## 2. 基本格式

```bash
diff [参数] 文件1 文件2
diff -r [参数] 目录1 目录2
```

## 3. 常用参数

| 参数 | 长格式 | 作用 |
|---|---|---|
| `-u` | `--unified` | 统一格式，前面带 `-`/`+`，最易读，git 也用这种格式 |
| `-y` | `--side-by-side` | 左右并排显示 |
| `-W 宽度` | `--width` | 并排显示时的总宽度 |
| `-q` | `--brief` | 只说是否不同，不列出细节 |
| `-r` | `--recursive` | 比较两个目录 |
| `-i` | `--ignore-case` | 忽略大小写 |
| `-w` | `--ignore-all-space` | 忽略所有空白的差异 |
| `-B` | `--ignore-blank-lines` | 忽略空行 |

## 4. 常用示例

演示用的两个配置文件：

| a.conf | b.conf |
|---|---|
| `host=db01` | `host=db02` |
| `port=1433` | `port=1433` |
| `user=sa` | `user=app` |
| | `timeout=30` |

### 4.1 `-u`：推荐的格式

```
$ diff -u a.conf b.conf
--- a.conf      2026-10-29 07:03:59.000975292 +0000
+++ b.conf      2026-10-29 07:03:59.000975292 +0000
@@ -1,3 +1,4 @@
-host=db01
+host=db02
 port=1433
-user=sa
+user=app
+timeout=30
```

`-` 开头的行只在 a.conf 里，`+` 开头的只在 b.conf 里，空格开头的两边一样。`@@ -1,3 +1,4 @@` 表示这一段对应 a.conf 的第 1 行起共 3 行、b.conf 的第 1 行起共 4 行。

### 4.2 默认格式

```
$ diff a.conf b.conf
1c1
< host=db01
---
> host=db02
3c3,4
< user=sa
---
> user=app
> timeout=30
```

`1c1` 表示第 1 行被修改（c = change），`a` 是新增（add），`d` 是删除（delete）。`<` 是第一个文件的内容，`>` 是第二个文件的。

### 4.3 `-y`：并排对比

```
$ diff -y -W 40 a.conf b.conf
host=db01          |    host=db02
port=1433               port=1433
user=sa            |    user=app
                   >    timeout=30
```

`|` 表示这一行两边不同，`>` 表示只有右边有。

### 4.4 `-q`：只问是否相同

```
$ diff -q a.conf b.conf
Files a.conf and b.conf differ

$ diff a.conf a.conf; echo "退出码=$?"
退出码=0
```

完全相同时没有任何输出，退出码为 0；有差异时退出码为 1。脚本里可以据此判断配置有没有被改过。

### 4.5 `-r`：比较两个目录

```
$ diff -q -r d1 d2
Files d1/a.conf and d2/a.conf differ
Only in d2: new.txt
```

升级程序前后，对比两个版本的配置目录，很快就能看出新增了哪些文件、哪些文件有改动。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 两个文件看起来一模一样，diff 却说每一行都不同 | 一个是 Windows 换行（`\r\n`），一个是 Linux 换行 | 用 `cat -A` 确认；`diff --strip-trailing-cr` 可以忽略这个差异 |
| 只是缩进从 Tab 改成了空格，diff 报了一大片 | 空白字符也算差异 | 加 `-w` 忽略空白 |

## 6. 一句话总结

对比配置用 `diff -u 旧 新`，`-` 是删掉的，`+` 是新加的；比目录加 `-r`。

---

[← 上一个：source](077-source.md) · [返回目录](../../README.md) · [下一个：cut →](079-cut.md)
