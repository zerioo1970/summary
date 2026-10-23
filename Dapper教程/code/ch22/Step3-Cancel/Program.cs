// code/ch22/Step3-Cancel/Program.cs
// 取消令牌接到 HTTP 请求上：浏览器关了 / 客户端断开，正在执行的 SQL 跟着停
using System.Data;
using System.Diagnostics;
using Dapper;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

string connStr = builder.Configuration.GetConnectionString("PriceDemo")
    ?? throw new InvalidOperationException("appsettings.json 里没有 ConnectionStrings:PriceDemo");
builder.Services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connStr));
builder.Services.AddScoped<ReportRepository>();

var app = builder.Build();

// 端点参数写一个 CancellationToken：框架自动绑定为"这个请求被中止"的信号（HttpContext.RequestAborted）
app.MapGet("/reports/slow", async (ReportRepository repo, CancellationToken ct) =>
{
    var sw = Stopwatch.StartNew();
    try
    {
        int count = await repo.GetSlowReportAsync(ct);
        Console.WriteLine($"[slow] 完成，用时 {sw.Elapsed.TotalSeconds:F1} 秒");
        return Results.Ok(new { count });
    }
    catch (Exception ex) when (ct.IsCancellationRequested)   // 不猜异常类型，问令牌（第 14 章）
    {
        Console.WriteLine($"[slow] 客户端已断开，查询被取消，用时 {sw.Elapsed.TotalSeconds:F1} 秒（{ex.GetType().Name}）");
        return Results.Empty;                                  // 客户端已经不在了，返回什么都收不到
    }
});

// 对照组：同样的查询，不接令牌
app.MapGet("/reports/slow-no-ct", async (ReportRepository repo) =>
{
    var sw = Stopwatch.StartNew();
    int count = await repo.GetSlowReportAsync(CancellationToken.None);
    Console.WriteLine($"[slow-no-ct] 完成，用时 {sw.Elapsed.TotalSeconds:F1} 秒（客户端早就走了）");
    return Results.Ok(new { count });
});

app.Run();

public interface IDbConnectionFactory
{
    IDbConnection Create();
}

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connStr;
    public SqlConnectionFactory(string connStr) => _connStr = connStr;
    public IDbConnection Create() => new SqlConnection(_connStr);
}

public class ReportRepository
{
    private readonly IDbConnectionFactory _factory;
    public ReportRepository(IDbConnectionFactory factory) => _factory = factory;

    // 仓储方法多一个 CancellationToken 参数，原样交给 CommandDefinition（第 14 章）
    public async Task<int> GetSlowReportAsync(CancellationToken ct)
    {
        using var conn = _factory.Create();
        var cmd = new CommandDefinition(@"
            WAITFOR DELAY '00:00:05';                 -- 模拟一个要跑 5 秒的报表
            SELECT COUNT(*) FROM dbo.PriceRequest;",
            cancellationToken: ct);
        return await conn.ExecuteScalarAsync<int>(cmd);
    }
}
