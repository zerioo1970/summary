# -*- coding: utf-8 -*-
"""第二批示意图：第 2 章（运行原理）、第 3 章（环境搭建）、附录 A（前端方法全景）"""
from docbuilder import _new_ax, _box, _arrow, save, DZ
import matplotlib.pyplot as plt
from matplotlib.patches import FancyBboxPatch


# ============================ 第 2 章 ============================
def fig_pipeline_journey():
    """一个 HTTP 请求在 ASP.NET Core 中的旅程"""
    fig, ax = _new_ax(11.5, 4.6)
    ax.set_xlim(0, 11.5); ax.set_ylim(0, 4.6)
    ax.text(5.75, 4.35, "一个 HTTP 请求的完整旅程", ha="center",
            fontsize=15, fontweight="bold", color="#0E4C92")
    boxes = [
        ("客户端\n(浏览器/App)", DZ["blue"], DZ["blue_e"]),
        ("Kestrel\nWeb 服务器", DZ["purple"], DZ["purple_e"]),
        ("中间件管道\nMiddleware", DZ["orange"], DZ["orange_e"]),
        ("路由\nRouting", DZ["yellow"], DZ["yellow_e"]),
        ("端点处理函数\n(你写的代码)", DZ["green"], DZ["green_e"]),
    ]
    n = len(boxes)
    w = 1.85; gap = (11.5 - n * w) / (n + 1)
    y = 2.5
    xs = []
    for i, (t, fc, ec) in enumerate(boxes):
        x = gap + i * (w + gap)
        xs.append(x)
        _box(ax, x, y, w, 1.05, t, fc, ec, fs=10, bold=True)
    for i in range(n - 1):
        _arrow(ax, xs[i] + w, y + 0.52, xs[i + 1], y + 0.52, color="#C0392B", lw=2.0)
    # 顶部“请求”标注
    ax.text(5.75, 3.75, "① HTTP 请求  →  →  →", ha="center", fontsize=10, color="#C0392B")
    # 返回箭头
    ax.annotate("", xy=(xs[0] + w/2, 2.5), xytext=(xs[-1] + w/2, 2.5),
                arrowprops=dict(arrowstyle="-|>", color="#2E9E5B", lw=2.0,
                                connectionstyle="arc3,rad=-0.25"))
    ax.text(5.75, 1.25, "② HTTP 响应 (JSON) 沿原路返回客户端", ha="center",
            fontsize=10, color="#2E9E5B")
    return save(fig, "fig_ch2_pipeline.png")


def fig_builder_app():
    """WebApplicationBuilder 与 WebApplication 的关系"""
    fig, ax = _new_ax(11, 5.0)
    ax.set_xlim(0, 11); ax.set_ylim(0, 5.0)
    ax.text(5.5, 4.7, "builder 与 app：配置阶段 → 运行阶段", ha="center",
            fontsize=15, fontweight="bold", color="#0E4C92")
    # builder
    _box(ax, 0.4, 0.6, 4.3, 3.6, "", DZ["blue"], DZ["blue_e"], rounding=0.03, lw=2.2)
    ax.text(2.55, 3.85, "WebApplicationBuilder", ha="center", fontsize=12.5,
            fontweight="bold", color="#1b5e91")
    ax.text(2.55, 3.45, "（配置阶段：准备原料）", ha="center", fontsize=9.5, color="#555")
    for i, t in enumerate(["注册服务  builder.Services", "读取配置  builder.Configuration",
                            "设置日志  builder.Logging"]):
        _box(ax, 0.7, 2.55 - i * 0.72, 3.7, 0.56, t, "#ffffff", DZ["blue_e"], fs=9.5)
    # app
    _box(ax, 6.3, 0.6, 4.3, 3.6, "", DZ["green"], DZ["green_e"], rounding=0.03, lw=2.2)
    ax.text(8.45, 3.85, "WebApplication", ha="center", fontsize=12.5,
            fontweight="bold", color="#1e7a45")
    ax.text(8.45, 3.45, "（运行阶段：对外服务）", ha="center", fontsize=9.5, color="#555")
    for i, t in enumerate(["配置中间件  app.Use...()", "映射路由  app.MapGet/MapPost",
                            "启动监听  app.Run()"]):
        _box(ax, 6.6, 2.55 - i * 0.72, 3.7, 0.56, t, "#ffffff", DZ["green_e"], fs=9.5)
    # 中间箭头
    _arrow(ax, 4.7, 2.3, 6.3, 2.3, color="#C0392B", lw=2.4)
    ax.text(5.5, 2.62, "var app =", ha="center", fontsize=10, color="#C0392B", fontweight="bold")
    ax.text(5.5, 2.0, "builder.Build();", ha="center", fontsize=10, color="#C0392B", fontweight="bold")
    return save(fig, "fig_ch2_builder_app.png")


