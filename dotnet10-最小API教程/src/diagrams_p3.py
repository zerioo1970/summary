# -*- coding: utf-8 -*-
"""第三批示意图：第 4~20 章。输出到顶层 images/ 目录。"""
import os
import docbuilder as db
from docbuilder import _box, _arrow, save, DZ
import matplotlib.pyplot as plt

# 输出到顶层 images/（markdown 以 ../images 引用）
db.IMG_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "images"))
os.makedirs(db.IMG_DIR, exist_ok=True)


def ax_(w, h):
    fig, ax = plt.subplots(figsize=(w, h))
    ax.set_xlim(0, w); ax.set_ylim(0, h); ax.axis("off")
    return fig, ax


def title(ax, x, y, t):
    ax.text(x, y, t, ha="center", fontsize=15, fontweight="bold", color="#0E4C92")


# ---------- 第4章 路由 ----------
def fig_ch4_routing():
    fig, ax = ax_(11, 5.2); title(ax, 5.5, 4.9, "路由：HTTP 方法 + URL → 端点")
    methods = [("MapGet", "查询", DZ["blue"], DZ["blue_e"]),
               ("MapPost", "新增", DZ["green"], DZ["green_e"]),
               ("MapPut", "更新", DZ["orange"], DZ["orange_e"]),
               ("MapDelete", "删除", DZ["red"], DZ["red_e"])]
    for i, (m, d, fc, ec) in enumerate(methods):
        x = 0.5 + i * 2.6
        _box(ax, x, 3.1, 2.3, 1.0, m, fc, ec, fs=12, bold=True)
        ax.text(x + 1.15, 2.75, d, ha="center", fontsize=10, color="#555")
    ax.text(5.5, 1.9, 'app.MapGet("/api/products/{id}", (int id) => ...)',
            ha="center", fontsize=12, color="#1E1E1E",
            family=db.CN_NAME)
    _box(ax, 3.0, 0.6, 5.0, 0.85, "{id} 是路由参数，会自动传给处理函数", "#F4F5F7", "#C9CDD4", fs=10)
    return save(fig, "fig_ch4_routing.png")


# ---------- 第5章 参数绑定 ----------
def fig_ch5_binding():
    fig, ax = ax_(11, 6.6); title(ax, 5.5, 6.35, "参数从哪来：6 种绑定来源")
    _box(ax, 4.0, 2.9, 3.0, 1.1, "端点处理函数\n的参数", DZ["green"], DZ["green_e"], fs=12, bold=True)
    sources = [
        ("路由 Route", "/{id}", DZ["blue"], 0.5, 4.9),
        ("查询串 Query", "?name=x", DZ["blue"], 4.0, 4.9),
        ("请求体 Body", "JSON", DZ["orange"], 7.5, 4.9),
        ("请求头 Header", "X-Token", DZ["purple"], 0.5, 0.6),
        ("表单 Form", "form-data", DZ["yellow"], 4.0, 0.6),
        ("服务 DI", "AddXxx()", DZ["gray"], 7.5, 0.6),
    ]
    for name, ex, fc, x, y in sources:
        ec = {DZ["blue"]:DZ["blue_e"],DZ["orange"]:DZ["orange_e"],DZ["purple"]:DZ["purple_e"],
              DZ["yellow"]:DZ["yellow_e"],DZ["gray"]:DZ["gray_e"]}[fc]
        _box(ax, x, y, 3.0, 0.95, f"{name}\n{ex}", fc, ec, fs=10, bold=True)
        top = y > 3
        _arrow(ax, x + 1.5, y + (0.0 if top else 0.95), 5.5, 4.0 if top else 2.9,
               color="#888", lw=1.5)
    return save(fig, "fig_ch5_binding.png")


