# 100 xdg-open：用默认程序打开文件或网址

> 级别：初级 · 软件包：`xdg-utils`（Ubuntu 26.04 为 1.2.1 版）

## 1. 作用

`xdg-open` 用系统默认的程序打开文件或网址：PDF 用文档查看器打开，图片用图片查看器，网址用默认浏览器，目录用文件管理器。效果和在桌面上双击文件一样。

它只在 **Ubuntu 桌面版**（有图形界面）上有用。服务器没有图形界面，xdg-open 打不开任何东西。

## 2. 基本格式

```bash
xdg-open 文件、目录或网址
```

一次只能打开一个。

## 3. 常用示例

```bash
xdg-open report.pdf            # 用文档查看器打开 PDF
xdg-open .                     # 在文件管理器里打开当前目录
xdg-open https://ubuntu.com    # 用默认浏览器打开网址
```

在终端里处理完一批文件，想用图形界面看一眼结果时，`xdg-open .` 最方便。

> ⚠️ 本节未实机验证：测试环境没有图形界面。

## 4. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 在服务器或 ssh 会话里报 `xdg-open: no method available for opening 'readme.txt'` | 没有图形界面，找不到可以打开文件的程序 | 文本文件用 `less`；其他文件下载到本机再打开 |
| 打开的程序不是你想要的 | 默认程序设置不同 | 在桌面的“设置 → 应用 → 默认应用”里修改；或直接用程序名打开，如 `evince report.pdf` |

第一条的实测：

```
$ xdg-open readme.txt
/usr/bin/xdg-open: 1032: www-browser: not found
/usr/bin/xdg-open: 1032: links2: not found
/usr/bin/xdg-open: 1032: elinks: not found
/usr/bin/xdg-open: 1032: links: not found
/usr/bin/xdg-open: 1032: lynx: not found
/usr/bin/xdg-open: 1032: w3m: not found
xdg-open: no method available for opening 'readme.txt'
```

没有图形界面时，xdg-open 会依次尝试几个终端里的文字浏览器，都没装就报错。

## 5. 一句话总结

桌面版上 `xdg-open 文件` 相当于双击打开，服务器上用不了。

---

[← 上一个：killall](099-killall.md) · [返回目录](../../README.md)