def fig_middleware_onion():
    """中间件洋葱模型"""
    fig, ax = _new_ax(8.2, 6.0)
    ax.set_xlim(0, 8.2); ax.set_ylim(0, 6.0)
    ax.text(4.1, 5.75, "中间件管道（洋葱模型）", ha="center",
            fontsize=15, fontweight="bold", color="#0E4C92")
    layers = [
        ("异常处理中间件", DZ["red"], DZ["red_e"], 6.6, 4.2),
        ("HTTPS 重定向", DZ["orange"], DZ["orange_e"], 5.4, 3.5),
        ("认证 / 授权", DZ["purple"], DZ["purple_e"], 4.2, 2.8),
        ("路由 Routing", DZ["blue"], DZ["blue_e"], 3.0, 2.1),
    ]
    cx = 4.1
    for name, fc, ec, w, h in layers:
        x = cx - w / 2
        y = 2.9 - h / 2
        _box(ax, x, y, w, h, "", fc, ec, rounding=0.04, lw=1.8)
        ax.text(cx, y + h - 0.28, name, ha="center", fontsize=10.5,
                fontweight="bold", color=ec)
    # 中心端点
    _box(ax, cx - 0.95, 2.9 - 0.5, 1.9, 1.0, "端点\n(你的代码)", DZ["green"],
         DZ["green_e"], fs=10, bold=True, rounding=0.05)
    # 进出箭头
    _arrow(ax, 0.15, 3.6, 1.1, 3.15, color="#C0392B", lw=2.2)
    ax.text(0.55, 3.85, "请求进", fontsize=9.5, color="#C0392B")
    _arrow(ax, 1.1, 2.35, 0.15, 1.9, color="#2E9E5B", lw=2.2)
    ax.text(0.55, 1.55, "响应出", fontsize=9.5, color="#2E9E5B")
    ax.text(4.1, 0.35, "请求由外向内穿过每一层，到达端点后，响应再由内向外返回",
            ha="center", fontsize=9.5, color="#555")
    return save(fig, "fig_ch2_middleware.png")


def fig_host():
    """Host / Kestrel / DI 关系"""
    fig, ax = _new_ax(9.5, 5.2)
    ax.set_xlim(0, 9.5); ax.set_ylim(0, 5.2)
    _box(ax, 0.4, 0.5, 8.7, 4.0, "", "#EAF1FB", "#0E4C92", rounding=0.02, lw=2.4)
    ax.text(4.75, 4.15, "ASP.NET Core 主机 (Host)", ha="center",
            fontsize=14, fontweight="bold", color="#0E4C92")
    ax.text(4.75, 3.72, "统一托管、启动与关闭下面所有组件", ha="center", fontsize=9.5, color="#555")
    inner = [
        ("Kestrel\nWeb 服务器", DZ["purple"], DZ["purple_e"]),
        ("依赖注入容器\nDI Container", DZ["green"], DZ["green_e"]),
        ("配置系统\nConfiguration", DZ["orange"], DZ["orange_e"]),
        ("日志系统\nLogging", DZ["blue"], DZ["blue_e"]),
    ]
    w = 1.9; gap = (8.7 - 4 * w) / 5
    for i, (t, fc, ec) in enumerate(inner):
        x = 0.4 + gap + i * (w + gap)
        _box(ax, x, 1.0, w, 2.1, t, fc, ec, fs=10, bold=True)
    return save(fig, "fig_ch2_host.png")


