# CMC办公助手：企业微信自建 H5 应用认证与 ASP.NET Core 8 WebApi 实现方案

> 版本说明：本文面向 **ASP.NET Core 8（.NET 8）**。
> 目标服务器实测环境：`.NET SDK 8.0.408` + `ASP.NET Core Runtime 8.0.15`，Windows Server 2016（win-x64）。
> 数据访问采用 **ADO.NET（`Microsoft.Data.SqlClient`）**，数据库为 SQL Server。
>
> 注意：Windows 上常见的 `.NET Framework 4.0.30319 / ASP.NET 4.8.x` 版本号是系统自带的旧 System.Web 组件，与本应用无关。本应用运行时以 `dotnet --info` 中的 **ASP.NET Core 8.0.15** 为准。

---

## 目录

1. [项目背景](#1-项目背景)
2. [总体设计思想（三层认证）](#2-总体设计思想三层认证)
3. [网络架构](#3-网络架构)
4. [企业微信后台配置](#4-企业微信后台配置)
5. [后端配置](#5-后端配置)
6. [Program.cs 启动配置（含关键修正）](#6-programcs-启动配置含关键修正)
7. [企业微信接口返回模型](#7-企业微信接口返回模型)
8. [企业微信服务封装（含缓存加锁）](#8-企业微信服务封装含缓存加锁)
9. [员工服务（ADO.NET 实现）](#9-员工服务adonet-实现)
10. [登录控制器 AuthController](#10-登录控制器-authcontroller)
11. [当前登录用户接口](#11-当前登录用户接口)
12. [前端 H5 登录逻辑](#12-前端-h5-登录逻辑)
13. [考勤接口示例](#13-考勤接口示例)
14. [年假接口示例](#14-年假接口示例)
15. [Nginx 反向代理配置](#15-nginx-反向代理配置)
16. [Cookie 与安全设置](#16-cookie-与安全设置)
17. [access_token 缓存策略](#17-access_token-缓存策略)
18. [日志设计建议](#18-日志设计建议)
19. [常见问题排查](#19-常见问题排查)
20. [推荐目录结构](#20-推荐目录结构)
21. [最小可运行实现步骤](#21-最小可运行实现步骤)
22. [关键原则总结](#22-关键原则总结)
23. [附：相对原始方案的修正清单](#23-附相对原始方案的修正清单)

---

## 1. 项目背景

本项目是一个基于企业微信自建应用的内部办公系统，名称为 **CMC办公助手**。

系统功能：

- 手机端查询考勤
- 手机端查询年假余额
- 手机端提交年假申请
- 通过企业微信认证用户身份
- 通过手机号匹配公司 HR 员工主数据
- 使用 ASP.NET Core 加密 Cookie 维持登录状态

当前访问地址：

```text
https://bc.canon-medical.com.cn/CmcWorkEasy/
```

系统形态：

```text
企业微信自建应用
H5 网页应用（不是小程序，不需要腾讯审核）
前端：HTML5
后端：ASP.NET Core 8 WebApi
数据访问：ADO.NET（Microsoft.Data.SqlClient）
数据库：SQL Server
```

---

## 2. 总体设计思想（三层认证）

本系统的本质不是让企业微信直接管理业务系统登录，而是：

> 企业微信负责证明"当前手机端用户是谁"，CMC办公助手负责判断"这个人是不是公司在职员工，并授予业务系统访问权限"。

认证分为三层：

```text
第一层：企业微信 OAuth 认证
  证明用户来自企业微信环境，并且用户同意授权。

第二层：HR 员工主数据校验
  用企业微信返回的手机号查询 HRPersonalInfoTable，
  判断这个手机号是否对应公司在职员工（排除 [在职状态]='离职'）。

第三层：ASP.NET Core Cookie 登录
  匹配成功后，由 CMC办公助手生成自己的加密认证 Cookie，
  后续业务接口不再每次调用企业微信。
```

两套身份系统协作：

| 层 | 身份凭证 | 谁签发 | 作用 |
|---|---|---|---|
| 企业微信侧 | `code`、`access_token`、`user_ticket` | 企业微信服务器 | 证明"这是本企业某位微信用户" |
| 你的应用侧 | ASP.NET Core 加密 Cookie（Claims） | 你的后端 | 证明"这是 HR 库里某位在职员工" |

整体流程：

```text
员工手机（企业微信内置浏览器）
  ↓ 打开 https://bc.canon-medical.com.cn/CmcWorkEasy/
  ↓ 未登录 → 前端 fetch /api/account/me 得到 401 → 跳转 /CmcWorkEasy/auth/login
  ↓ 后端生成企业微信 OAuth 授权地址（302 跳转）
  ↓ 用户在企业微信里点"同意"
  ↓ 企业微信回调 /CmcWorkEasy/auth/callback?code=xxx&state=yyy
  ↓ 后端校验 state
  ↓ 后端用 CorpId + Secret 换 access_token（带缓存）
  ↓ 后端用 access_token + code 调 getuserinfo → 拿 userid / user_ticket
  ↓ 后端用 user_ticket 调 getuserdetail → 拿手机号 mobile
  ↓ 后端用 mobile 查 HRPersonalInfoTable，排除 [在职状态]='离职'
  ↓ 匹配成功 → 写入 Claims → 生成加密认证 Cookie
  ↓ 跳回 H5 首页
  ↓ 前端调用考勤、年假等业务 API（自动带 Cookie，[Authorize] 校验）
```

---

## 3. 网络架构

```text
员工手机（企业微信内置浏览器）
  ↓ HTTPS
bc.canon-medical.com.cn（公司 HTTPS 域名 = 企业微信可信域名）
  ↓ Nginx 反向代理
192.168.105.14:5187（ASP.NET Core 8 监听地址）
  ↓ HTTPS（出口公网 IP 必须 = 124.127.244.155 = 企业微信可信IP）
qyapi.weixin.qq.com（企业微信服务端 API）
```

| 节点 | 说明 |
|---|---|
| 员工手机 | 通过企业微信内置浏览器打开 H5 应用 |
| bc.canon-medical.com.cn | 公司 HTTPS 域名，也是企业微信应用可信域名 |
| Nginx | 对外提供 HTTPS，向内转发到 ASP.NET Core |
| 192.168.105.14:5187 | ASP.NET Core WebApi 实际监听地址 |
| 124.127.244.155 | 后端访问企业微信 API 时的公网出口 IP（可信 IP） |
| qyapi.weixin.qq.com | 企业微信服务端 API 地址 |

---

## 4. 企业微信后台配置

### 4.1 自建应用类型

```text
应用名称：CMC办公助手
应用类型：自建应用
访问方式：H5 网页（不是小程序，不需要腾讯审核）
```

### 4.2 应用可信域名

```text
bc.canon-medical.com.cn
```

注意：

1. OAuth 回调地址必须使用这个域名。
2. H5 页面必须从这个域名加载。
3. 不要使用内网 IP、localhost 或其他测试域名作为正式回调地址。
4. 企业微信校验的是域名，不是完整路径。

正确示例：

```text
https://bc.canon-medical.com.cn/CmcWorkEasy/auth/callback
```

错误示例：

```text
http://192.168.105.14:5187/CmcWorkEasy/auth/callback
http://localhost:5187/CmcWorkEasy/auth/callback
https://other-domain.example.com/CmcWorkEasy/auth/callback
```

### 4.3 企业可信 IP

```text
124.127.244.155
```

这里的 IP 必须是**后端服务器访问企业微信 API 时，企业微信服务器看到的公网出口 IP**。

它不是：

```text
员工手机 IP
内网服务器 IP：192.168.105.14
Nginx 内网 IP
```

如果公司网络经过 NAT、防火墙、代理服务器，则应填写 NAT 出口 IP。调用 `gettoken`、`getuserdetail` 等敏感接口时都会校验此 IP。

### 4.4 CorpId / AgentId / Secret

| 参数 | 含义 |
|---|---|
| CorpId | 企业 ID，标识整个企业微信组织 |
| AgentId | 自建应用 ID，标识 CMC办公助手这个应用 |
| Secret | 自建应用密钥，后端用它换取 access_token |

> **Secret 只能保存在后端服务器。** 前端 HTML、JavaScript、浏览器缓存、接口返回值里都不能出现 Secret。

### 4.5 应用可见范围

```text
可见范围：全体员工 或 指定部门/指定人员
```

如果员工不在应用可见范围内，即使打开链接，也可能无法正常完成授权（`getuserinfo` 拿不到 userid）。

### 4.6 敏感字段权限（手机号）

要获取手机号，必须确保应用被允许获取成员手机号等敏感字段，否则会出现"能拿到 userid，但拿不到 mobile"。

企业微信提供两条获取手机号的路径，二选一：

| 路径 | scope | 用户体验 | 前提 |
|---|---|---|---|
| A（本文默认） | `snsapi_privateinfo` | 会弹出授权页，需用户点"同意" | 应用已开启手机号敏感字段权限 |
| B（备选） | `snsapi_base`（静默拿 userid）再调 `/cgi-bin/user/get` 读 mobile | 无感知 | 应用有"读取成员"权限且手机号在可见字段内 |

本文采用路径 A（更贴合"用户授权同意后获取手机号"的隐私合规流程）。

---

## 5. 后端配置

### 5.1 项目文件（.csproj）

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>CmcWorkEasy</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <!-- ADO.NET 首选提供程序（不要用已淘汰的 System.Data.SqlClient） -->
    <PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />
  </ItemGroup>
</Project>
```

> `IMemoryCache`、`IHttpClientFactory`、Cookie 认证在 ASP.NET Core 8 内置，无需额外包。

### 5.2 appsettings.json 示例

```json
{
  "WeCom": {
    "CorpId": "wwxxxxxxxxxxxxxxxx",
    "AgentId": "1000002",
    "Secret": "不要把正式 Secret 明文提交到 Git",
    "CallbackUrl": "https://bc.canon-medical.com.cn/CmcWorkEasy/auth/callback"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=数据库服务器;Database=sankodata;User Id=用户名;Password=密码;Encrypt=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" }
  }
}
```

### 5.3 正式环境 Secret 的保存方式

正式环境不建议把 Secret 明文写在 `appsettings.json`。建议使用：

```text
Windows 环境变量 / IIS 应用程序环境变量
Docker Secret / Kubernetes Secret
公司统一密钥管理系统
```

ASP.NET Core 会自动把环境变量 `WeCom__Secret` 映射到配置键 `WeCom:Secret`：

```powershell
setx WeCom__Secret "企业微信应用Secret" /M
```

### 5.4 配置类

```csharp
namespace CmcWorkEasy.Options;

/// <summary>企业微信自建应用配置（仅后端使用）。</summary>
public class WeComOptions
{
    public string CorpId { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
    /// <summary>自建应用 Secret，只能后端使用，不能暴露给前端。</summary>
    public string Secret { get; set; } = string.Empty;
    /// <summary>OAuth 回调地址，必须使用企业微信后台配置的可信域名。</summary>
    public string CallbackUrl { get; set; } = string.Empty;
}
```

---

## 6. Program.cs 启动配置（含关键修正）

> **关键修正 1**：为 API 路径重写 `OnRedirectToLogin`，未登录时返回 **401** 而不是 302 跳转，否则前端 `fetch` 会跟随 302 拿到登录页 HTML，永远判断不出未登录。
> **关键修正 2**：`UseForwardedHeaders` 显式指定 `KnownProxies`，不要清空信任列表。

```csharp
using CmcWorkEasy.Options;
using CmcWorkEasy.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// 监听地址（也可在 launchSettings / 环境变量里配置）
builder.WebHost.UseUrls("http://0.0.0.0:5187");

builder.Services.Configure<WeComOptions>(builder.Configuration.GetSection("WeCom"));
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// 后端调用企业微信接口用的 HttpClient（工厂管理，避免 socket 耗尽）
builder.Services.AddHttpClient<IWeComService, WeComService>(c =>
{
    c.Timeout = TimeSpan.FromSeconds(15);
});

// 查询 HRPersonalInfoTable
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "CmcWorkEasy.Auth";
        options.Cookie.HttpOnly = true;                       // JS 读不到
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // 仅 HTTPS
        options.Cookie.SameSite = SameSiteMode.Lax;           // 同时挡住跨站 POST 的 CSRF
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.LoginPath = "/CmcWorkEasy/auth/login";
        options.AccessDeniedPath = "/CmcWorkEasy/auth/denied";

        // ★ 修正：/api 未登录返回 401，其余才 302 跳登录
        options.Events.OnRedirectToLogin = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/CmcWorkEasy/api"))
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            else
                ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/CmcWorkEasy/api"))
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            else
                ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ★ 修正：反向代理转发头，显式信任 Nginx 内网 IP（不要清空 KnownProxies）
var fho = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
fho.KnownProxies.Add(IPAddress.Parse("192.168.105.14")); // 换成 Nginx 实际内网 IP
app.UseForwardedHeaders(fho);

app.UseStaticFiles();     // wwwroot 下的 index.html 等
app.UseRouting();
app.UseAuthentication();  // 顺序：先认证
app.UseAuthorization();   // 再授权
app.MapControllers();

app.Run();
```

> 说明：ASP.NET Core 8 使用操作系统 TLS，**无需**手动设置 `ServicePointManager.SecurityProtocol`（那是 .NET Framework 才有的坑）。

---

## 7. 企业微信接口返回模型

```csharp
using System.Text.Json.Serialization;

namespace CmcWorkEasy.Models.WeCom;

public class AccessTokenResponse
{
    [JsonPropertyName("errcode")] public int ErrCode { get; set; }
    [JsonPropertyName("errmsg")]  public string ErrMsg { get; set; } = string.Empty;
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
    [JsonPropertyName("expires_in")]   public int ExpiresIn { get; set; }
}

public class UserInfoResponse
{
    [JsonPropertyName("errcode")] public int ErrCode { get; set; }
    [JsonPropertyName("errmsg")]  public string ErrMsg { get; set; } = string.Empty;
    /// <summary>企业微信通讯录成员 userid。</summary>
    [JsonPropertyName("userid")]      public string UserId { get; set; } = string.Empty;
    /// <summary>获取敏感信息（手机号）时需要使用的票据（scope=snsapi_privateinfo 才有）。</summary>
    [JsonPropertyName("user_ticket")] public string UserTicket { get; set; } = string.Empty;
}

public class UserDetailResponse
{
    [JsonPropertyName("errcode")] public int ErrCode { get; set; }
    [JsonPropertyName("errmsg")]  public string ErrMsg { get; set; } = string.Empty;
    [JsonPropertyName("userid")]  public string UserId { get; set; } = string.Empty;
    [JsonPropertyName("mobile")]  public string Mobile { get; set; } = string.Empty;
    [JsonPropertyName("email")]   public string Email { get; set; } = string.Empty;
}
```

---

## 8. 企业微信服务封装（含缓存加锁）

> **关键修正 3**：`access_token` 缓存加 `SemaphoreSlim` 锁 + 双重检查，避免并发首次命中时同时调用 `gettoken`（缓存击穿 + 触发频率限制）。

企业微信接口清单：

| 用途 | 方法与地址 |
|---|---|
| 获取 access_token | `GET /cgi-bin/gettoken?corpid=&corpsecret=` |
| code 换用户身份 | `GET /cgi-bin/auth/getuserinfo?access_token=&code=` |
| user_ticket 换手机号 | `POST /cgi-bin/auth/getuserdetail?access_token=`，body `{"user_ticket":"..."}` |

### 8.1 接口

```csharp
namespace CmcWorkEasy.Services;

public interface IWeComService
{
    Task<string> GetAccessTokenAsync();
    Task<(string UserId, string UserTicket)> GetUserInfoByCodeAsync(string code);
    Task<string> GetMobileByUserTicketAsync(string userTicket);
}
```

### 8.2 实现

```csharp
using System.Net.Http.Json;
using System.Text.Json;
using CmcWorkEasy.Models.WeCom;
using CmcWorkEasy.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CmcWorkEasy.Services;

public class WeComService : IWeComService
{
    private const string AccessTokenCacheKey = "WeCom:AccessToken";
    private static readonly SemaphoreSlim TokenLock = new(1, 1);

    private readonly HttpClient _http;
    private readonly WeComOptions _opt;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WeComService> _logger;

    public WeComService(HttpClient http, IOptions<WeComOptions> opt,
        IMemoryCache cache, ILogger<WeComService> logger)
    {
        _http = http;
        _opt = opt.Value;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>获取 access_token（缓存 + 加锁，提前 5 分钟过期）。</summary>
    public async Task<string> GetAccessTokenAsync()
    {
        if (_cache.TryGetValue(AccessTokenCacheKey, out string? cached)
            && !string.IsNullOrWhiteSpace(cached))
            return cached!;

        await TokenLock.WaitAsync();
        try
        {
            if (_cache.TryGetValue(AccessTokenCacheKey, out string? again)
                && !string.IsNullOrWhiteSpace(again))
                return again!; // 双重检查：等锁期间别人可能已刷新

            var url = "https://qyapi.weixin.qq.com/cgi-bin/gettoken" +
                      $"?corpid={Uri.EscapeDataString(_opt.CorpId)}" +
                      $"&corpsecret={Uri.EscapeDataString(_opt.Secret)}";

            var res = await _http.GetFromJsonAsync<AccessTokenResponse>(url)
                      ?? throw new InvalidOperationException("gettoken 接口无返回。");

            if (res.ErrCode != 0 || string.IsNullOrWhiteSpace(res.AccessToken))
            {
                _logger.LogError("gettoken 失败 errcode={C} errmsg={M}", res.ErrCode, res.ErrMsg);
                throw new InvalidOperationException(
                    $"获取 access_token 失败：{res.ErrCode} {res.ErrMsg}（检查 CorpId/Secret/可信IP=124.127.244.155）");
            }

            var seconds = Math.Max(res.ExpiresIn - 300, 60);
            _cache.Set(AccessTokenCacheKey, res.AccessToken, TimeSpan.FromSeconds(seconds));
            return res.AccessToken;
        }
        finally { TokenLock.Release(); }
    }

    /// <summary>用 OAuth 回调 code 换 userid + user_ticket（code 仅能用一次，约 5 分钟有效）。</summary>
    public async Task<(string UserId, string UserTicket)> GetUserInfoByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("code 不能为空。", nameof(code));

        var token = await GetAccessTokenAsync();
        var url = "https://qyapi.weixin.qq.com/cgi-bin/auth/getuserinfo" +
                  $"?access_token={Uri.EscapeDataString(token)}&code={Uri.EscapeDataString(code)}";

        var res = await _http.GetFromJsonAsync<UserInfoResponse>(url)
                  ?? throw new InvalidOperationException("getuserinfo 接口无返回。");

        if (res.ErrCode != 0)
        {
            _logger.LogError("getuserinfo 失败 errcode={C} errmsg={M}", res.ErrCode, res.ErrMsg);
            throw new InvalidOperationException($"getuserinfo 失败：{res.ErrCode} {res.ErrMsg}");
        }
        if (string.IsNullOrWhiteSpace(res.UserId))
            throw new InvalidOperationException("未取得 userid（用户可能不在应用可见范围）。");
        if (string.IsNullOrWhiteSpace(res.UserTicket))
            throw new InvalidOperationException("user_ticket 为空，无法获取手机号（确认 scope=snsapi_privateinfo 且用户已授权）。");

        return (res.UserId, res.UserTicket);
    }

    /// <summary>用 user_ticket 换手机号（user_ticket 约 1800 秒有效）。</summary>
    public async Task<string> GetMobileByUserTicketAsync(string userTicket)
    {
        if (string.IsNullOrWhiteSpace(userTicket))
            throw new ArgumentException("userTicket 不能为空。", nameof(userTicket));

        var token = await GetAccessTokenAsync();
        var url = "https://qyapi.weixin.qq.com/cgi-bin/auth/getuserdetail" +
                  $"?access_token={Uri.EscapeDataString(token)}";

        var httpResp = await _http.PostAsJsonAsync(url, new { user_ticket = userTicket });
        var text = await httpResp.Content.ReadAsStringAsync();

        if (!httpResp.IsSuccessStatusCode)
        {
            _logger.LogError("getuserdetail HTTP 失败 status={S} body={B}", httpResp.StatusCode, text);
            throw new InvalidOperationException($"getuserdetail HTTP 请求失败：{httpResp.StatusCode}");
        }

        var res = JsonSerializer.Deserialize<UserDetailResponse>(text)
                  ?? throw new InvalidOperationException("getuserdetail 返回无法解析。");

        if (res.ErrCode != 0)
        {
            _logger.LogError("getuserdetail 失败 errcode={C} errmsg={M}", res.ErrCode, res.ErrMsg);
            throw new InvalidOperationException($"getuserdetail 失败：{res.ErrCode} {res.ErrMsg}");
        }
        if (string.IsNullOrWhiteSpace(res.Mobile))
            throw new InvalidOperationException("未返回 mobile：请在后台开启该应用的手机号敏感字段权限。");

        return NormalizeMobile(res.Mobile);
    }

    /// <summary>规范化手机号：只保留数字并去掉国家码 86。</summary>
    public static string NormalizeMobile(string mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile)) return string.Empty;
        var digits = new string(mobile.Where(char.IsDigit).ToArray());
        if (digits.Length == 13 && digits.StartsWith("86"))
            digits = digits[2..]; // 8613800138000 -> 13800138000
        return digits;
    }
}
```

---

## 9. 员工服务（ADO.NET 实现）

> **关键修正 4**：原始方案使用 Dapper，本文按要求改为 **ADO.NET（`Microsoft.Data.SqlClient`）**，参数化查询防注入；手机号命中多条时**拒绝登录**而非取第一条。

### 9.1 实体

```csharp
namespace CmcWorkEasy.Models;

public class EmployeeLoginInfo
{
    public string LoginName { get; set; } = string.Empty;
    public string ChineseName { get; set; } = string.Empty;
    public string MobilePhone { get; set; } = string.Empty;
    public string EmployeeNo { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string EmploymentStatus { get; set; } = string.Empty;
}
```

### 9.2 接口

```csharp
using CmcWorkEasy.Models;

namespace CmcWorkEasy.Services;

public interface IEmployeeService
{
    /// <summary>根据手机号查找在职员工；不存在返回 null，重复则抛业务异常。</summary>
    Task<EmployeeLoginInfo?> FindActiveEmployeeByMobileAsync(string mobile);
}
```

### 9.3 实现（ADO.NET）

```csharp
using System.Data;
using CmcWorkEasy.Models;
using Microsoft.Data.SqlClient;

namespace CmcWorkEasy.Services;

public class EmployeeService : IEmployeeService
{
    private readonly string _connStr;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(IConfiguration cfg, ILogger<EmployeeService> logger)
    {
        _connStr = cfg.GetConnectionString("DefaultConnection")
                   ?? throw new InvalidOperationException("缺少连接字符串 DefaultConnection。");
        _logger = logger;
    }

    public async Task<EmployeeLoginInfo?> FindActiveEmployeeByMobileAsync(string mobile)
    {
        mobile = WeComService.NormalizeMobile(mobile);
        if (string.IsNullOrWhiteSpace(mobile)) return null;

        // 若已增加规范化列 NormalizedMobilePhone（推荐），改成 WHERE NormalizedMobilePhone=@Mobile 以走索引
        const string sql = @"
SELECT LoginName, ChineseName, MobilePhone, EmployeeNo, DepartmentName,
       [在职状态] AS EmploymentStatus
FROM HRPersonalInfoTable
WHERE REPLACE(REPLACE(REPLACE(ISNULL(MobilePhone,''),' ',''),'-',''),'+86','') = @Mobile
  AND ISNULL([在职状态], N'') <> N'离职';";

        var list = new List<EmployeeLoginInfo>();

        await using var conn = new SqlConnection(_connStr);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@Mobile", SqlDbType.VarChar, 20)
        {
            Direction = ParameterDirection.Input,
            Value = mobile
        });

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new EmployeeLoginInfo
            {
                LoginName        = reader["LoginName"] as string ?? string.Empty,
                ChineseName      = reader["ChineseName"] as string ?? string.Empty,
                MobilePhone      = reader["MobilePhone"] as string ?? string.Empty,
                EmployeeNo       = reader["EmployeeNo"] as string ?? string.Empty,
                DepartmentName   = reader["DepartmentName"] as string ?? string.Empty,
                EmploymentStatus = reader["EmploymentStatus"] as string ?? string.Empty
            });
        }

        if (list.Count == 0)
        {
            _logger.LogWarning("手机号未匹配到在职员工。");
            return null;
        }
        if (list.Count > 1)
        {
            _logger.LogError("HRPersonalInfoTable 存在重复手机号，Count={C}", list.Count);
            throw new InvalidOperationException("HR 员工表中存在重复手机号，请联系人事或系统管理员处理。");
        }
        return list[0];
    }
}
```

### 9.4 建议的索引优化

`WHERE REPLACE(...)` 会导致索引失效。若手机号查询频繁，建议在 HR 表增加规范化字段并建索引：

```sql
-- 方案一：增加持久化计算列并建索引
ALTER TABLE HRPersonalInfoTable
ADD NormalizedMobilePhone AS
    (REPLACE(REPLACE(REPLACE(ISNULL(MobilePhone,''),' ',''),'-',''),'+86','')) PERSISTED;

CREATE INDEX IX_HRPersonalInfoTable_NormalizedMobilePhone
ON HRPersonalInfoTable (NormalizedMobilePhone);
```

然后把查询改为 `WHERE NormalizedMobilePhone = @Mobile AND ISNULL([在职状态], N'') <> N'离职'`。

---

## 10. 登录控制器 AuthController

职责：

```text
GET  /CmcWorkEasy/auth/login      发起企业微信 OAuth（302 跳转）
GET  /CmcWorkEasy/auth/callback   接收企业微信 OAuth 回调、签发 Cookie
POST /CmcWorkEasy/auth/logout     退出登录
GET  /CmcWorkEasy/auth/denied     权限不足提示
```

```csharp
using System.Security.Claims;
using CmcWorkEasy.Options;
using CmcWorkEasy.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CmcWorkEasy.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private const string StateCookieName = "CmcWorkEasy.WxOAuth.State";

    private readonly WeComOptions _opt;
    private readonly IWeComService _weCom;
    private readonly IEmployeeService _emp;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IOptions<WeComOptions> opt, IWeComService weCom,
        IEmployeeService emp, ILogger<AuthController> logger)
    {
        _opt = opt.Value;
        _weCom = weCom;
        _emp = emp;
        _logger = logger;
    }

    /// <summary>登录入口：生成企业微信 OAuth 地址并 302 跳转。</summary>
    [HttpGet("/CmcWorkEasy/auth/login")]
    public IActionResult Login()
    {
        var state = Guid.NewGuid().ToString("N");

        Response.Cookies.Append(StateCookieName, state, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromMinutes(5),
            Path = "/CmcWorkEasy"
        });

        var redirectUri = Uri.EscapeDataString(_opt.CallbackUrl);
        var oauthUrl =
            "https://open.weixin.qq.com/connect/oauth2/authorize" +
            $"?appid={Uri.EscapeDataString(_opt.CorpId)}" +
            $"&redirect_uri={redirectUri}" +
            "&response_type=code" +
            "&scope=snsapi_privateinfo" +           // 获取手机号必须用它
            $"&agentid={Uri.EscapeDataString(_opt.AgentId)}" +
            $"&state={Uri.EscapeDataString(state)}" +
            "#wechat_redirect";

        return Redirect(oauthUrl);
    }

    /// <summary>企业微信 OAuth 回调。</summary>
    [HttpGet("/CmcWorkEasy/auth/callback")]
    public async Task<IActionResult> Callback([FromQuery] string? code, [FromQuery] string? state)
    {
        if (string.IsNullOrWhiteSpace(code)) return BadRequest("企业微信回调缺少 code。");
        if (string.IsNullOrWhiteSpace(state)) return BadRequest("企业微信回调缺少 state。");

        var savedState = Request.Cookies[StateCookieName];
        if (string.IsNullOrWhiteSpace(savedState) ||
            !string.Equals(savedState, state, StringComparison.Ordinal))
        {
            _logger.LogWarning("OAuth state 校验失败。");
            return BadRequest("OAuth state 校验失败。");
        }

        Response.Cookies.Delete(StateCookieName, new CookieOptions { Path = "/CmcWorkEasy" });

        try
        {
            // 1. code 换 userid + user_ticket
            var (userId, userTicket) = await _weCom.GetUserInfoByCodeAsync(code);

            // 2. user_ticket 换手机号
            var mobile = await _weCom.GetMobileByUserTicketAsync(userTicket);

            // 3. 手机号查 HR 表，确认在职
            var employee = await _emp.FindActiveEmployeeByMobileAsync(mobile);
            if (employee == null)
            {
                _logger.LogWarning("企业微信用户未匹配到在职员工。UserId={U}", userId);
                return Unauthorized("未匹配到公司在职员工，无法登录。");
            }

            // 4. 写入 Claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, employee.LoginName),
                new("DisplayName", employee.ChineseName),
                new("Mobile", employee.MobilePhone),
                new("EmployeeNo", employee.EmployeeNo),
                new("Department", employee.DepartmentName),
                new("WeComUserId", userId),
                new("AuthSource", "WeCom")
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // 5. 签发加密认证 Cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true });

            _logger.LogInformation("用户登录成功。LoginName={L}", employee.LoginName);

            // 6. 跳回首页
            return Redirect("/CmcWorkEasy/");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "企业微信登录处理失败。");
            return StatusCode(500, "登录失败，请稍后重试或联系系统管理员。");
        }
    }

    [HttpPost("/CmcWorkEasy/auth/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { success = true, message = "已退出登录" });
    }

    [HttpGet("/CmcWorkEasy/auth/denied")]
    public IActionResult Denied() => Forbid();
}
```

---

## 11. 当前登录用户接口

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CmcWorkEasy.Controllers;

[ApiController]
[Authorize]
public class AccountController : ControllerBase
{
    /// <summary>获取当前登录用户信息，前端据此判断是否已登录。</summary>
    [HttpGet("/CmcWorkEasy/api/account/me")]
    public IActionResult Me() => Ok(new
    {
        loginName   = User.Identity?.Name,
        displayName = User.FindFirst("DisplayName")?.Value,
        mobile      = User.FindFirst("Mobile")?.Value,
        employeeNo  = User.FindFirst("EmployeeNo")?.Value,
        department  = User.FindFirst("Department")?.Value,
        authSource  = User.FindFirst("AuthSource")?.Value
    });
}
```

---

## 12. 前端 H5 登录逻辑

前端只负责：显示页面、调用业务 API、发现未登录（401）时跳转后端登录入口。

前端**绝不**：保存 Secret、直接调 qyapi、自报手机号/工号、伪造身份。

```html
<!DOCTYPE html>
<html lang="zh-CN">
<head>
  <meta charset="UTF-8">
  <title>CMC办公助手</title>
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <style>
    body { font-family: Arial, "Microsoft YaHei", sans-serif; margin: 0; padding: 16px; background: #f5f5f5; }
    .card { background: #fff; border-radius: 8px; padding: 16px; margin-bottom: 12px; box-shadow: 0 2px 6px rgba(0,0,0,.08); }
    button { width: 100%; padding: 12px; border: 0; border-radius: 6px; background: #0066cc; color: #fff; font-size: 16px; }
  </style>
</head>
<body>
  <div class="card"><h2>CMC办公助手</h2><p id="userInfo">正在检查登录状态...</p></div>
  <div class="card"><button onclick="loadAttendance()">查询考勤</button></div>
  <div class="card"><button onclick="loadAnnualLeave()">查询年假</button></div>

  <script>
    async function apiGet(url) {
      const r = await fetch(url, { credentials: 'include' });
      if (r.status === 401) { location.href = '/CmcWorkEasy/auth/login'; return null; }
      return r;
    }

    async function checkLogin() {
      const r = await apiGet('/CmcWorkEasy/api/account/me');
      if (!r) return;
      if (!r.ok) { document.getElementById('userInfo').innerText = '登录状态检查失败，请稍后重试。'; return; }
      const u = await r.json();
      document.getElementById('userInfo').innerText = `欢迎您，${u.displayName || u.loginName}`;
    }

    async function loadAttendance() {
      const r = await apiGet('/CmcWorkEasy/api/attendance');
      if (r && r.ok) alert(JSON.stringify(await r.json(), null, 2));
    }

    async function loadAnnualLeave() {
      const r = await apiGet('/CmcWorkEasy/api/annual-leave/balance');
      if (r && r.ok) alert(JSON.stringify(await r.json(), null, 2));
    }

    checkLogin();
  </script>
</body>
</html>
```

> 因为 `Program.cs` 已把 `/api` 未登录改为返回 **401**，上面的 `status === 401` 判断才真正生效。

---

## 13. 考勤接口示例

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CmcWorkEasy.Controllers;

[ApiController]
[Authorize]
public class AttendanceController : ControllerBase
{
    [HttpGet("/CmcWorkEasy/api/attendance")]
    public IActionResult GetAttendance()
    {
        // 当前用户身份来自 Cookie Claims，绝不信任前端传来的工号
        var employeeNo  = User.FindFirst("EmployeeNo")?.Value;
        var displayName = User.FindFirst("DisplayName")?.Value;

        // 实际项目：根据 employeeNo 用 ADO.NET 查询考勤库
        var result = new
        {
            employeeNo,
            name = displayName,
            month = DateTime.Now.ToString("yyyy-MM"),
            records = new[]
            {
                new { date = DateTime.Today.ToString("yyyy-MM-dd"), checkIn = "08:55", checkOut = "17:35", status = "正常" }
            }
        };
        return Ok(result);
    }
}
```

> **关键原则**：当前用户是谁由 Cookie Claims 决定，不要相信前端传来的 loginName、employeeNo、mobile。

---

## 14. 年假接口示例

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CmcWorkEasy.Controllers;

[ApiController]
[Authorize]
public class AnnualLeaveController : ControllerBase
{
    [HttpGet("/CmcWorkEasy/api/annual-leave/balance")]
    public IActionResult GetBalance()
    {
        var employeeNo  = User.FindFirst("EmployeeNo")?.Value;
        var displayName = User.FindFirst("DisplayName")?.Value;
        // 实际项目：从 HR/考勤系统查询年假余额
        return Ok(new { employeeNo, name = displayName, year = DateTime.Now.Year, totalDays = 10, usedDays = 2, remainingDays = 8 });
    }

    [HttpPost("/CmcWorkEasy/api/annual-leave/apply")]
    public IActionResult Apply([FromBody] AnnualLeaveApplyRequest request)
    {
        var employeeNo  = User.FindFirst("EmployeeNo")?.Value; // 员工号来自 Claims，不来自前端
        var displayName = User.FindFirst("DisplayName")?.Value;

        if (request.StartDate > request.EndDate) return BadRequest("开始日期不能晚于结束日期。");
        if (request.Days <= 0) return BadRequest("请假天数必须大于 0。");

        // 实际项目：用 ADO.NET 写入请假申请表
        return Ok(new { success = true, message = "年假申请已提交", employeeNo, name = displayName,
                        request.StartDate, request.EndDate, request.Days, request.Reason });
    }
}

public class AnnualLeaveApplyRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Days { get; set; }
    public string Reason { get; set; } = string.Empty;
}
```

> **CSRF 说明**：Cookie 设了 `SameSite=Lax`，跨站 POST 不会带上 Cookie，已能挡住绝大多数 CSRF。如需纵深防御，可要求写操作附带自定义请求头（如 `X-Requested-With`）并在服务端校验。

---

## 15. Nginx 反向代理配置

### 15.1 基本配置

```nginx
server {
    listen 443 ssl;
    server_name bc.canon-medical.com.cn;

    ssl_certificate     /etc/nginx/cert/bc.canon-medical.com.cn.pem;
    ssl_certificate_key /etc/nginx/cert/bc.canon-medical.com.cn.key;

    location /CmcWorkEasy/ {
        proxy_pass http://192.168.105.14:5187/CmcWorkEasy/;
        proxy_http_version 1.1;

        proxy_set_header Host              $host;
        proxy_set_header X-Real-IP         $remote_addr;
        proxy_set_header X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto https;   # 关键：告诉 ASP.NET Core 外部是 HTTPS
    }
}
```

### 15.2 HTTP 强制跳转 HTTPS

```nginx
server {
    listen 80;
    server_name bc.canon-medical.com.cn;
    return 301 https://$host$request_uri;
}
```

### 15.3 为什么 `X-Forwarded-Proto` 很重要

外部是 `https://...`，但 Nginx→ASP.NET Core 是 `http://192.168.105.14:5187/...`。若不转发 `X-Forwarded-Proto=https`，ASP.NET Core 会以为当前是 HTTP，导致：

```text
生成错误的 redirect_uri / 回调地址异常
Secure Cookie 不下发
登录后 Cookie 丢失、循环跳登录
```

配合 `Program.cs` 里的 `UseForwardedHeaders`（已显式信任 Nginx 内网 IP）一起生效。

---

## 16. Cookie 与安全设置

### 16.1 建议配置

```csharp
options.Cookie.Name = "CmcWorkEasy.Auth";
options.Cookie.HttpOnly = true;
options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
options.Cookie.SameSite = SameSiteMode.Lax;
options.ExpireTimeSpan = TimeSpan.FromHours(8);
options.SlidingExpiration = true;
```

| 配置 | 作用 |
|---|---|
| HttpOnly | JavaScript 无法读取 Cookie |
| Secure | Cookie 只通过 HTTPS 传输 |
| SameSite=Lax | 防止大多数跨站请求（含 CSRF） |
| ExpireTimeSpan | 控制登录有效期 |
| SlidingExpiration | 用户活跃时自动续期 |

### 16.2 Claims 里放什么

可以放：`LoginName / DisplayName / Mobile / EmployeeNo / Department / WeComUserId`。

不要放：企业微信 Secret、数据库连接串、完整权限表、身份证号、银行卡号等敏感隐私。Cookie 虽加密签名，仍应尽量精简。

### 16.3 离职员工处理

登录时已排除 `[在职状态]='离职'`，但要考虑"上午登录成功、下午被改为离职、Cookie 仍有效"的窗口期。可选增强：

1. 缩短 Cookie 有效期（如 4 小时）。
2. 在关键业务接口重新校验员工在职状态。
3. 用自定义中间件定期校验 HR 状态。
4. 员工离职时主动清理服务端会话。

### 16.4 多实例部署的加密密钥

若部署多台后端做负载均衡，各实例默认的数据保护密钥不同，会导致"A 实例签发的 Cookie 到 B 实例解不开"。需配置共享的 Data Protection 密钥环（指向共享目录或 Redis），并统一应用名：

```csharp
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"\\shared\keys\CmcWorkEasy"))
    .SetApplicationName("CmcWorkEasy");
```

---

## 17. access_token 缓存策略

### 17.1 为什么要缓存

每次登录都调用 `/cgi-bin/gettoken` 会导致：调用频繁、性能下降、触发企业微信频率限制、增加登录延迟。因此必须缓存（有效期通常 7200 秒，提前 5 分钟过期）。

### 17.2 单机部署

使用内置 `IMemoryCache`（本文第 8 节实现，已加 `SemaphoreSlim` 锁防击穿）。

### 17.3 多实例部署

改用分布式缓存（Redis / SQL Server 缓存表），避免每台各自刷新导致 token 相互失效。可用 `IDistributedCache` 替换 `IMemoryCache`。

---

## 18. 日志设计建议

### 18.1 应记录

```text
登录开始 / OAuth state 校验失败 / 企业微信接口调用失败
手机号未匹配到员工 / 手机号重复
员工登录成功 / 员工提交年假申请
```

### 18.2 不应记录

```text
Secret / access_token / user_ticket / 完整 Cookie
身份证号 / 其它敏感个人信息
```

手机号建议脱敏后记录：

```csharp
private static string MaskMobile(string mobile)
    => (string.IsNullOrWhiteSpace(mobile) || mobile.Length < 7)
        ? mobile
        : mobile[..3] + "****" + mobile[^4..];

// 用法
_logger.LogInformation("用户登录成功，Mobile={M}", MaskMobile(employee.MobilePhone));
```

---

## 19. 常见问题排查

### 19.1 企业微信提示可信域名错误

原因：`redirect_uri` 域名不是 `bc.canon-medical.com.cn`、用了 localhost/内网 IP、后台可信域名没配对。
解决：确保回调地址为 `https://bc.canon-medical.com.cn/CmcWorkEasy/auth/callback`。

### 19.2 获取 access_token 失败（如 `60020 not allow to access from your ip`）

原因：CorpId/Secret 错、可信 IP 配置错、服务器出口 IP 不是 124.127.244.155、用了其他应用的 Secret。
排查：服务器 `curl qyapi.weixin.qq.com` 确认出口公网 IP；核对后台可信 IP；核对 CorpId/AgentId/Secret 是否匹配。

### 19.3 能拿 userid 但拿不到 mobile

原因：`scope` 不是 `snsapi_privateinfo`、后台未开手机号敏感字段、用户没点"同意"、`user_ticket` 为空。

### 19.4 登录成功后前端 API 仍 401

原因：前端 `fetch` 没带 `credentials:'include'`；Cookie `Path` 不覆盖 `/CmcWorkEasy`；`Secure` 但外部不是 HTTPS；Nginx 没转发 `X-Forwarded-Proto`；漏了 `UseAuthentication`；中间件顺序错。
正确顺序：`UseForwardedHeaders → UseStaticFiles → UseRouting → UseAuthentication → UseAuthorization → MapControllers`。

### 19.5 Cookie 不写入浏览器

检查：是否 HTTPS 访问、`Secure` 是否要求 HTTPS、`SameSite` 是否过严、Nginx 是否转发 `X-Forwarded-Proto`、`Path` 是否覆盖 `/CmcWorkEasy`。

### 19.6 `code` 无效（如 `40029`）

原因：`code` 被重复使用（只能用一次）或已过期（约 5 分钟）。回调里不要重复调用换取接口。

### 19.7 手机号匹配错人

原因：HR 表存在重复手机号、格式不统一、测试数据未清理、离职员工手机号与在职员工重复。
解决：手机号唯一性检查、新增 `NormalizedMobilePhone` 字段、发现重复时拒绝登录、由 HR 处理数据。

---

## 20. 推荐目录结构

```text
CmcWorkEasy
├── Controllers
│   ├── AuthController.cs
│   ├── AccountController.cs
│   ├── AttendanceController.cs
│   └── AnnualLeaveController.cs
├── Models
│   ├── EmployeeLoginInfo.cs
│   └── WeCom
│       ├── AccessTokenResponse.cs
│       ├── UserInfoResponse.cs
│       └── UserDetailResponse.cs
├── Options
│   └── WeComOptions.cs
├── Services
│   ├── IWeComService.cs
│   ├── WeComService.cs
│   ├── IEmployeeService.cs
│   └── EmployeeService.cs
├── wwwroot
│   └── index.html
├── appsettings.json
├── CmcWorkEasy.csproj
└── Program.cs
```

---

## 21. 最小可运行实现步骤

建议按下面顺序增量实现，不要一次做完：

1. **部署 H5 首页**：确认 `https://bc.canon-medical.com.cn/CmcWorkEasy/` 能打开。
2. **配置 Nginx 反向代理**：确认外部 HTTPS 可访问、内部能转发到 5187、`X-Forwarded-Proto=https` 生效。
3. **实现 `/auth/login`**：确认能跳转到企业微信授权页。
4. **实现 `/auth/callback`**：确认收到 `code`、`state`，且 `state` 校验成功。
5. **实现 gettoken**：确认能拿到 `access_token`（失败先查 CorpId/Secret/可信 IP/出口 IP）。
6. **实现 getuserinfo**：确认能拿到 `userid` + `user_ticket`。
7. **实现 getuserdetail**：确认能拿到 `mobile`（拿不到查敏感字段授权）。
8. **实现 HR 表匹配**：确认能匹配员工并排除 `[在职状态]='离职'`。
9. **生成 Cookie**：确认登录后浏览器存在 `CmcWorkEasy.Auth`。
10. **保护业务 API**：加 `[Authorize]`，确认未登录返回 401、登录后可访问。

---

## 22. 关键原则总结

```text
前端只负责页面展示和调用业务 API。
前端不能保存 Secret。
前端不能自报手机号后告诉后端"我是谁"。
手机号必须由后端调用企业微信 API 获得。
HRPersonalInfoTable 决定用户是否是公司在职员工。
ASP.NET Core 加密 Cookie 决定后续业务接口是否允许访问。
业务接口使用 Claims 中的员工号/登录名查询数据。
不要相信前端传来的员工号、手机号、登录名。
Nginx 必须正确传递 X-Forwarded-Proto。
企业微信可信域名和 OAuth 回调域名必须一致。
企业可信 IP 必须是后端访问企业微信时的公网出口 IP。
```

一句话概括：

> CMC办公助手的登录流程是：员工在企业微信中打开 H5，自建应用通过 OAuth 让后端拿到企业微信认证过的手机号；后端用手机号匹配 HR 在职员工；匹配成功后生成 ASP.NET Core 加密 Cookie；之后考勤查询和年假申请都基于这个 Cookie 中的身份声明完成。

---

## 23. 附：相对原始方案的修正清单

本文在原始方案基础上做了以下修正与补充：

| 序号 | 类型 | 说明 |
|---|---|---|
| 1 | 环境澄清 | 目标运行时确认为 **ASP.NET Core 8（.NET 8）**；Windows 自带的 .NET Framework 4.8 / System.Web 版本号与本应用无关。 |
| 2 | **修正 bug** | 为 `/api` 路径重写 `OnRedirectToLogin` / `OnRedirectToAccessDenied`，未登录返回 **401/403** 而非 302，使前端 `status===401` 判断真正生效。 |
| 3 | 技术栈对齐 | 数据访问由 Dapper 改为 **ADO.NET（`Microsoft.Data.SqlClient`）**，参数化查询。 |
| 4 | 健壮性 | `access_token` 缓存加 `SemaphoreSlim` + 双重检查，防缓存击穿与频率限制。 |
| 5 | 安全 | `UseForwardedHeaders` 显式配置 `KnownProxies`（信任 Nginx 内网 IP），不清空信任列表。 |
| 6 | 健壮性 | `NormalizeMobile` 改为"只保留数字 + 去 86 前缀"，覆盖更多手机号格式。 |
| 7 | 补充 | 明确 `code`（一次性、约 5 分钟）与 `user_ticket`（约 1800 秒）时效；补充获取手机号的两条路径（`snsapi_privateinfo` / `snsapi_base + user/get`）。 |
| 8 | 补充 | 多实例部署需共享 Data Protection 密钥；access_token 用分布式缓存。 |
| 9 | 补充 | CSRF 说明（`SameSite=Lax` + 可选自定义头）；`NormalizedMobilePhone` 计算列 + 索引优化。 |
| 10 | 说明 | .NET 8 使用系统 TLS，无需手动设置 `ServicePointManager.SecurityProtocol`。 |
