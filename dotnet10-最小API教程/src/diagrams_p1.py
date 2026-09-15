# -*- coding: utf-8 -*-
"""第一批示意图：第 1、2 章"""
import os
import docbuilder as _db
from docbuilder import _new_ax, _box, _arrow, save, DZ
# 统一输出到顶层 images/（markdown 以 ../images 引用）
_db.IMG_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "images"))
os.makedirs(_db.IMG_DIR, exist_ok=True)


def fig_minimal_vs_mvc():
    fig, ax = _new_ax(11, 6.2)
    ax.set_ylim(0, 7)
    ax.text(5, 6.6, "最小 API   vs   传统 MVC 控制器", ha="center",
            fontsize=17, fontweight="bold", color="#0E4C92")

    # 左：最小 API
    _box(ax, 0.3, 0.4, 4.4, 5.7, "", DZ["green"], DZ["green_e"], rounding=0.03, lw=2.2)
    ax.text(2.5, 5.75, "最小 API (Minimal API)", ha="center", fontsize=13,
            fontweight="bold", color="#1e7a45")
    _box(ax, 0.6, 4.55, 3.8, 0.95,
         "Program.cs 一个文件搞定\napp.MapGet(\"/hi\", () => \"Hi\");",
         "#ffffff", DZ["green_e"], fs=10)
    items_l = [
        "少量样板代码，几行即可跑通",
        "启动快、内存占用低",
        "路由 = 一行 Map 方法",
        "适合微服务 / 小型 API",
        "原生支持 AoT 编译",
    ]
    for i, t in enumerate(items_l):
        y = 3.95 - i * 0.66
        ax.text(0.75, y, "✓", fontsize=13, color="#2E9E5B", fontweight="bold")
        ax.text(1.15, y, t, fontsize=10.5, va="center", color="#222")

    # 右：MVC
    _box(ax, 5.3, 0.4, 4.4, 5.7, "", DZ["orange"], DZ["orange_e"], rounding=0.03, lw=2.2)
    ax.text(7.5, 5.75, "传统 MVC 控制器", ha="center", fontsize=13,
            fontweight="bold", color="#b3700a")
    _box(ax, 5.6, 4.55, 3.8, 0.95,
         "Controller 类 + 特性 + 约定\n[ApiController] class XxxController",
         "#ffffff", DZ["orange_e"], fs=10)
    items_r = [
        "结构规范，适合大型复杂系统",
        "功能全面（过滤器/模型绑定等）",
        "样板代码多，文件多",
        "基于约定，学习曲线较陡",
        "启动与内存开销相对更大",
    ]
    for i, t in enumerate(items_r):
        y = 3.95 - i * 0.66
        mark = "✓" if i in (0, 1) else "•"
        col = "#2E9E5B" if i in (0, 1) else "#b3700a"
        ax.text(5.75, y, mark, fontsize=13, color=col, fontweight="bold")
        ax.text(6.15, y, t, fontsize=10.5, va="center", color="#222")

    return save(fig, "fig_1_1_minimal_vs_mvc.png")


def fig_when_to_use():
    fig, ax = _new_ax(11, 4.4)
    ax.set_ylim(0, 5)
    ax.text(2.6, 4.6, "适合用最小 API", ha="center", fontsize=13,
            fontweight="bold", color="#2E9E5B")
    ax.text(7.6, 4.6, "更适合用 MVC / 其他", ha="center", fontsize=13,
            fontweight="bold", color="#C0392B")
    good = ["微服务 / 单一职责的小服务", "为前端/App 提供 JSON 接口",
            "原型验证、Demo、教学", "Serverless / 容器化轻量服务",
            "追求启动速度与低内存"]
    bad = ["大型企业级、模块极多的系统", "需要返回服务器渲染网页(Razor)",
           "重度依赖 MVC 特有过滤器管线", "团队已有大量 MVC 代码与规范"]
    for i, t in enumerate(good):
        y = 4.0 - i * 0.72
        _box(ax, 0.3, y - 0.28, 4.6, 0.56, t, DZ["green"], DZ["green_e"], fs=10.5)
    for i, t in enumerate(bad):
        y = 4.0 - i * 0.72
        _box(ax, 5.1, y - 0.28, 4.6, 0.56, t, DZ["red"], DZ["red_e"], fs=10.5)
    return save(fig, "fig_1_2_when_to_use.png")