# ============================ 第 3 章 ============================
def fig_sdk_runtime():
    fig, ax = _new_ax(10, 3.2)
    ax.set_xlim(0, 10); ax.set_ylim(0, 3.2)
    ax.text(5, 3.0, "SDK 与 运行时（Runtime）的区别", ha="center",
            fontsize=14, fontweight="bold", color="#0E4C92")
    _box(ax, 0.4, 0.5, 4.5, 2.0, "", DZ["blue"], DZ["blue_e"], rounding=0.03, lw=2.2)
    ax.text(2.65, 2.15, ".NET SDK（开发用）", ha="center", fontsize=12, fontweight="bold", color="#1b5e91")
    ax.text(2.65, 1.55, "包含：编译器 + CLI 工具", ha="center", fontsize=9.5, color="#333")
    ax.text(2.65, 1.15, "+ 运行时", ha="center", fontsize=9.5, color="#333")
    ax.text(2.65, 0.72, "写代码 / 编译 / 运行都靠它", ha="center", fontsize=9, color="#666")
    _box(ax, 5.1, 0.5, 4.5, 2.0, "", DZ["green"], DZ["green_e"], rounding=0.03, lw=2.2)
    ax.text(7.35, 2.15, ".NET Runtime（运行用）", ha="center", fontsize=12, fontweight="bold", color="#1e7a45")
    ax.text(7.35, 1.55, "只包含：运行已编译程序", ha="center", fontsize=9.5, color="#333")
    ax.text(7.35, 1.15, "所需的环境", ha="center", fontsize=9.5, color="#333")
    ax.text(7.35, 0.72, "部署服务器上装它即可", ha="center", fontsize=9, color="#666")
    return save(fig, "fig_ch3_sdk_runtime.png")


def fig_project_structure():
    fig, ax = _new_ax(11, 5.6)
    ax.set_xlim(0, 11); ax.set_ylim(0, 5.6)
    ax.text(5.5, 5.35, "最小 API 项目结构", ha="center",
            fontsize=15, fontweight="bold", color="#0E4C92")
    # is_dir: True=文件夹(实心块) False=文件(空心块)
    tree = [
        (0, "MyApi/", "项目根目录", "#0E4C92", True, True),
        (1, "Program.cs", "程序入口——写接口的地方（最重要）", "#2E9E5B", True, False),
        (1, "MyApi.csproj", "项目文件：目标框架、NuGet 依赖", "#E08E0B", False, False),
        (1, "appsettings.json", "配置文件：连接串、参数等", "#8E44AD", False, False),
        (1, "Properties/", "", "#666666", False, True),
        (2, "launchSettings.json", "本地启动配置：端口、环境变量", "#8E44AD", False, False),
        (1, "bin/", "编译输出目录（自动生成，可忽略）", "#999999", False, True),
        (1, "obj/", "编译中间文件（自动生成，可忽略）", "#999999", False, True),
    ]
    y = 4.7
    for indent, name, desc, color, bold, is_dir in tree:
        x = 0.6 + indent * 0.9
        # 小色块标记：文件夹实心，文件空心
        mk = FancyBboxPatch((x, y - 0.14), 0.28, 0.28,
                            boxstyle="round,pad=0.005,rounding_size=0.03",
                            linewidth=1.6, edgecolor=color,
                            facecolor=color if is_dir else "#ffffff", zorder=3)
        ax.add_patch(mk)
        ax.text(x + 0.42, y, name, fontsize=11.5, va="center",
                color=color, fontweight="bold" if bold else "normal")
        if desc:
            ax.text(5.3, y, "← " + desc, fontsize=10, va="center", color="#444")
        y -= 0.56
    # 竖分隔线
    ax.plot([5.15, 5.15], [0.3, 4.95], color="#cccccc", lw=1, ls="--")
    return save(fig, "fig_ch3_structure.png")


