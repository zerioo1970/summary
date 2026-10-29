# 097 lspci：列出 PCI 设备

> 级别：初级 · 软件包：`pciutils`

## 1. 作用

`lspci` 列出主板上的 PCI/PCIe 设备：显卡、网卡、NVMe 硬盘控制器、RAID 卡等。装显卡驱动、确认服务器用的是哪种网卡时，用它看具体型号。

## 2. 基本格式

```bash
lspci [参数]
```

## 3. 常用参数

| 参数 | 作用 |
|---|---|
| `-k` | 显示每个设备当前使用的驱动（内核模块） |
| `-v` | 显示详细信息 |
| `-nn` | 同时显示厂商和设备的 ID 编号 |
| `-s 地址` | 只显示指定地址的设备，如 `-s 00:03.0` |

## 4. 常用示例

> 以下输出来自测试环境（云服务器），所以设备都是虚拟化平台提供的。

```
$ lspci | head -5
00:00.0 Host bridge: Intel Corporation 440FX - 82441FX PMC [Natoma]
00:01.0 ISA bridge: Intel Corporation 82371SB PIIX3 ISA [Natoma/Triton II]
00:01.3 Non-VGA unclassified device: Intel Corporation 82371AB/EB/MB PIIX4 ACPI (rev 08)
00:03.0 VGA compatible controller: Amazon.com, Inc. Device 1111
00:04.0 Non-Volatile memory controller: Amazon.com, Inc. NVMe EBS Controller
```

每行开头是设备地址，后面是设备类型和型号。

常用的筛选：

```bash
lspci | grep -i vga          # 显卡
lspci | grep -i ethernet     # 有线网卡
lspci -k | grep -A 3 -i vga  # 显卡用的是哪个驱动
```

`lspci -k` 的输出里，`Kernel driver in use: nvidia` 说明在用 NVIDIA 官方驱动，显示 `nouveau` 则是开源驱动。

> ⚠️ `lspci -k` 的说明未实机验证：测试环境没有独立显卡。

## 5. 一句话总结

`lspci | grep -i 关键词` 查硬件型号，`-k` 看用的是哪个驱动。

---

[← 上一个：lsusb](096-lsusb.md) · [返回目录](../../README.md) · [下一个：nproc →](098-nproc.md)
