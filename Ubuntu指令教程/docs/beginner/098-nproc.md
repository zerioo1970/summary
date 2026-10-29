# 098 nproc：显示可用的 CPU 数量

> 级别：初级 · 软件包：`rust-coreutils`（系统自带）

## 1. 作用

`nproc` 只输出一个数字：当前进程可以使用的 CPU 数量。它最常用在脚本里，按 CPU 数量决定并行任务数，比如编译时的 `make -j$(nproc)`。

## 2. 基本格式

```bash
nproc [参数]
```

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `--all` | 显示系统安装的全部 CPU 数，不考虑限制 |

## 4. 常用示例

```
$ nproc
8

$ nproc --all
8
```

脚本里按 CPU 数并行：

```bash
make -j"$(nproc)"
xargs -P "$(nproc)" -I{} gzip {} < files.txt
```

## 5. nproc 和 nproc --all 什么时候不一样

在容器里，或者用 `taskset` 限制了程序只能使用某几个 CPU 时，`nproc` 返回的是允许使用的数量，`--all` 返回的是整台机器的数量。给程序设置线程数时，应该用 `nproc` 的结果。

```
$ taskset -c 0,1 nproc
2
```

`taskset -c 0,1` 让 nproc 只能使用 0 号和 1 号 CPU，所以它报告的是 2，而不是整机的 8。

## 6. 一句话总结

脚本里要知道有几个 CPU 就用 `$(nproc)`。

---

[← 上一个：lspci](097-lspci.md) · [返回目录](../../README.md) · [下一个：killall →](099-killall.md)
