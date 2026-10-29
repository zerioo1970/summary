# 064 dpkg：安装和查询 .deb 包

> 级别：初级 · 软件包：`dpkg`

## 1. 作用

`dpkg` 是 Ubuntu 最底层的软件包工具，apt 在下载完安装包之后，实际就是调用 dpkg 来安装的。日常用 dpkg 做两类事：

- 安装从网上下载的单个 `.deb` 文件（比如某些厂商提供的客户端）；
- 查询：装了哪些包、某个包装了哪些文件、某个文件属于哪个包。

dpkg 不会自动下载依赖，这是它和 apt 最大的区别。

## 2. 基本格式

```bash
sudo dpkg -i 文件.deb       # 安装
dpkg -l [包名]              # 查询
```

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-i 文件.deb` | 安装 |
| `-r 包名` | 卸载，保留配置 |
| `-P 包名` | 卸载并删除配置 |
| `-l [模式]` | 列出已安装的包 |
| `-s 包名` | 查看包的状态和详情 |
| `-L 包名` | 列出这个包装了哪些文件 |
| `-S 文件路径` | 查这个文件属于哪个包 |
| `-c 文件.deb` | 查看 .deb 文件里有什么（不安装） |

## 4. 常用示例

以下以 root 身份执行；只查询的命令普通用户也能执行。

### 4.1 `-l`：查是否装了某个包

```
$ dpkg -l tree htop
+-- Desired=Unknown/Install/Remove/Purge/Hold
|+- Status=Not/Inst/Conf-files/Unpacked/halF-conf/Half-inst/trig-aWait/Trig-pend
||+ Err?=(none)/Reinst-required (Status,Err: uppercase=bad)
||| Name           Version       Architecture Description
+++-==============-=============-============-=============================================
ii  htop           3.4.1-5build2 amd64        interactive processes viewer
ii  tree           2.3.1-1       amd64        displays an indented directory tree, in color
```

第一列的 `ii` 表示“期望安装，已经安装”，是正常状态。`rc` 表示已卸载但配置文件还在。

### 4.2 `-l` 加通配符

```
$ dpkg -l "*coreutils*"
...
||| Name                  Version                     Architecture Description
+++-=====================-===========================-============-==========================================
ii  coreutils             9.5-1ubuntu2+0.0.0~ubuntu25 all          coreutils meta package
un  coreutils-from        <none>                      <none>       (no description available)
un  coreutils-from-gnu    <none>                      <none>       (no description available)
ii  coreutils-from-uutils 0.0.0~ubuntu25              all          coreutils from the uutils project
ii  gnu-coreutils         9.7-3ubuntu2.1              amd64        GNU core utilities
ii  rust-coreutils        0.10.0-1ubuntu2~26.04.1     amd64        Universal coreutils utils, written in Rust
```

从这里可以看出：Ubuntu 26.04 的基础命令默认来自 Rust 版（`coreutils-from-uutils`、`rust-coreutils`），GNU 版（`gnu-coreutils`）也装着。

### 4.3 `-S`：这个文件是哪个包装的

```
$ dpkg -S /usr/bin/tree
tree: /usr/bin/tree

$ dpkg -S /usr/bin/ls
coreutils-from-uutils: /usr/bin/ls
```

### 4.4 `-L`：这个包装了哪些文件

```
$ dpkg -L tree | head -6
/.
/usr
/usr/bin
/usr/bin/tree
/usr/share
/usr/share/doc
```

装完一个软件不知道配置文件在哪，用 `dpkg -L 包名 | grep etc`。

### 4.5 `-i`：安装 .deb 文件

```
$ dpkg -i tree_2.3.1-1_amd64.deb
(Reading database ... 16848 files and directories currently installed.)
Preparing to unpack tree_2.3.1-1_amd64.deb ...
Unpacking tree (2.3.1-1) over (2.3.1-1) ...
Setting up tree (2.3.1-1) ...
Processing triggers for man-db (2.13.1-1build1) ...
```

### 4.6 `-r`：卸载

```
$ dpkg -r tree
(Reading database ... 16848 files and directories currently installed.)
Removing tree (2.3.1-1) ...
Processing triggers for man-db (2.13.1-1build1) ...

$ dpkg -l tree
dpkg-query: no packages found matching tree
```

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| `dpkg -i` 报 `dependency problems`，软件装了一半 | dpkg 不会自动安装依赖 | 执行 `sudo apt-get install -f` 自动补齐依赖；或者一开始就用 `sudo apt install ./文件.deb`（注意 `./`），apt 会处理依赖 |
| `apt install 文件.deb` 报找不到包 | 没写 `./`，apt 把它当成了包名 | 写成 `sudo apt install ./文件.deb` |
| `dpkg -l` 里有很多 `rc` 状态的包 | 卸载时用了 remove，配置文件留着 | 不需要的话 `sudo apt purge 包名` 清掉 |

## 6. 一句话总结

装 .deb 用 `sudo apt install ./文件.deb`；查询用 `dpkg -l`、`-L`、`-S`。
