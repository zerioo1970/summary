// code/ch14/Step2-Timeout/Program.cs
// commandTimeout：一条命令最多等几秒。默认 30 秒；超时后抛 SqlException，Number = -2
using System.Diagnostics;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

await using var conn = new SqlConnection(connStr);

// 例 1：不写 commandTimeout 时用的是谁的默认值
using (var cmd = conn.CreateCommand())
    Console.WriteLine($"例 1 SqlCommand 默认 CommandTimeout = {cmd.CommandTimeout} 秒");
Console.WriteLine($"     Dapper 全局 SqlMapper.Settings.CommandTimeout = {SqlMapper.Settings.CommandTimeout?.ToString() ?? "null"}");

// 例 2：给得够——等 1 秒的语句，允许 10 秒，正常完成
var sw = Stopwatch.StartNew();
await conn.ExecuteAsync("WAITFOR DELAY '00:00:01'", commandTimeout: 10);
Console.WriteLine($"例 2 允许 10 秒，实际 {sw.Elapsed.TotalSeconds:F1} 秒，正常完成");

// 例 3：给不够——等 5 秒的语句（模拟慢报表），只允许 2 秒
sw.Restart();
try
{
    await conn.ExecuteAsync("WAITFOR DELAY '00:00:05'", commandTimeout: 2);
    Console.WriteLine("例 3 不会走到这里");
}
catch (SqlException ex) when (ex.Number == -2)   // -2 是客户端超时的固定编号（第 20 章细讲 Number）
{
    Console.WriteLine($"例 3 允许 2 秒，{sw.Elapsed.TotalSeconds:F1} 秒后放弃，Number = {ex.Number}");
    Console.WriteLine($"     {ex.Message}");
}

// 例 4：超时之后，这个连接对象还能接着用（Dapper 自己开的连接已经关上，下次重新打开）
Console.WriteLine($"例 4 超时后连接状态：{conn.State}，再查一次：{await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM dbo.Product")} 个产品");
