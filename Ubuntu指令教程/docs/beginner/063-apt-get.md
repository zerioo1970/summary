# 063 apt-get：脚本里用的软件包管理命令

> 级别：初级 · 软件包：`apt`

## 1. 作用

`apt-get` 和 [apt](012-apt.md) 做的是同一件事：安装、升级、卸载软件。区别在于用途：

| | apt | apt-get |
|---|---|---|
| 面向 | 人在终端里操作 | 脚本、自动化部署、Dockerfile |
| 输出 | 有颜色、进度条，格式可能随版本调整 | 格式稳定，多年不变 |
| 在脚本里用 | 会提示 `WARNING: apt does not have a stable CLI interface` | 不会提示 |

日常手动操作用 apt，写进脚本的一律用 apt-get。

## 2. 基本格式

```bash
sudo apt-get [参数] 子命令 [包名...]
```

## 3. 常用子命令和参数

| 子命令 | 作用 |
|---|---|
| `update` | 刷新软件包清单 |
| `upgrade` | 升级已安装的包 |
| `install 包名` | 安装 |
| `remove 包名` | 卸载，保留配置 |
| `purge 包名` | 卸载，删除配置 |
| `autoremove` | 删除不再需要的依赖 |
| `clean` | 清空已下载的安装包缓存（/var/cache/apt/archives） |
| `download 包名` | 只下载 .deb 文件到当前目录，不安装 |

| 参数 | 作用 |
|---|---|
| `-y` | 自动回答“是” |
| `-q` / `-qq` | 减少输出 / 几乎不输出 |
| `--no-install-recommends` | 不安装“推荐”的附加包，只装必需的依赖 |
| `-s` | 模拟执行 |

## 4. 常用示例

以下以 root 身份执行（普通用户加 sudo）。

### 4.1 脚本里的标准写法

```bash
sudo apt-get update
sudo DEBIAN_FRONTEND=noninteractive apt-get install -y -q nginx
```

`DEBIAN_FRONTEND=noninteractive` 让安装过程中不弹出任何配置问答，全部用默认值。自动化脚本里不加它，遇到需要回答问题的包就会卡住。

### 4.2 `--no-install-recommends`：装得更少

```
$ apt-get install -y --no-install-recommends -q fortune-mod
...
The following additional packages will be installed:
  librecode3
Recommended packages:
  fortunes-min | fortune-cookie-db
The following NEW packages will be installed:
  fortune-mod librecode3
```

“推荐”的包（Recommended）没有装。做 Docker 镜像、精简服务器时常用这个参数控制体积。

### 4.3 `download`：只下载安装包

```
$ apt-get download tree
Get:1 http://archive.ubuntu.com/ubuntu resolute/universe amd64 tree amd64 2.3.1-1 [53.5 kB]
Fetched 53.5 kB in 0s (166 kB/s)

$ ls *.deb
tree_2.3.1-1_amd64.deb
```

给不能上网的内网服务器装软件时，先在能上网的机器上下载，再拷过去用 [dpkg](064-dpkg.md) 安装。

### 4.4 `clean`：释放缓存空间

```
$ apt-get clean; ls /var/cache/apt/archives
lock
partial
```

下载过的 .deb 文件都被删除了。/var 分区紧张时可以清一下。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 脚本执行到 apt-get install 就停住不动 | 等待输入 `Y`，或者弹出了配置问答 | 加 `-y`，并设置 `DEBIAN_FRONTEND=noninteractive` |
| Dockerfile 里 `apt-get install` 报找不到包 | 镜像里的软件包清单是空的 | 在同一个 RUN 里先 `apt-get update`：`RUN apt-get update && apt-get install -y 包名` |
| 自动化安装时输出一大段 `debconf: unable to initialize frontend: Dialog` | 没有终端，debconf 尝试几种问答界面都失败，最后改用默认值 | 这些是提示不是错误；加 `DEBIAN_FRONTEND=noninteractive` 就不会出现 |

## 6. 一句话总结

脚本里用 `apt-get install -y`，配上 `DEBIAN_FRONTEND=noninteractive`。
