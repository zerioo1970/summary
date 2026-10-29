# 065 snap：管理 Snap 软件包

> 级别：初级 · 软件包：`snapd`（Ubuntu 桌面版和服务器版默认安装）

## 1. 作用

`snap` 管理 Snap 格式的软件包。Snap 是 Canonical（Ubuntu 的开发公司）推出的软件分发格式：每个 snap 自带运行所需的依赖库，在沙箱里运行，并且会**自动更新**。

Ubuntu 上有些软件只提供或优先提供 snap 版，例如 Firefox、Thunderbird、LXD、MicroK8s。Ubuntu 桌面版的“应用中心”装的软件也大多是 snap。

| | apt（.deb） | snap |
|---|---|---|
| 依赖 | 和系统共用库 | 每个软件自带 |
| 更新 | 手动 `apt upgrade` | 后台自动更新 |
| 体积 | 小 | 大 |
| 启动速度 | 快 | 第一次启动较慢 |

## 2. 基本格式

```bash
snap 子命令 [参数] [软件名]
```

修改类的子命令（install、remove、refresh）要加 sudo。

## 3. 常用子命令

| 子命令 | 作用 |
|---|---|
| `find 关键词` | 在 Snap 商店里搜索 |
| `info 软件名` | 查看详情和可用的版本通道 |
| `install 软件名` | 安装 |
| `list` | 列出已安装的 snap |
| `refresh [软件名]` | 立即更新 |
| `remove 软件名` | 卸载 |
| `services` | 查看 snap 提供的后台服务 |

常用参数：

| 参数 | 作用 |
|---|---|
| `--classic` | 以“经典模式”安装，不受沙箱限制。开发工具（如 VS Code、各种 SDK）通常需要 |
| `--channel=通道` | 指定版本通道，如 `--channel=1.30/stable` |

## 4. 常用示例

> ⚠️ 本页未实机验证：snapd 需要 systemd 作为系统的第一个进程才能运行，测试环境不满足。命令写法为 snap 长期稳定的用法。

```bash
snap find postman                  # 搜索
snap info code                     # 看详情和通道
sudo snap install code --classic   # 安装 VS Code（需要经典模式）
snap list                          # 已安装列表
sudo snap refresh                  # 立即更新所有 snap
sudo snap remove code              # 卸载
```

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 安装时报错，提示需要 `--classic` | 这个软件需要访问沙箱外的文件 | 加 `--classic` |
| `df -h` 里出现一大堆 `/dev/loop` 设备 | 每个 snap 都以只读镜像的方式挂载 | 这是正常的。`df -h -x squashfs` 可以不显示它们 |
| 磁盘被 snap 占了好几个 GB | snap 默认保留每个软件的旧版本 | `sudo snap set system refresh.retain=2` 只保留 2 个版本 |
| snap 软件读不到 `/mnt` 或其他目录下的文件 | 沙箱限制了文件访问范围 | 用 `snap connections 软件名` 查看权限，必要时改用 apt 版本 |

## 6. 一句话总结

apt 找不到或官方推荐 snap 时用 `sudo snap install`，开发工具加 `--classic`。

---

[← 上一个：dpkg](064-dpkg.md) · [返回目录](../../README.md) · [下一个：systemctl →](066-systemctl.md)
