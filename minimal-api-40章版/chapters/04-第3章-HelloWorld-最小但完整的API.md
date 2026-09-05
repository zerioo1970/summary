# 第3章　Hello World：最小但完整的API

本章从一个空Web项目开始，每次只增加一个概念：字符串响应、JSON响应、路由参数、查询参数和状态码。每一步都能独立运行，方便新手判断问题出在哪一步。

  --------------------------------------------------------------------------------------------------------
  **先把三个问题说清楚　**这是什么：Hello World API是一个真正监听HTTP请求并返回响应的ASP.NET Core程序。\
  目的是什么：建立创建项目、写端点、运行、访问、检查结果的完整开发闭环。\
  最后得到什么：最终得到GET /api/hello、GET /api/hello/{name}和GET /api/users/{id}三个可测试端点。
  --------------------------------------------------------------------------------------------------------

  --------------------------------------------------------------------------------------------------------

  -------------------------------------------------------------------------------------------------------------------------------
  **本章操作路线　**创建项目 → 理解csproj → 写第一个GET → 运行并复制端口 → 返回JSON → 接收参数 → 返回状态码 → 用.http和curl验证
  -------------------------------------------------------------------------------------------------------------------------------

  -------------------------------------------------------------------------------------------------------------------------------

## 3.1 从父目录创建HelloMinimalApi

先在父目录创建项目，再进入项目目录。这样可以清楚区分"存放多个项目的文件夹"和"包含csproj的单个项目文件夹"。

**示例3-1　创建并进入项目**

```bash
cd D:\dotnet-labs
dotnet new web -n HelloMinimalApi -f net10.0
cd HelloMinimalApi
dir
```

**示例3-2　初始目录结构**

```text
HelloMinimalApi/
├─ Properties/
│ └─ launchSettings.json
├─ appsettings.json
├─ appsettings.Development.json
├─ HelloMinimalApi.csproj
└─ Program.cs
```

## 3.2 csproj是什么，为什么必须先看它

csproj是项目的构建说明书。dotnet命令通过它知道项目使用哪种SDK、目标.NET版本和NuGet依赖。Program.cs是业务入口，csproj则决定怎样编译它。

**示例3-3　HelloMinimalApi.csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
<PropertyGroup>
<TargetFramework>net10.0</TargetFramework>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>
</Project>
```

-   Microsoft.NET.Sdk.Web：说明这是ASP.NET Core Web项目，自动引入Web开发所需的构建能力。

-   TargetFramework net10.0：项目针对.NET 10编译。

-   Nullable enable：启用可空引用类型分析，帮助发现可能的null问题。

-   ImplicitUsings enable：自动导入常用命名空间，所以最小Program.cs不必写很多using。

## 3.3 第一步：只返回一段文字

先把Program.cs改成最少代码。目的不是展示高级功能，而是确认路由、服务器和响应链路能工作。

**示例3-4　第一个Minimal API**

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api/hello", () => "Hello World!");

app.Run();
```

-   CreateBuilder：读取命令行、配置和环境，并准备服务容器。

-   Build：创建WebApplication。

-   MapGet：只在HTTP方法是GET且路径是/api/hello时执行处理函数。

-   lambda返回字符串：框架生成200响应和文本响应体。

-   Run：启动Kestrel。没有它，程序不会持续监听。

## 3.4 第二步：运行并打开正确的网址

保存代码，在包含HelloMinimalApi.csproj的目录执行build和run。运行后的端口由本机配置决定，所以必须读取终端。

**示例3-5　编译和运行**

```bash
dotnet build
dotnet run

# 典型输出，实际端口可能不同：
# Now listening on: https://localhost:7123
# Now listening on: http://localhost:5187
```

-   如果终端显示https://localhost:7123，浏览器就打开https://localhost:7123/api/hello。

-   如果只显示http://localhost:5187，就打开http://localhost:5187/api/hello。

-   看到Hello World!就是本步骤的最终结果。

-   看到404说明服务器已响应，但路径或HTTP方法不匹配；连接被拒绝通常说明服务器没运行或端口错误。

## 3.5 第三步：返回对象，让框架自动生成JSON

真实前端通常需要结构化数据。返回C#对象时，ASP.NET Core会自动把它序列化为JSON并设置application/json，不需要手工拼接字符串。

**示例3-6　返回JSON对象**

```csharp
app.MapGet("/api/hello", () =>
{
var result = new
{
message = "Hello, Minimal API!",
serverTime = DateTimeOffset.Now
};

return Results.Ok(result);
});
```

**示例3-7　浏览器看到的JSON结构示意**

```json
{
"message": "Hello, Minimal API!",
"serverTime": "2026-07-18T16:30:00+08:00"
}
```

-   匿名对象负责组织字段。

-   Results.Ok明确返回200 OK。