# ---------- 第6章 返回结果 ----------
def fig_ch6_results():
    fig, ax = ax_(11, 5.0); title(ax, 5.5, 4.7, "TypedResults：常用返回结果与状态码")
    items = [("Ok(x)", "200 成功", DZ["green"], DZ["green_e"]),
             ("Created(...)", "201 已创建", DZ["green"], DZ["green_e"]),
             ("NoContent()", "204 无内容", DZ["blue"], DZ["blue_e"]),
             ("NotFound()", "404 未找到", DZ["orange"], DZ["orange_e"]),
             ("BadRequest(x)", "400 请求错误", DZ["red"], DZ["red_e"]),
             ("Unauthorized()", "401 未授权", DZ["red"], DZ["red_e"]),
             ("Problem(...)", "500 服务器错误", DZ["purple"], DZ["purple_e"]),
             ("Redirect(url)", "302 重定向", DZ["yellow"], DZ["yellow_e"])]
    for i, (m, d, fc, ec) in enumerate(items):
        col = i % 4; row = i // 4
        x = 0.4 + col * 2.65; y = 2.6 - row * 1.5
        _box(ax, x, y, 2.4, 1.0, f"{m}\n{d}", fc, ec, fs=10.5, bold=True)
    return save(fig, "fig_ch6_results.png")


# ---------- 第7章 DI 生命周期 ----------
def fig_ch7_di():
    fig, ax = ax_(11, 5.4); title(ax, 5.5, 5.1, "依赖注入的三种生命周期")
    data = [("Singleton\n单例", "整个程序共用一个实例", DZ["blue"], DZ["blue_e"]),
            ("Scoped\n作用域", "每个请求一个实例", DZ["green"], DZ["green_e"]),
            ("Transient\n瞬时", "每次获取都新建", DZ["orange"], DZ["orange_e"])]
    for i, (n, d, fc, ec) in enumerate(data):
        x = 0.5 + i * 3.5
        _box(ax, x, 2.6, 3.0, 1.5, n, fc, ec, fs=13, bold=True)
        ax.text(x + 1.5, 2.2, d, ha="center", fontsize=10.5, color="#333")
        ax.text(x + 1.5, 1.5, f"builder.Services.Add{n.split(chr(10))[0]}<T>()",
                ha="center", fontsize=8.5, color="#666", family=db.CN_NAME)
    _box(ax, 1.5, 0.4, 8.0, 0.7,
         "选择原则：无状态且可共享→Singleton；每请求隔离(如数据库上下文)→Scoped；轻量易变→Transient",
         "#FFF7E0", "#C9A227", fs=9.5)
    return save(fig, "fig_ch7_di.png")


# ---------- 第8章 校验 ----------
def fig_ch8_validation():
    fig, ax = ax_(11, 3.8); title(ax, 5.5, 3.5, "请求校验流程（.NET 10 内置）")
    _box(ax, 0.4, 1.6, 2.2, 1.0, "收到请求\n(带数据)", DZ["blue"], DZ["blue_e"], fs=11, bold=True)
    _box(ax, 3.4, 1.6, 2.2, 1.0, "按特性校验\n[Required] 等", DZ["purple"], DZ["purple_e"], fs=11, bold=True)
    _box(ax, 6.6, 2.4, 2.0, 0.9, "通过 → 执行端点", DZ["green"], DZ["green_e"], fs=10.5, bold=True)
    _box(ax, 6.6, 0.7, 2.0, 0.9, "失败 → 400\nProblemDetails", DZ["red"], DZ["red_e"], fs=10, bold=True)
    _arrow(ax, 2.6, 2.1, 3.4, 2.1, color="#888")
    _arrow(ax, 5.6, 2.2, 6.6, 2.75, color="#2E9E5B")
    _arrow(ax, 5.6, 2.0, 6.6, 1.1, color="#C0392B")
    return save(fig, "fig_ch8_validation.png")


# ---------- 第9章 EF Core ----------
def fig_ch9_efcore():
    fig, ax = ax_(11, 4.2); title(ax, 5.5, 3.9, "EF Core：用 C# 对象操作数据库")
    _box(ax, 0.5, 1.3, 2.6, 1.4, "C# 实体类\nProduct", DZ["blue"], DZ["blue_e"], fs=11, bold=True)
    _box(ax, 4.2, 1.3, 2.6, 1.4, "DbContext\n(中间翻译官)", DZ["purple"], DZ["purple_e"], fs=11, bold=True)
    _box(ax, 7.9, 1.3, 2.6, 1.4, "数据库表\nProducts", DZ["orange"], DZ["orange_e"], fs=11, bold=True)
    _arrow(ax, 3.1, 2.2, 4.2, 2.2, color="#888", lw=2)
    _arrow(ax, 4.2, 1.6, 3.1, 1.6, color="#888", lw=2)
    _arrow(ax, 6.8, 2.2, 7.9, 2.2, color="#888", lw=2)
    _arrow(ax, 7.9, 1.6, 6.8, 1.6, color="#888", lw=2)
    ax.text(5.5, 0.6, "你写 LINQ，EF Core 自动翻译成 SQL 执行", ha="center", fontsize=10, color="#555")
    return save(fig, "fig_ch9_efcore.png")