def fig_front_back_roles():
    fig, ax = _new_ax(11, 5.4)
    ax.set_ylim(0, 6)
    # 前端
    _box(ax, 0.3, 0.6, 4.2, 4.9, "", DZ["blue"], DZ["blue_e"], rounding=0.03, lw=2.2)
    ax.text(2.4, 5.15, "前端 (Frontend)", ha="center", fontsize=14,
            fontweight="bold", color="#1b5e91")
    ax.text(2.4, 4.62, "用户能看到、能点的界面", ha="center", fontsize=10, color="#333")
    fl = ["HTML / CSS / JavaScript", "Vue / React / Angular",
          "微信小程序 / 手机 App", "运行在：用户的浏览器 / 手机"]
    for i, t in enumerate(fl):
        _box(ax, 0.6, 3.7 - i * 0.74, 3.6, 0.56, t, "#ffffff", DZ["blue_e"], fs=10)
    # 后端
    _box(ax, 5.5, 0.6, 4.2, 4.9, "", DZ["green"], DZ["green_e"], rounding=0.03, lw=2.2)
    ax.text(7.6, 5.15, "后端 (Backend)", ha="center", fontsize=14,
            fontweight="bold", color="#1e7a45")
    ax.text(7.6, 4.62, "最小 API 就在这里", ha="center", fontsize=10, color="#333")
    bl = ["ASP.NET Core 最小 API", "处理业务逻辑", "读写数据库", "运行在：服务器"]
    for i, t in enumerate(bl):
        _box(ax, 5.8, 3.7 - i * 0.74, 3.6, 0.56, t, "#ffffff", DZ["green_e"], fs=10)
    # 中间 HTTP 双箭头
    _arrow(ax, 4.6, 3.0, 5.4, 3.0, color="#C0392B", style="-|>", lw=2.4)
    _arrow(ax, 5.4, 2.4, 4.6, 2.4, color="#C0392B", style="-|>", lw=2.4)
    ax.text(5.0, 3.35, "HTTP 请求", ha="center", fontsize=9.5, color="#C0392B")
    ax.text(5.0, 2.05, "JSON 响应", ha="center", fontsize=9.5, color="#C0392B")
    return save(fig, "fig_2_1_front_back_roles.png")


def fig_http_flow():
    fig, ax = _new_ax(11, 5.6)
    ax.set_ylim(0, 6.2)
    ax.text(5, 5.9, "一次调用的全流程（类比“点外卖”）", ha="center",
            fontsize=15, fontweight="bold", color="#0E4C92")
    _box(ax, 0.4, 4.7, 3.0, 0.9, "前端（顾客）", DZ["blue"], DZ["blue_e"], fs=12, bold=True)
    _box(ax, 6.6, 4.7, 3.0, 0.9, "后端最小 API（餐厅）", DZ["green"], DZ["green_e"], fs=12, bold=True)
    # 竖直生命线
    ax.plot([1.9, 1.9], [0.5, 4.7], color="#8ab4d8", lw=1.5, ls="--", zorder=0)
    ax.plot([8.1, 8.1], [0.5, 4.7], color="#8fce9f", lw=1.5, ls="--", zorder=0)
    steps = [
        (4.1, "① 发请求：GET /api/products", "#2E74B5", True),
        (3.15, "② 处理：查数据库、组织数据", "#2E9E5B", False),
        (2.2, "③ 返回：JSON 数据", "#2E74B5", False),
        (1.25, "④ 前端把数据渲染到页面", "#2E9E5B", True),
    ]
    # ① 请求 前->后
    _arrow(ax, 1.9, 4.1, 8.1, 4.1, color="#2E74B5", lw=2.2)
    ax.text(5.0, 4.28, "① GET /api/products  “我要商品列表”", ha="center", fontsize=10, color="#2E74B5")
    # ② 后端内部处理
    _box(ax, 6.9, 3.0, 2.4, 0.7, "② 查库 + 组织数据", "#ffffff", DZ["green_e"], fs=9.5)
    # ③ 响应 后->前
    _arrow(ax, 8.1, 2.2, 1.9, 2.2, color="#2E9E5B", lw=2.2)
    ax.text(5.0, 2.38, "③ 返回 JSON: [{\"id\":1,\"name\":\"苹果\"}]", ha="center", fontsize=10, color="#2E9E5B")
    # ④ 前端渲染
    _box(ax, 0.5, 1.0, 2.8, 0.7, "④ 渲染成页面列表", "#ffffff", DZ["blue_e"], fs=9.5)
    return save(fig, "fig_2_2_http_flow.png")


