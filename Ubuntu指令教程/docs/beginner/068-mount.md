# 068 mount：挂载磁盘和分区

> 级别：初级 · 软件包：`mount`

## 1. 作用

Linux 里所有文件都在一棵从 `/` 开始的目录树上。一块新硬盘、一个 U 盘、一个网络共享目录，要先**挂载**（mount）到树上的某个空目录，才能通过这个目录访问里面的文件。这个目录叫**挂载点**。

`mount` 不带参数时，列出当前所有的挂载。

## 2. 基本格式

```bash
mount                                # 查看所有挂载
sudo mount [参数] 设备 挂载点          # 挂载
```

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-t 类型` | 指定文件系统类型，如 `ext4`、`vfat`、`ntfs3`、`cifs`（Windows 共享） |
| `-o 选项` | 挂载选项，多个用逗号分隔：`ro` 只读，`rw` 读写，`noexec` 禁止执行程序 |
| `-a` | 挂载 /etc/fstab 里所有还没挂载的条目 |

## 4. 常用示例

### 4.1 查看当前挂载

```
$ findmnt /home
TARGET SOURCE FSTYPE OPTIONS
/home  tmpfs  tmpfs  rw,nosuid,nodev,relatime,seclabel,mode=755
```

`mount` 的完整输出很长，查某个目录用 `mount | grep " /home "` 或 `findmnt 目录` 更清楚（findmnt 在中级篇讲）。

### 4.2 挂载一块新硬盘

```bash
lsblk -f                          # 确认分区名和文件系统，假设是 /dev/sdb1，ext4
sudo mkdir -p /data               # 挂载点必须先存在
sudo mount /dev/sdb1 /data
df -h /data                       # 确认挂载成功
```

### 4.3 挂载 U 盘（只读）

```bash
sudo mount -o ro /dev/sdc1 /mnt
```

### 4.4 开机自动挂载：/etc/fstab

用 mount 挂载的内容重启后就没了。要永久挂载，在 `/etc/fstab` 里加一行：

```
UUID=1234abcd-...  /data  ext4  defaults  0  2
```

UUID 从 `lsblk -f` 或 `blkid` 获得。加完后先执行 `sudo mount -a` 测试，没有报错再重启。

> ⚠️ 4.2–4.4 未实机验证：测试环境的容器不允许挂载设备。命令写法为长期稳定的标准用法。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 挂载后，挂载点里原来的文件“不见了” | 挂载会把原目录的内容盖住，卸载后会重新出现 | 挂载点用空目录 |
| /etc/fstab 写错一个字，重启后系统进入紧急模式 | 开机时挂载失败 | 改完一定先 `sudo mount -a` 测试；不重要的盘在选项里加 `nofail` |
| `mount: wrong fs type, bad option, bad superblock` | 文件系统类型不对，或者分区还没格式化 | 用 `lsblk -f` 确认 FSTYPE |
| 普通用户执行 mount 报 `mount: /mnt: must be superuser to use mount.` | 挂载需要 root | 加 `sudo` |

## 6. 一句话总结

`sudo mount 设备 目录` 临时挂载，永久挂载写 /etc/fstab，写完先 `mount -a` 测试。
