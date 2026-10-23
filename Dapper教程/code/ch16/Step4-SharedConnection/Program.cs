// code/ch16/Step4-SharedConnection/Program.cs
// 反例：全程序共享一个静态 SqlConnection，50 个任务同时查询；对照：每次查询新建连接
using System.Collections.Concurrent;
using Dapper;
using Microsoft.Data.SqlClient;

const string sql = "SELECT COUNT(*) FROM dbo.Customer";

// 错误写法：50 个任务同时用 Db.Shared
var (ok1, errors1) = RunParallel(() => Db.Shared.ExecuteScalar<int>(sql));
Console.WriteLine($"== 共享一个连接：成功 {ok1} 次，失败 {50 - ok1} 次");
foreach (var e in errors1.OrderByDescending(e => e.Value))
    Console.WriteLine($"  {e.Value} 次  {e.Key}");

// 被共享连接弄乱的连接池先清掉（16.2 节用过），保证对照组从干净的状态开始
SqlConnection.ClearAllPools();

// 正确写法：每次查询 new 一个连接，用完 Dispose（连接池保证这样做不慢）
var (ok2, errors2) = RunParallel(() =>
{
    using var conn = new SqlConnection(Db.ConnStr);
    return conn.ExecuteScalar<int>(sql);
});
Console.WriteLine($"== 每次新建连接：成功 {ok2} 次，失败 {50 - ok2} 次");
foreach (var e in errors2.OrderByDescending(e => e.Value))
    Console.WriteLine($"  {e.Value} 次  {e.Key}");

// 同时启动 50 个任务执行 query，统计成功次数和各种异常出现的次数
static (int ok, ConcurrentDictionary<string, int> errors) RunParallel(Func<int> query)
{
    int ok = 0;
    var errors = new ConcurrentDictionary<string, int>();
    Parallel.For(0, 50, _ =>
    {
        try
        {
            if (query() == 3) Interlocked.Increment(ref ok);   // 客户表有 3 行，结果对才算成功
        }
        catch (Exception ex)
        {
            errors.AddOrUpdate($"{ex.GetType().Name}: {ex.Message}", 1, (_, n) => n + 1);
        }
    });
    return (ok, errors);
}

static class Db
{
    public const string ConnStr =
        "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

    // ❌ 反例：看起来"省得每次 new"，实际上 SqlConnection 不是线程安全的
    public static readonly SqlConnection Shared = new SqlConnection(ConnStr);
}
