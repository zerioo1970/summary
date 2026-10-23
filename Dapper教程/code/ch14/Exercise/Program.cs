// code/ch14/Exercise/Program.cs
// 第 14 章练习参考答案：一个带超时和取消的"跑报表"方法，分清 完成 / 超时 / 已取消 三种结局
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

// 情况 1：语句 1 秒，允许 3 秒 → 完成
Console.WriteLine("情况 1：" + await RunReportAsync("WAITFOR DELAY '00:00:01'; SELECT COUNT(*) FROM dbo.PriceRequest;", 3, CancellationToken.None));

// 情况 2：语句 3 秒，只允许 1 秒 → 超时
Console.WriteLine("情况 2：" + await RunReportAsync("WAITFOR DELAY '00:00:03'; SELECT COUNT(*) FROM dbo.PriceRequest;", 1, CancellationToken.None));

// 情况 3：语句 3 秒，允许 10 秒，但 1 秒后用户点了取消 → 已取消
using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1)))
    Console.WriteLine("情况 3：" + await RunReportAsync("WAITFOR DELAY '00:00:03'; SELECT COUNT(*) FROM dbo.PriceRequest;", 10, cts.Token));

async Task<string> RunReportAsync(string sql, int timeoutSeconds, CancellationToken ct)
{
    await using var conn = new SqlConnection(connStr);   // 每次调用一个新连接，用完即关
    var cmd = new CommandDefinition(sql, commandTimeout: timeoutSeconds, cancellationToken: ct);
    try
    {
        int count = await conn.ExecuteScalarAsync<int>(cmd);
        return $"完成，申请单 {count} 张";
    }
    catch (SqlException ex) when (ex.Number == -2)          // 超时：服务器太慢
    {
        return $"超时（允许 {timeoutSeconds} 秒，Number = {ex.Number}）";
    }
    catch (Exception) when (ct.IsCancellationRequested)     // 取消：是调用方不想等了
    {
        return "已取消";
    }
}