def fig_separation_arch():
    fig, ax = _new_ax(11, 4.2)
    ax.set_ylim(0, 4.6)
    _box(ax, 0.3, 1.2, 3.0, 2.0, "前端\n(Vue/React\n/浏览器)", DZ["blue"], DZ["blue_e"], fs=12, bold=True)
    _box(ax, 4.0, 1.2, 3.4, 2.0, "后端最小 API\n(ASP.NET Core)", DZ["green"], DZ["green_e"], fs=12, bold=True)
    _box(ax, 8.1, 1.4, 1.7, 1.6, "数据库", DZ["orange"], DZ["orange_e"], fs=12, bold=True)
    _arrow(ax, 3.3, 2.55, 4.0, 2.55, color="#C0392B", lw=2.2)
    _arrow(ax, 4.0, 1.85, 3.3, 1.85, color="#C0392B", lw=2.2)
    ax.text(3.65, 2.85, "请求", ha="center", fontsize=9, color="#C0392B")
    ax.text(3.65, 1.5, "JSON", ha="center", fontsize=9, color="#C0392B")
    _arrow(ax, 7.4, 2.2, 8.1, 2.2, color="#7a5a10", lw=2.0)
    ax.text(1.8, 0.85, "用户的浏览器", ha="center", fontsize=9.5, color="#666")
    ax.text(5.7, 0.85, "服务器", ha="center", fontsize=9.5, color="#666")
    ax.text(8.95, 0.95, "服务器", ha="center", fontsize=9.5, color="#666")
    ax.text(5, 4.2, "前后端分离架构", ha="center", fontsize=14, fontweight="bold", color="#0E4C92")
    return save(fig, "fig_2_3_separation.png")


def fig_test_tools():
    fig, ax = _new_ax(11, 3.2)
    ax.set_ylim(0, 3.4)
    ax.text(5, 3.1, "没有前端时，用这些工具测试接口", ha="center",
            fontsize=13, fontweight="bold", color="#0E4C92")
    tools = [
        ("浏览器地址栏", "只能测 GET", DZ["gray"], DZ["gray_e"]),
        ("Postman", "图形化，最常用", DZ["orange"], DZ["orange_e"]),
        (".http 文件", "VS/VSCode 内建", DZ["blue"], DZ["blue_e"]),
        ("Swagger UI", ".NET 10 自带", DZ["green"], DZ["green_e"]),
    ]
    for i, (name, desc, fc, ec) in enumerate(tools):
        x = 0.3 + i * 2.45
        _box(ax, x, 1.4, 2.2, 1.1, name, fc, ec, fs=12, bold=True)
        ax.text(x + 1.1, 1.05, desc, ha="center", fontsize=9.5, color="#555")
    return save(fig, "fig_2_4_test_tools.png")


if __name__ == "__main__":
    for f in [fig_minimal_vs_mvc, fig_when_to_use, fig_front_back_roles,
              fig_http_flow, fig_separation_arch, fig_test_tools]:
        print("saved:", f())