# ---------- 第10章 OpenAPI ----------
def fig_ch10_openapi():
    fig, ax = ax_(11, 4.0); title(ax, 5.5, 3.7, "OpenAPI：从代码自动生成接口文档")
    steps = [("你的最小 API\n代码", DZ["green"], DZ["green_e"]),
             ("OpenAPI 3.1\nJSON 文档", DZ["blue"], DZ["blue_e"]),
             ("Swagger UI /\nScalar 可视化", DZ["purple"], DZ["purple_e"])]
    xs = [0.6, 4.2, 7.8]
    for (t, fc, ec), x in zip(steps, xs):
        _box(ax, x, 1.5, 2.6, 1.3, t, fc, ec, fs=11, bold=True)
    _arrow(ax, 3.2, 2.15, 4.2, 2.15, color="#888", lw=2)
    _arrow(ax, 6.8, 2.15, 7.8, 2.15, color="#888", lw=2)
    ax.text(5.5, 0.8, "开启只需：builder.Services.AddOpenApi(); app.MapOpenApi();",
            ha="center", fontsize=9.5, color="#555", family=db.CN_NAME)
    return save(fig, "fig_ch10_openapi.png")


# ---------- 第11章 认证授权 ----------
def fig_ch11_auth():
    fig, ax = ax_(11, 4.2); title(ax, 5.5, 3.9, "认证 vs 授权")
    _box(ax, 0.6, 1.8, 4.2, 1.4, "认证 Authentication\n“你是谁？”\n验证身份(如 JWT 令牌)",
         DZ["blue"], DZ["blue_e"], fs=11, bold=True)
    _box(ax, 5.6, 1.8, 4.2, 1.4, "授权 Authorization\n“你能做什么？”\n检查权限/角色/策略",
         DZ["green"], DZ["green_e"], fs=11, bold=True)
    _arrow(ax, 4.8, 2.5, 5.6, 2.5, color="#C0392B", lw=2.2)
    ax.text(5.5, 1.0, "先认证、后授权——顺序不能反", ha="center", fontsize=10.5, color="#555")
    return save(fig, "fig_ch11_auth.png")


# ---------- 第12章 端点过滤器执行顺序 ----------
def fig_ch12_filter():
    fig, ax = ax_(9, 5.6); title(ax, 4.5, 5.3, "端点过滤器的执行顺序")
    layers = [("过滤器 A", DZ["blue"], DZ["blue_e"], 7.0, 3.8),
              ("过滤器 B", DZ["purple"], DZ["purple_e"], 5.4, 2.9)]
    cx = 4.5
    for name, fc, ec, w, h in layers:
        _box(ax, cx - w/2, 2.6 - h/2, w, h, "", fc, ec, lw=1.8, rounding=0.04)
        ax.text(cx, 2.6 + h/2 - 0.3, name, ha="center", fontsize=11, fontweight="bold", color=ec)
    _box(ax, cx - 1.0, 2.1, 2.0, 1.0, "端点\n(你的代码)", DZ["green"], DZ["green_e"], fs=10, bold=True)
    _arrow(ax, 0.3, 3.3, 1.3, 2.9, color="#C0392B", lw=2); ax.text(0.6, 3.55, "请求", fontsize=9.5, color="#C0392B")
    _arrow(ax, 1.3, 2.1, 0.3, 1.7, color="#2E9E5B", lw=2); ax.text(0.6, 1.4, "响应", fontsize=9.5, color="#2E9E5B")
    ax.text(4.5, 0.4, "A 前 → B 前 → 端点 → B 后 → A 后", ha="center", fontsize=10, color="#555")
    return save(fig, "fig_ch12_filter.png")


