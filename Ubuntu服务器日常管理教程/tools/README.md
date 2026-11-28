# 实测环境说明

教程里的命令输出和安装界面截图是怎么得到的，以及沙箱重置后怎样重建。所有工具放在 `/projects/sandbox/.tools`（沙箱重置会丢），本目录保存脚本副本。

## 1. 安装镜像和系统文件

```bash
# 下载并校验镜像（SHA256 与 releases.ubuntu.com/26.04/SHA256SUMS 一致）
curl -L -o .tools/iso/ubuntu-26.04.1-live-server-amd64.iso https://releases.ubuntu.com/26.04/ubuntu-26.04.1-live-server-amd64.iso
sha256sum .tools/iso/ubuntu-26.04.1-live-server-amd64.iso
# cc8a95cde20f6ced61a322420de00f10cc3c90ced545daa46cb9c1a117f1d927
```

- 用 7-Zip 的 Linux 版 `7zz`（7-zip.org 下载）解开 ISO 和其中的 squashfs。
- 装好后的系统 = `casper/ubuntu-server-minimal.squashfs` + `casper/ubuntu-server-minimal.ubuntu-server.squashfs` 两层依次解到 `.tools/u2604`（`7zz x -snld`，保留符号链接）。
- `installer/ur.sh 命令` 用 bwrap 在这个根文件系统里执行命令（主机名 `web01`），用于 `cat /etc/os-release`、`lsb_release -a` 等。内核相关输出（`uname`）是沙箱宿主机的，不能用。
- 软件包版本从 `casper/*.manifest` 读取（例如内核 `linux-image-7.0.0-30-generic 7.0.0-30.30`、`open-vm-tools 2:13.0.10-1ubuntu1`）。

## 2. 安装向导截图（第 1-1 章）

安装程序是镜像里 `casper/ubuntu-server-minimal.ubuntu-server.installer.squashfs` 中的 `subiquity_7403.snap`（版本 26.04.1）。沙箱没有虚拟化，用 subiquity 自带的演示模式 `--dry-run` 运行界面，不写磁盘：

1. 解出 snap 到 `.tools/iso/snap`（其中有 Python 3.12 和全部依赖）。
2. snap 依赖 core24 的系统库：下载 `ubuntu-base-24.04.5-base-amd64.tar.gz` 解到 `.tools/r2404`，补装 `libexpat1`、`libreadline8t64`、`libsqlite3-0`、新版 `distro-info-data`（`dpkg-deb -x`），并在 `/usr/local/bin` 放几个桩程序：`localectl`、`timedatectl`、`systemctl`、`setupcon`、`loadkeys`（直接退出 0）、`systemd-run`（去掉选项后直接执行命令）、`subiquity-legacy-cloud-init-validate`（输出 `{}`）。
3. 克隆 subiquity 源码 `git clone --branch 26.04.1 https://github.com/canonical/subiquity.git .tools/subiquity-src`，打上 `installer/subiquity-dryrun.patch`。补丁只改演示模式的假数据：版本信息改成 26.04.1、GeoIP 国家改成中国（镜像源显示 `cn.archive.ubuntu.com`）、网卡改成 DHCP、不弹"安装程序有新版本"。
4. 机器配置 `installer/machine-vmware.json`：由源码里的 `examples/machines/simple.json` 改成 40 GiB 的 `VMware_Virtual_S` 硬盘、Intel 82545EM 网卡 `ens33`（MAC `00:0c:29:…`，地址 192.168.1.50/24）。复制到 `subiquity-src/examples/machines/vmware.json`；`dry-run-config.yaml` 复制到 `examples/dry-run-configs/kiro.yaml`；镜像里的 `casper/install-sources.yaml` 复制到 `examples/sources/iso2604.yaml`（安装类型那一屏的两个选项来自它）。
5. `COLS=100 ROWS=32 python3 installer/drive.py installer/st_final.json 输出目录`：在伪终端里启动 `sq.sh`，按剧本发按键，用 pyte（`pip install --target=.tools/pylib pyte`）记录每屏的字符和颜色。`BOOT=bios` 可以改成 BIOS 固件。剧本步骤：`wait` 等文字出现、`keys` 发按键、`tabto` 连按 Tab 直到某个按钮高亮、`shot` 保存。
6. `python3 installer/render.py html目录 输出目录/*.json` 生成 HTML（DejaVu Sans Mono 字体，颜色按 subiquity 的调色板），`node installer/shot.mjs png目录 html目录/*.html` 用无头 Chromium 截图。

演示模式做不到的：安装进度日志（靠 journald，沙箱没有）、"Installation complete!" 那一屏、重启和第一次开机。这些内容在正文里标注了未实机验证，界面文字从源码 `subiquity/ui/views/installprogress.py` 核实。

## 3. PowerShell 输出

`Get-FileHash` 等命令用 Linux 版 PowerShell 7.7.0（GitHub 发布页的 `linux-x64.tar.gz`）对真实 ISO 文件执行；Windows 上的路径写法按 Windows 改写。

## 4. 测试环境做不到、正文标注 ⚠ 未实机验证 的内容

- VMware Workstation 的界面和菜单；
- 安装进度、重启、控制台登录和欢迎信息；
- 依赖真实虚拟机的命令输出：`ip`、`lsblk`、`df`、`vgs`、`systemctl`；
- `sudo` 的密码提示。
