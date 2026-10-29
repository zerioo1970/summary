# 038 reboot：重启系统

> 级别：初级 · 软件包：`systemd`（`reboot` 是指向 `systemctl` 的链接）

## 1. 作用

`reboot` 立即重启计算机。升级内核、改了某些系统配置之后需要重启才能生效。

重启会中断所有正在运行的程序和所有人的连接。在服务器上执行前，确认没有人正在使用，重要服务已经安排好。

## 2. 基本格式

```bash
sudo reboot
```

## 3. 常用参数

| 写法 | 作用 |
|---|---|
| `sudo reboot` | 立即重启 |
| `sudo systemctl reboot` | 效果相同，是 reboot 实际调用的命令 |
| `sudo shutdown -r +10` | 10 分钟后重启，并通知已登录的用户（见 [shutdown](039-shutdown.md)） |

## 4. 常用示例

### 4.1 判断是否需要重启

```bash
cat /var/run/reboot-required
```

`apt upgrade` 升级了内核或核心库之后，Ubuntu 会创建这个文件，内容是 `*** System restart required ***`。文件不存在就说明暂时不需要重启。需要重启的软件包列在 `/var/run/reboot-required.pkgs` 里。

### 4.2 远程服务器重启后重新连接

```bash
sudo reboot
# ssh 连接会断开，等 1–2 分钟后重新 ssh 登录
uptime        # 确认刚刚重启过：up 后面的时间只有几分钟
```

> ⚠️ 本页未实机验证：测试环境不是完整的系统（没有 systemd），无法执行重启。命令写法为 Ubuntu 长期稳定的用法。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 在一台 ssh 窗口里执行了 reboot，重启的却是生产服务器 | 开了多个 ssh 窗口，搞混了 | 执行前看一眼提示符里的主机名，或先执行 `hostname` |
| 重启后服务没有自动起来 | 服务没有设为开机自启 | `sudo systemctl enable 服务名` |
| 重启后连不上服务器 | 网络配置（netplan）或防火墙改错了，重启后才生效 | 改网络配置前确认有控制台（云服务器的 VNC 控制台、机房 KVM）可以兜底 |

## 6. 一句话总结

`sudo reboot` 立即重启；先确认主机名，有 `/var/run/reboot-required` 才需要重启。

---

[← 上一个：unzip](037-unzip.md) · [返回目录](../../README.md) · [下一个：shutdown →](039-shutdown.md)