# ---------- 第13章 SSE ----------
def fig_ch13_sse():
    fig, ax = ax_(11, 3.6); title(ax, 5.5, 3.3, "Server-Sent Events：服务器持续推送")
    _box(ax, 0.5, 1.3, 2.6, 1.2, "前端\nEventSource", DZ["blue"], DZ["blue_e"], fs=11, bold=True)
    _box(ax, 7.9, 1.3, 2.6, 1.2, "后端最小 API\nSSE 端点", DZ["green"], DZ["green_e"], fs=11, bold=True)
    for i in range(3):
        y = 2.3 - i * 0.5
        _arrow(ax, 7.9, y, 3.1, y, color="#2E9E5B", lw=1.8)
    ax.text(5.5, 2.55, "事件1 → 事件2 → 事件3 …（一条连接持续推）", ha="center", fontsize=9.5, color="#2E9E5B")
    ax.text(5.5, 0.7, "适合：股价、通知、进度、日志流等单向实时场景", ha="center", fontsize=10, color="#555")
    return save(fig, "fig_ch13_sse.png")


# ---------- 第14章 配置来源优先级 ----------
def fig_ch14_config():
    fig, ax = ax_(9, 5.2); title(ax, 4.5, 4.9, "配置来源优先级（上层覆盖下层）")
    layers = [("命令行参数", DZ["red"], DZ["red_e"]),
              ("环境变量", DZ["orange"], DZ["orange_e"]),
              ("用户机密 (开发)", DZ["purple"], DZ["purple_e"]),
              ("appsettings.{环境}.json", DZ["green"], DZ["green_e"]),
              ("appsettings.json", DZ["blue"], DZ["blue_e"])]
    for i, (t, fc, ec) in enumerate(layers):
        y = 0.5 + i * 0.82
        w = 5.0 + i * 0.6
        _box(ax, 4.5 - w/2, y, w, 0.66, t, fc, ec, fs=10.5, bold=True)
    _arrow(ax, 8.2, 0.8, 8.2, 4.2, color="#333", lw=2)
    ax.text(8.55, 2.5, "优先级升高", rotation=90, va="center", fontsize=10, color="#333")
    return save(fig, "fig_ch14_config.png")


# ---------- 第15章 限流 ----------
def fig_ch15_ratelimit():
    fig, ax = ax_(11, 3.6); title(ax, 5.5, 3.3, "限流 / CORS / 健康检查")
    boxes = [("CORS", "控制哪些前端来源可访问", DZ["blue"], DZ["blue_e"]),
             ("Rate Limiting", "限制单位时间请求次数", DZ["orange"], DZ["orange_e"]),
             ("Health Checks", "/health 检查服务是否健康", DZ["green"], DZ["green_e"])]
    for i, (n, d, fc, ec) in enumerate(boxes):
        x = 0.4 + i * 3.6
        _box(ax, x, 1.5, 3.3, 1.1, n, fc, ec, fs=12, bold=True)
        ax.text(x + 1.65, 1.1, d, ha="center", fontsize=9, color="#555")
    return save(fig, "fig_ch15_ratelimit.png")


# ---------- 第16章 版本控制 ----------
def fig_ch16_version():
    fig, ax = ax_(11, 3.4); title(ax, 5.5, 3.1, "API 版本控制")
    _box(ax, 0.6, 1.4, 3.0, 1.1, "客户端", DZ["blue"], DZ["blue_e"], fs=12, bold=True)
    _box(ax, 7.4, 2.1, 3.0, 0.85, "/v1/products", DZ["green"], DZ["green_e"], fs=11, bold=True)
    _box(ax, 7.4, 0.9, 3.0, 0.85, "/v2/products", DZ["orange"], DZ["orange_e"], fs=11, bold=True)
    _arrow(ax, 3.6, 2.1, 7.4, 2.5, color="#888", lw=1.8)
    _arrow(ax, 3.6, 1.8, 7.4, 1.3, color="#888", lw=1.8)
    ax.text(5.5, 0.4, "新版本上线时，老版本仍可用，平滑过渡", ha="center", fontsize=10, color="#555")
    return save(fig, "fig_ch16_version.png")


