# -*- coding: utf-8 -*-
"""新增第8章（一台服务器部署多个 Minimal API）的示意图。输出到 ../images/。"""
import os
import docbuilder as _db
from docbuilder import _box, _arrow, DZ, save
import matplotlib.pyplot as plt

_db.IMG_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "images"))
os.makedirs(_db.IMG_DIR, exist_ok=True)


def ax_(w, h):
    fig, ax = plt.subplots(figsize=(w, h))
    ax.set_xlim(0, w); ax.set_ylim(0, h); ax.axis("off")
    return fig, ax


def fig_layout():
    """图8-1 一台服务器上多个 Minimal API 的布局"""
    fig, ax = ax_(11, 7.2)
    ax.text(5.5, 6.95, "一台服务器上跑多个 Minimal API", ha="center",
            fontsize=15, fontweight="bold", color="#0E4C92")

    # 客户端
    _box(ax, 3.9, 6.05, 3.2, 0.62, "客户端 / 浏览器", DZ["blue"], DZ["blue_e"], fs=11, bold=True)
    # 反向代理
    _box(ax, 3.0, 5.0, 5.0, 0.72, "反向代理  IIS / Nginx（80、443）\n统一入口 + 域名 + HTTPS",
         DZ["gray"], DZ["gray_e"], fs=10, bold=True)
    _arrow(ax, 5.5, 6.05, 5.5, 5.72, color="#C0392B", lw=2)

    apps = [
        ("订单 API", "orders-api", "127.0.0.1:5101", DZ["green"], DZ["green_e"]),
        ("商品 API", "products-api", "127.0.0.1:5102", DZ["orange"], DZ["orange_e"]),
        ("报表 API", "reports-api", "127.0.0.1:5103", DZ["purple"], DZ["purple_e"]),
    ]
    for i, (title, name, port, fc, ec) in enumerate(apps):
        x = 0.35 + i * 3.6
        # 应用外框
        _box(ax, x, 1.15, 3.3, 3.35, "", fc, ec, rounding=0.03, lw=2)
        ax.text(x + 1.65, 4.18, title, ha="center", fontsize=12, fontweight="bold", color=ec)
        # 三要素
        _box(ax, x + 0.22, 3.45, 2.86, 0.55, f"独立目录\\Apps\\{name}", "#ffffff", ec, fs=8.5)
        _box(ax, x + 0.22, 2.75, 2.86, 0.55, f"独立端口 {port}", "#ffffff", ec, fs=8.5)
        _box(ax, x + 0.22, 2.05, 2.86, 0.55, "独立进程（自己的 dotnet）", "#ffffff", ec, fs=8.5)
        _box(ax, x + 0.22, 1.35, 2.86, 0.55, f"服务名 {name}", "#F4F5F7", "#888", fs=8.5)
        # 代理 -> 应用
        _arrow(ax, 5.5 - (1 - i) * 0.0 if False else (3.0 + 5.0 / 6 + i * 5.0 / 3),
               5.0, x + 1.65, 4.5, color="#888", lw=1.6)

    # 底部守护层
    _box(ax, 0.35, 0.35, 10.3, 0.62,
         "进程守护：Windows 服务（sc.exe） 或 systemd —— 开机自启、崩溃自动重启、各应用互不影响",
         DZ["yellow"], DZ["yellow_e"], fs=10, bold=True)
    return save(fig, "ch8_multiapp_layout.png")


def fig_release():
    """图8-2 升级与回滚：releases + current 切换"""
    fig, ax = ax_(11, 4.6)
    ax.text(5.5, 4.35, "只升级一个应用：releases 存版本，current 指向在用版本", ha="center",
            fontsize=14, fontweight="bold", color="#0E4C92")

    # releases 区
    _box(ax, 0.4, 1.0, 4.5, 2.85, "", "#F4F5F7", "#C9CDD4", rounding=0.02, lw=2)
    ax.text(2.65, 3.55, "releases\\  （保留历史版本）", ha="center", fontsize=10.5,
            fontweight="bold", color="#555")
    _box(ax, 0.7, 2.55, 3.9, 0.6, "2026-07-01  上一个版本（可回滚）", "#ffffff", DZ["gray_e"], fs=9)
    _box(ax, 0.7, 1.75, 3.9, 0.6, "2026-07-15  新版本（刚发布）", DZ["green"], DZ["green_e"], fs=9, bold=True)
    _box(ax, 0.7, 1.15, 3.9, 0.45, "……更早版本", "#ffffff", DZ["gray_e"], fs=8.5)

    # current
    _box(ax, 6.1, 2.35, 4.3, 0.95, "current  →  指向某个版本\n（IIS 站点/服务只认这个路径）",
         DZ["blue"], DZ["blue_e"], fs=10, bold=True)
    _arrow(ax, 4.6, 2.05, 6.1, 2.6, color="#2E9E5B", lw=2.2, text="切换指向")

    # 重启
    _box(ax, 6.1, 1.1, 4.3, 0.8, "重启这一个服务即可生效\nRestart-Service orders-api",
         DZ["orange"], DZ["orange_e"], fs=9.5, bold=True)
    _arrow(ax, 8.25, 2.35, 8.25, 1.9, color="#888", lw=1.8)
    ax.text(5.5, 0.55, "回滚 = 把 current 指回上一个版本 + 再重启一次（其它应用完全不受影响）",
            ha="center", fontsize=10, color="#C0392B")
    return save(fig, "ch8_release_switch.png")


if __name__ == "__main__":
    for f in [fig_layout, fig_release]:
        print("saved:", os.path.basename(f()))
