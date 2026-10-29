# 059 tree：树状显示目录结构

> 级别：初级 · 软件包：`tree`（不是所有安装方式都自带，没有就 `sudo apt install tree`）

## 1. 作用

`tree` 用树形图显示目录下的所有子目录和文件，层级关系一目了然。接手一个新项目、写文档说明目录结构、确认部署包里的文件齐不齐，都用它。

## 2. 基本格式

```bash
tree [参数] [目录]
```

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-L 层数` | 只显示到第几层 |
| `-d` | 只显示目录 |
| `-a` | 包括隐藏文件 |
| `-h` | 显示文件大小 |
| `-P "模式"` | 只显示匹配的文件 |
| `-I "模式"` | 排除匹配的文件或目录 |
| `--prune` | 不显示空目录（和 `-P` 配合） |

## 4. 常用示例

### 4.1 显示全部

```
$ tree
.
├── big.iso
├── link.txt -> readme.txt
├── logs
│   └── app.log
├── readme.txt
├── run.sh
└── src
    └── products.csv

3 directories, 6 files
```

### 4.2 `-L`：只看一层

```
$ tree -L 1
.
├── big.iso
├── link.txt -> readme.txt
├── logs
├── readme.txt
├── run.sh
└── src

3 directories, 4 files
```

大项目的目录有几千个文件，先用 `-L 2` 看个大概。

### 4.3 `-d`：只看目录

```
$ tree -d
.
├── logs
└── src

3 directories
```

### 4.4 `-h`：带大小

```
$ tree -h
[4.0K]  .
├── [2.4M]  big.iso
├── [  10]  link.txt -> readme.txt
├── [4.0K]  logs
│   └── [ 404]  app.log
├── [   6]  readme.txt
├── [  20]  run.sh
└── [4.0K]  src
    └── [ 152]  products.csv

3 directories, 6 files
```

### 4.5 `-P` 和 `-I`：筛选

```
$ tree -P "*.csv" --prune
.
└── src
    └── products.csv

2 directories, 1 file

$ tree -I "logs"
.
├── big.iso
├── link.txt -> readme.txt
├── readme.txt
├── run.sh
└── src
    └── products.csv

2 directories, 5 files
```

前端项目里常用 `tree -I "node_modules|dist"` 跳过依赖和构建产物，`|` 分隔多个模式。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `tree: command not found` | 系统没装 | `sudo apt install tree` |
| 在大目录下执行，刷屏几分钟 | 文件太多 | 加 `-L 2`，或用 `-I` 排除大目录 |
| 把输出贴到 Windows 记事本里，线条变成乱码 | 树形线条是 UTF-8 字符 | 加 `--charset=ascii`，线条改用 `|--` 显示 |

## 6. 一句话总结

`tree -L 2` 看项目结构，`-I` 排除大目录，`-d` 只看目录。

---

[← 上一个：vim](058-vim.md) · [返回目录](../../README.md) · [下一个：alias →](060-alias.md)