def fig_run_flow():
    fig, ax = _new_ax(11.5, 3.4)
    ax.set_xlim(0, 11.5); ax.set_ylim(0, 3.4)
    ax.text(5.75, 3.15, "从创建到运行的全流程", ha="center",
            fontsize=14, fontweight="bold", color="#0E4C92")
    steps = [
        ("dotnet new web", "创建项目", DZ["blue"], DZ["blue_e"]),
        ("dotnet run", "编译并启动", DZ["orange"], DZ["orange_e"]),
        ("Kestrel 监听\nhttp://localhost:5xxx", "服务器就绪", DZ["purple"], DZ["purple_e"]),
        ("浏览器访问", "看到输出 ✓", DZ["green"], DZ["green_e"]),
    ]
    n = len(steps); w = 2.3; gap = (11.5 - n * w) / (n + 1)
    y = 1.3; xs = []
    for i, (cmd, desc, fc, ec) in enumerate(steps):
        x = gap + i * (w + gap); xs.append(x)
        _box(ax, x, y, w, 1.05, cmd, fc, ec, fs=9.5, bold=True)
        ax.text(x + w / 2, y - 0.32, desc, ha="center", fontsize=9, color="#555")
    for i in range(n - 1):
        _arrow(ax, xs[i] + w, y + 0.52, xs[i + 1], y + 0.52, color="#C0392B", lw=2.0)
    return save(fig, "fig_ch3_run_flow.png")


# ============================ 附录 A ============================
def fig_frontend_methods():
    fig, ax = _new_ax(11, 6.4)
    ax.set_xlim(0, 11); ax.set_ylim(0, 6.4)
    ax.text(5.5, 6.15, "前端调用后端 API 的各种方法", ha="center",
            fontsize=15, fontweight="bold", color="#0E4C92")
    groups = [
        ("原生 JavaScript", ["fetch()", "XMLHttpRequest"], DZ["blue"], DZ["blue_e"]),
        ("第三方库", ["axios", "jQuery.ajax()"], DZ["green"], DZ["green_e"]),
        ("前端框架", ["Vue", "React", "Angular HttpClient"], DZ["orange"], DZ["orange_e"]),
        ("原生 HTML", ["<form> 表单提交"], DZ["yellow"], DZ["yellow_e"]),
        ("实时通信", ["EventSource (SSE)", "WebSocket"], DZ["purple"], DZ["purple_e"]),
        ("小程序 / 服务端", ["wx.request (小程序)", "Node.js / C# HttpClient"], DZ["gray"], DZ["gray_e"]),
    ]
    # 2 列 x 3 行
    col_w = 5.1; row_h = 1.75
    for idx, (title, items, fc, ec) in enumerate(groups):
        col = idx % 2; row = idx // 2
        x = 0.4 + col * (col_w + 0.4)
        y = 4.35 - row * (row_h + 0.05)
        _box(ax, x, y, col_w, row_h, "", fc, ec, rounding=0.03, lw=2.0)
        ax.text(x + 0.25, y + row_h - 0.32, title, fontsize=11.5,
                fontweight="bold", color=ec, va="center")
        for j, it in enumerate(items):
            ax.text(x + 0.45, y + row_h - 0.75 - j * 0.4, "• " + it,
                    fontsize=10, color="#333", va="center")
    return save(fig, "fig_appx_methods.png")


if __name__ == "__main__":
    funcs = [fig_pipeline_journey, fig_builder_app, fig_middleware_onion, fig_host,
             fig_sdk_runtime, fig_project_structure, fig_run_flow, fig_frontend_methods]
    for f in funcs:
        print("saved:", f())