# ---------- 第17章 测试金字塔 ----------
def fig_ch17_testpyramid():
    fig, ax = ax_(8, 5.0); title(ax, 4.0, 4.7, "测试金字塔")
    import matplotlib.patches as mp
    tri = [(("集成测试", DZ["orange_e"], 3.2, 1.6, 0.9)),
           (("单元测试", DZ["green_e"], 5.6, 0.5, 1.0))]
    # 画两层梯形/矩形
    _box(ax, 2.6, 2.7, 2.8, 1.0, "集成测试\nWebApplicationFactory", DZ["orange"], DZ["orange_e"], fs=10, bold=True)
    _box(ax, 1.6, 1.2, 4.8, 1.2, "单元测试\n(测端点里的纯逻辑，多而快)", DZ["green"], DZ["green_e"], fs=10.5, bold=True)
    ax.text(4.0, 0.6, "下层多而快，上层少而全", ha="center", fontsize=10, color="#555")
    return save(fig, "fig_ch17_testpyramid.png")


# ---------- 第18章 垂直切片 ----------
def fig_ch18_slice():
    fig, ax = ax_(11, 4.4); title(ax, 5.5, 4.1, "组织方式：按功能垂直切片")
    feats = ["Products", "Orders", "Users"]
    for i, f in enumerate(feats):
        x = 0.6 + i * 3.5
        _box(ax, x, 0.8, 3.0, 2.7, "", DZ["blue"], DZ["blue_e"], rounding=0.03, lw=2)
        ax.text(x + 1.5, 3.2, f, ha="center", fontsize=12, fontweight="bold", color="#1b5e91")
        for j, part in enumerate(["端点 Endpoints", "业务逻辑", "数据访问"]):
            _box(ax, x + 0.3, 2.4 - j * 0.62, 2.4, 0.5, part, "#ffffff", DZ["blue_e"], fs=9.5)
    ax.text(5.5, 0.35, "每个功能自成一体，改一个功能不影响其它", ha="center", fontsize=10, color="#555")
    return save(fig, "fig_ch18_slice.png")


# ---------- 第19章 部署 ----------
def fig_ch19_deploy():
    fig, ax = ax_(11, 3.8); title(ax, 5.5, 3.5, "部署方式与原生 AoT")
    boxes = [("依赖框架部署", "服务器需装 .NET 运行时", DZ["blue"], DZ["blue_e"]),
             ("自包含部署", "带上运行时，独立运行", DZ["green"], DZ["green_e"]),
             ("原生 AoT", "编译成原生可执行文件\n启动最快、体积最小", DZ["purple"], DZ["purple_e"]),
             ("Docker 容器", "打包成镜像，随处运行", DZ["orange"], DZ["orange_e"])]
    for i, (n, d, fc, ec) in enumerate(boxes):
        col = i % 2; row = i // 2
        x = 0.6 + col * 5.0; y = 1.9 - row * 1.4
        _box(ax, x, y, 4.6, 1.1, n, fc, ec, fs=11.5, bold=True)
        ax.text(x + 2.3, y - 0.25, d, ha="center", fontsize=8.5, color="#555")
    return save(fig, "fig_ch19_deploy.png")


# ---------- 第20章 综合架构 ----------
def fig_ch20_arch():
    fig, ax = ax_(11, 4.6); title(ax, 5.5, 4.3, "综合实战：完整项目分层")
    layers = [("前端 / 客户端", DZ["blue"], DZ["blue_e"]),
              ("最小 API 端点层 (路由+校验+返回)", DZ["green"], DZ["green_e"]),
              ("业务服务层 (依赖注入)", DZ["purple"], DZ["purple_e"]),
              ("EF Core 数据访问层", DZ["orange"], DZ["orange_e"]),
              ("数据库", DZ["gray"], DZ["gray_e"])]
    for i, (t, fc, ec) in enumerate(layers):
        y = 3.4 - i * 0.72
        _box(ax, 1.5, y, 8.0, 0.6, t, fc, ec, fs=11, bold=True)
        if i < len(layers) - 1:
            _arrow(ax, 5.5, y, 5.5, y - 0.12, color="#888", lw=1.5)
    return save(fig, "fig_ch20_arch.png")


if __name__ == "__main__":
    funcs = [fig_ch4_routing, fig_ch5_binding, fig_ch6_results, fig_ch7_di,
             fig_ch8_validation, fig_ch9_efcore, fig_ch10_openapi, fig_ch11_auth,
             fig_ch12_filter, fig_ch13_sse, fig_ch14_config, fig_ch15_ratelimit,
             fig_ch16_version, fig_ch17_testpyramid, fig_ch18_slice, fig_ch19_deploy,
             fig_ch20_arch]
    for f in funcs:
        print("saved:", os.path.basename(f()))
