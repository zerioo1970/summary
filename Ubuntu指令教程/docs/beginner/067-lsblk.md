# 067 lsblk：列出磁盘和分区

> 级别：初级 · 软件包：`util-linux`

## 1. 作用

`lsblk`（list block devices）用树形列出所有磁盘（块设备）、磁盘上的分区，以及它们挂载在哪个目录。新加了一块硬盘、插了一个 U 盘，想知道它叫什么名字（/dev/sdb 还是 /dev/nvme1n1），就用 lsblk。

## 2. 基本格式

```bash
lsblk [参数] [设备]
```

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-f` | 显示文件系统类型、卷标、UUID、可用空间 |
| `-d` | 只显示磁盘，不显示分区 |
| `-o 列,...` | 自定义显示的列 |
| `-p` | 显示设备的完整路径（/dev/...） |

## 4. 常用示例

> 以下输出来自测试环境（云服务器）。

### 4.1 `lsblk`

```
$ lsblk
NAME          MAJ:MIN RM  SIZE RO TYPE MOUNTPOINTS
loop0           7:0    0 86.9G  0 loop
loop1           7:1    0 86.9G  0 loop /
nvme0n1       259:0    0   30G  0 disk
├─nvme0n1p1   259:1    0   30G  0 part
├─nvme0n1p127 259:2    0    1M  0 part
└─nvme0n1p128 259:3    0   10M  0 part
nvme1n1       259:4    0  128G  0 disk /etc/resolv.conf
```

| 列 | 含义 |
|---|---|
| NAME | 设备名，完整路径是 `/dev/` 加上它 |
| SIZE | 容量 |
| TYPE | `disk` 整块磁盘，`part` 分区，`loop` 回环设备（snap、镜像文件），`lvm` 逻辑卷 |
| MOUNTPOINTS | 挂载到的目录，空白表示没有挂载 |

磁盘命名规律：

| 名字 | 是什么 |
|---|---|
| `sda`、`sdb` | SATA/SAS 硬盘、U 盘，依次编号 |
| `nvme0n1`、`nvme1n1` | NVMe 固态硬盘 |
| `sda1`、`nvme0n1p1` | 上面那块盘的第 1 个分区 |
| `vda` | 虚拟机里的虚拟磁盘 |

### 4.2 `-d`：只看磁盘

```
$ lsblk -d
NAME    MAJ:MIN RM  SIZE RO TYPE MOUNTPOINTS
loop0     7:0    0 86.9G  0 loop
loop1     7:1    0 86.9G  0 loop /
nvme0n1 259:0    0   30G  0 disk
nvme1n1 259:4    0  128G  0 disk /etc/resolv.conf
```

### 4.3 `-o`：只看关心的列

```
$ lsblk -o NAME,SIZE,TYPE,FSTYPE,MOUNTPOINTS
NAME           SIZE TYPE FSTYPE MOUNTPOINTS
loop0         86.9G loop
loop1         86.9G loop        /
nvme0n1         30G disk
├─nvme0n1p1     30G part
├─nvme0n1p127    1M part
└─nvme0n1p128   10M part
nvme1n1        128G disk        /etc/resolv.conf
```

### 4.4 `-f`：看文件系统和 UUID

```bash
lsblk -f
```

在普通 Ubuntu 机器上，FSTYPE 一列会显示 ext4、vfat（EFI 分区）、swap 等，UUID 一列是写进 `/etc/fstab` 时用的唯一编号。测试环境的容器读不到这些信息，所以这两列是空的。

## 5. ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
|---|---|---|
| 新插的硬盘在 lsblk 里只有 disk，没有 part | 新硬盘还没分区 | 用 `fdisk` 或 `parted` 分区后格式化（中级篇讲） |
| 云服务器扩容了磁盘，lsblk 显示的磁盘变大了，`df -h` 里的分区没变 | 扩容的是磁盘，分区和文件系统还要单独扩展 | 用 `growpart` 扩分区，再用 `resize2fs` 扩文件系统（高级篇讲） |
| 每次开机 sdb、sdc 的顺序可能不一样 | 设备名按识别顺序分配 | 在 /etc/fstab 里用 UUID，不用 /dev/sdb1 这样的名字 |

## 6. 一句话总结

`lsblk` 看有哪些盘和分区，`lsblk -f` 看文件系统和 UUID。

---

[← 上一个：systemctl](066-systemctl.md) · [返回目录](../../README.md) · [下一个：mount →](068-mount.md)
