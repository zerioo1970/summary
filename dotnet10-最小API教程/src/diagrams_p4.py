# -*- coding: utf-8 -*-
"""第四批示意图：新增的 Windows 部署与管理章（Part 1 新第4章）。输出到顶层 images/。"""
import os
import docbuilder as _db
from docbuilder import _box, _arrow, DZ, save
import matplotlib.pyplot as plt
from matplotlib.patches import FancyBboxPatch

_db.IMG_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "images"))
os.makedirs(_db.IMG_DIR, exist_ok=True)


def ax_(w, h):
    fig, ax = plt.subplots(figsize=(w, h))
    ax.set_xlim(0, w); ax.set_ylim(0, h); ax.axis("off")
    return fig, ax


def title(ax, x, y, t):
    ax.text(x, y, t, ha="center", fontsize=15, fontweight="bold", color="#0E4C92")


def fig_publish_output():
    fig, ax = ax_(11, 5.2); title(ax, 5.5, 4.9, "dotnet publish 之后：EXE 还是 DLL？")
    _box(ax, 0.5, 3.4, 2.6, 0.9, "dotnet publish\n-c Release", DZ["blue"], DZ["blue_e"], fs=11, bold=True)
    _arrow(ax, 3.1, 3.85, 4.1, 3.85, color="#888", lw=2)
    _box(ax, 4.1, 0.5, 6.4, 4.1, "", "#F4F5F7", "#C9CDD4", rounding=0.02, lw=2)
    ax.text(7.3, 4.3, "publish/ 文件夹", ha="center", fontsize=11.5, fontweight="bold", color="#555")
    files = [
        ("MyApi.dll", "← 程序本体（真正运行的就是它）", DZ["green_e"], True),
        ("MyApi.exe", "← 启动器（依赖框架时随附，可选）", DZ["orange_e"], True),
        ("appsettings.json", "← 配置文件", "#8E44AD", False),
        ("web.config", "← IIS 托管所需（自动生成）", "#C0392B", False),
        ("*.deps.json / runtimeconfig", "← 依赖与运行时配置", "#888", False),
    ]
    for i, (name, desc, col, bold) in enumerate(files):
        y = 3.7 - i * 0.62
        mk = FancyBboxPatch((4.4, y - 0.13), 0.26, 0.26, boxstyle="round,pad=0.005,rounding_size=0.03",
                            linewidth=1.5, edgecolor=col, facecolor=col if bold else "#fff", zorder=3)
        ax.add_patch(mk)
        ax.text(4.85, y, name, fontsize=10.5, va="center", color=col, fontweight="bold" if bold else "normal")
        ax.text(7.0, y, desc, fontsize=8.8, va="center", color="#444")
    return save(fig, "fig_ch4win_publish.png")


def fig_iis_deploy():
    fig, ax = ax_(11, 6.2); title(ax, 5.5, 5.9, "Windows + IIS 部署最小 API 的流程")
    steps = [
        ("① 服务器装\n.NET Hosting Bundle", DZ["blue"], DZ["blue_e"]),
        ("② dotnet publish\n发布项目", DZ["green"], DZ["green_e"]),
        ("③ 拷贝 publish 文件夹\n到服务器", DZ["orange"], DZ["orange_e"]),
        ("④ IIS 新建站点\n物理路径指向它", DZ["purple"], DZ["purple_e"]),
        ("⑤ 应用池设为\n“无托管代码”", DZ["yellow"], DZ["yellow_e"]),
    ]
    for i, (t, fc, ec) in enumerate(steps):
        y = 5.0 - i * 0.82
        _box(ax, 0.6, y, 4.4, 0.68, t, fc, ec, fs=10, bold=True)
        if i < len(steps) - 1:
            _arrow(ax, 2.8, y, 2.8, y - 0.14, color="#888", lw=1.6)
    _box(ax, 5.6, 0.8, 4.9, 3.9, "", "#EAF1FB", "#0E4C92", rounding=0.02, lw=2)
    ax.text(8.05, 4.4, "请求如何被处理", ha="center", fontsize=11.5, fontweight="bold", color="#0E4C92")
    _box(ax, 6.0, 3.4, 4.1, 0.7, "浏览器 / 客户端", DZ["blue"], DZ["blue_e"], fs=10, bold=True)
    _box(ax, 6.0, 2.4, 4.1, 0.7, "IIS（对外的 Web 服务器）", DZ["gray"], DZ["gray_e"], fs=10, bold=True)
    _box(ax, 6.0, 1.4, 4.1, 0.7, "ASP.NET Core 模块\n→ 拉起 dotnet MyApi.dll", DZ["green"], DZ["green_e"], fs=9.5, bold=True)
    _arrow(ax, 8.05, 3.4, 8.05, 3.1, color="#C0392B", lw=1.8)
    _arrow(ax, 8.05, 2.4, 8.05, 2.1, color="#C0392B", lw=1.8)
    return save(fig, "fig_ch4win_iis.png")


def fig_manage():
    fig, ax = ax_(11, 4.8); title(ax, 5.5, 4.5, "写多了怎么办：按功能归位，快速定位与替换")
    feats = [("Products", DZ["blue"], DZ["blue_e"]),
             ("Orders", DZ["green"], DZ["green_e"]),
             ("Users", DZ["orange"], DZ["orange_e"])]
    for i, (f, fc, ec) in enumerate(feats):
        x = 0.6 + i * 3.5
        _box(ax, x, 1.3, 3.0, 2.6, "", fc, ec, rounding=0.03, lw=2)
        ax.text(x + 1.5, 3.6, f"{f} 功能", ha="center", fontsize=11.5, fontweight="bold", color=ec)
        for j, part in enumerate(["XxxEndpoints.cs", "XxxService.cs", "Xxx.cs 模型"]):
            _box(ax, x + 0.3, 2.85 - j * 0.62, 2.4, 0.5, part, "#ffffff", ec, fs=9)
    ax.text(5.5, 0.75, "改哪个功能只动它的文件夹 · OpenAPI 用 Tag 分组 · 需要独立升级就拆成独立服务",
            ha="center", fontsize=9.5, color="#555")
    ax.text(5.5, 0.35, "→ 想替换某个接口：定位到对应 Endpoints 文件，改完重新发布该项目即可",
            ha="center", fontsize=9.5, color="#C0392B")
    return save(fig, "fig_ch4win_manage.png")


if __name__ == "__main__":
    for f in [fig_publish_output, fig_iis_deploy, fig_manage]:
        print("saved:", os.path.basename(f()))
