# 066 systemctl：管理系统服务

> 级别：初级 · 软件包：`systemd`（Ubuntu 26.04 为 systemd 259）

## 1. 作用

`systemctl` 管理 systemd 下的服务（service，在后台长期运行的程序，比如 nginx、mysql、ssh）：启动、停止、重启、查看状态、设置开机自启。装好一个服务之后，几乎所有的日常操作都通过 systemctl 完成。

每个服务都对应一个**单元文件**（unit file），描述怎么启动它、失败了怎么办。系统自带的单元文件在 `/usr/lib/systemd/system/`，自己写的放 `/etc/systemd/system/`。

## 2. 基本格式

```bash
sudo systemctl 子命令 服务名
```

服务名后面的 `.service` 可以省略：`nginx` 等同 `nginx.service`。

## 3. 常用子命令

| 子命令 | 作用 |
|---|---|
| `status` | 查看状态和最近几行日志（不需要 sudo） |
| `start` / `stop` | 启动 / 停止 |
| `restart` | 重启 |
| `reload` | 重新加载配置，不中断服务（服务要支持才行） |
| `enable` / `disable` | 设置 / 取消开机自启 |
| `enable --now` | 设为开机自启并立即启动 |
| `is-active` / `is-enabled` | 只输出是否在运行 / 是否开机自启，适合脚本 |
| `list-units --type=service` | 列出正在运行的服务 |
| `list-unit-files` | 列出所有单元文件及其自启状态 |
| `cat 服务名` | 查看单元文件内容 |
| `daemon-reload` | 修改单元文件后，让 systemd 重新读取 |

## 4. 常用示例

> ⚠️ 本节中 start、stop、status 等需要 systemd 正在运行的命令未实机验证（测试环境没有以 systemd 启动）。`cat`、`list-unit-files`、`is-enabled` 已在 Ubuntu 26.04 实测。

### 4.1 日常操作

```bash
systemctl status nginx            # 看状态
sudo systemctl restart nginx      # 改了配置后重启
sudo systemctl reload nginx       # 或者只重新加载配置
sudo systemctl enable --now nginx # 开机自启并立即启动
```

`status` 的输出里，重点看这两行：

```
     Loaded: loaded (/usr/lib/systemd/system/nginx.service; enabled; preset: enabled)
     Active: active (running) since Thu 2026-10-29 08:00:00 UTC; 5min ago
```

`enabled` 表示开机自启；`active (running)` 表示正在运行。服务启动失败时，这里显示 `failed`，下面会跟着最近几行错误日志。

### 4.2 `cat`：看服务是怎么启动的

```
$ systemctl cat ssh.service
# /usr/lib/systemd/system/ssh.service
[Unit]
Description=OpenBSD Secure Shell server
Documentation=man:sshd(8) man:sshd_config(5)
After=network.target nss-user-lookup.target auditd.service
ConditionPathExists=!/etc/ssh/sshd_not_to_be_run

[Service]
EnvironmentFile=-/etc/default/ssh
ExecStartPre=/usr/sbin/sshd -t
ExecStart=/usr/sbin/sshd -D $SSHD_OPTS
ExecReload=/usr/sbin/sshd -t
ExecReload=/bin/kill -HUP $MAINPID
KillMode=process
Restart=on-failure
...
```

`ExecStart` 是启动命令，`Restart=on-failure` 表示进程异常退出时自动拉起。

### 4.3 查看开机自启的单元

```
$ systemctl list-unit-files --state=enabled
UNIT FILE                       STATE   PRESET
cron.service                    enabled enabled
e2scrub_reap.service            enabled enabled
getty@.service                  enabled enabled
sshd-keygen.service             enabled enabled
systemd-pstore.service          enabled enabled
systemd-resolved.service        enabled enabled
systemd-timesyncd.service       enabled enabled
wtmpdb-update-boot.service      enabled enabled
ssh.socket                      enabled enabled
...
```

### 4.4 Ubuntu 26.04 的 ssh 是按需启动的

```
$ systemctl list-unit-files "ssh*"
UNIT FILE           STATE    PRESET
ssh.service         disabled enabled
sshd-keygen.service enabled  enabled
sshd.service        alias    -
sshd@.service       static   -
ssh.socket          enabled  enabled
ssh-access.target   static   -

6 unit files listed.
```

`ssh.service` 是 disabled，开机自启的是 `ssh.socket`：systemd 先替 ssh 监听 22 端口，有人连接时才启动 sshd。所以改 ssh 端口时，只改 `/etc/ssh/sshd_config` 后重启 ssh.service 不一定生效，还需要 `sudo systemctl daemon-reload` 并重启 `ssh.socket`。

### 4.5 脚本里判断服务是否在运行

```bash
if systemctl is-active --quiet nginx; then
    echo "nginx 正在运行"
fi
```

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 改了单元文件，重启服务后没有变化，还提示 `Run 'systemctl daemon-reload' to reload units` | systemd 缓存了旧的单元文件 | `sudo systemctl daemon-reload` 后再重启 |
| `start` 了服务，重启服务器后它没起来 | start 只管这一次，不设开机自启 | `sudo systemctl enable 服务名` |
| 服务启动失败，status 里只看到几行日志 | status 只显示最近 10 行 | 用 `journalctl -u 服务名 -e` 看完整日志 |
| 在容器里报 `System has not been booted with systemd as init system (PID 1). Can't operate.` | 容器里通常没有运行 systemd | 在容器里直接运行程序，不用 systemctl |
| 直接修改 `/usr/lib/systemd/system/` 下的文件，升级软件后改动消失 | 那是软件包管理的文件，会被覆盖 | 用 `sudo systemctl edit 服务名` 写覆盖配置，保存在 `/etc/systemd/system/` |

第四条的实测：

```
$ systemctl status ssh
System has not been booted with systemd as init system (PID 1). Can't operate.
Failed to connect to system scope bus via local transport: Host is down
```

## 6. 一句话总结

`status` 看状态，`restart` 重启，`enable --now` 开机自启；改单元文件后先 `daemon-reload`。

---

[← 上一个：snap](065-snap.md) · [返回目录](../../README.md) · [下一个：lsblk →](067-lsblk.md)
