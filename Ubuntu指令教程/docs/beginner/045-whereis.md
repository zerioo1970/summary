# 045 whereis：查程序、手册和源码的位置

> 级别：初级 · 软件包：`util-linux`

## 1. 作用

`whereis` 在系统的标准目录里查找一个命令的程序文件、手册文件和源码文件。和 which 相比，它不依赖 PATH，而是直接到 `/usr/bin`、`/usr/sbin`、`/usr/share/man` 这些固定位置去找，所以能找到不在 PATH 里的程序。

## 2. 基本格式

```bash
whereis [参数] 名称...
```

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-b` | 只查程序文件 |
| `-m` | 只查手册文件 |
| `-s` | 只查源码文件 |

## 4. 常用示例

```
$ whereis ls
ls: /usr/bin/ls

$ whereis bash
bash: /usr/bin/bash /usr/share/man/man1/bash.1.gz

$ whereis -b nginx
nginx:
```

bash 的结果里有两项：程序 `/usr/bin/bash` 和手册 `bash.1.gz`。nginx 后面是空的，说明系统里没有装。

## 5. 和 which 的区别

| | which | whereis |
|---|---|---|
| 查找范围 | PATH 里的目录 | 系统固定的标准目录 |
| 能找到手册 | 不能 | 能 |
| 回答的问题 | “我执行这个命令时，运行的是哪个文件” | “这个东西装在系统哪里” |

日常确认“当前用的是哪个版本”用 which；确认“系统里装没装、装在哪”用 whereis。

## 6. 一句话总结

`whereis 名称` 查程序和手册装在哪，后面为空就是没装。