-   serverTime每次请求都会重新计算，所以刷新页面时会变化。

-   JSON字段名默认采用Web JSON命名规则；第11章会介绍统一配置。

## 3.6 第四步：接收路由参数和查询参数

路由参数是URL路径的一部分，通常用来标识某个资源；查询参数写在问号后面，通常用于筛选、排序或可选设置。参数绑定系统会把字符串转换为处理函数参数。

**示例3-8　路由参数name和查询参数language**

```csharp
app.MapGet("/api/hello/{name}",
(string name, string? language) =>
{
var greeting = language?.ToLowerInvariant() switch
{
"en" => $"Hello, {name}!",
"zh" => $"你好，{name}！",
_ => $"欢迎，{name}！"
};

return Results.Ok(new { greeting, language });
});
```

-   访问/api/hello/Alice?language=en。Alice绑定到name，en绑定到language。

-   string?表示language可以省略；省略时值为null并使用默认问候语。

-   name来自{name}路由段，所以不能省略。

-   用户输入可能包含空格或中文，客户端应进行URL编码；浏览器和前端URL API通常会处理。

## 3.7 第五步：根据结果返回200或404

API不仅要返回数据，还要用HTTP状态码说明结果。找到资源返回200；资源不存在返回404。客户端可以先看状态码，再决定怎样处理响应体。

**示例3-9　按资源是否存在返回不同状态码**

```csharp
var users = new[]
{
new User(1, "Alice"),
new User(2, "Bob")
};

app.MapGet("/api/users/{id:int}", (int id) =>
{
var user = users.FirstOrDefault(x => x.Id == id);

return user is null
? Results.NotFound(new { message = $"用户{id}不存在" })
: Results.Ok(user);
});

record User(int Id, string Name);
```

-   {id:int}是路由约束，只有整数路径段才匹配这个端点。

-   访问/api/users/1得到200和用户JSON。

-   访问/api/users/999得到404和错误JSON。

-   访问/api/users/abc不会进入处理函数，因为abc无法满足int约束。

## 3.8 使用.http文件重复测试

浏览器地址栏适合简单GET；.http文件可以保存多个请求、请求头和JSON请求体。它与代码一起进入版本控制后，其他人可以重复同样的测试。

**示例3-10　HelloMinimalApi.http**

```text
@baseUrl = https://localhost:7123

### 最简单的GET
GET {{baseUrl}}/api/hello

### 路由参数和查询参数
GET {{baseUrl}}/api/hello/Alice?language=zh

### 成功和不存在
GET {{baseUrl}}/api/users/1

###
GET {{baseUrl}}/api/users/999
```

  ----------------------------------------------------------------------------------------------------------
  **先改baseUrl　**把7123替换为dotnet run终端显示的实际HTTPS端口。如果只使用HTTP，就连协议和端口一起替换。
  ----------------------------------------------------------------------------------------------------------

  ----------------------------------------------------------------------------------------------------------

## 3.9 使用curl检查状态码和响应头

curl特别适合命令行和服务器环境。-i让它同时显示响应头，这样可以看到HTTP状态码和Content-Type。

**示例3-11　使用curl测试**

```bash
curl -i https://localhost:7123/api/hello
curl -i "https://localhost:7123/api/hello/Alice?language=zh"
curl -i https://localhost:7123/api/users/999
```

-   PowerShell中URL包含&时应使用引号。

-   开发证书不可信时不要长期使用跳过证书校验作为解决方案，应先修复证书。

-   curl返回的状态行、Content-Type和响应体应该与代码意图一致。

## 3.10 合并后的完整Program.cs

下面代码把本章三个阶段合并在一起。复制后先build，再run，然后按前面的具体网址逐个测试。

**示例3-12　本章最终Program.cs**

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var users = new[]
{
new User(1, "Alice"),
new User(2, "Bob")
};

app.MapGet("/api/hello", () =>
Results.Ok(new
{
message = "Hello, Minimal API!",
serverTime = DateTimeOffset.Now
}));

app.MapGet("/api/hello/{name}",
(string name, string? language) =>
{
var greeting = language?.ToLowerInvariant() switch
{
"en" => $"Hello, {name}!",
"zh" => $"你好，{name}！",
_ => $"欢迎，{name}！"
};

return Results.Ok(new { greeting, language });
});

app.MapGet("/api/users/{id:int}", (int id) =>
{
var user = users.FirstOrDefault(x => x.Id == id);
return user is null
? Results.NotFound(new { message = $"用户{id}不存在" })
: Results.Ok(user);
});

app.Run();

record User(int Id, string Name);
```

  ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
  最终验收　三个端点都能从终端实际地址访问；成功请求得到200；不存在的用户得到404；终端保持运行且没有未处理异常。达到这些结果后，先进入第4章理解真正监听端口的Kestrel，再进入第6章使用Swagger UI测试接口。
  ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

  ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
