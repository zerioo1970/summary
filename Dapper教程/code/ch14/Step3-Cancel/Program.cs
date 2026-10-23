// code/ch14/Step3-Cancel/Program.cs
// CommandDefinition + CancellationToken：不等超时，由你的代码决定"现在就停"
using System.Diagnostics;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

await using var conn = new SqlConnection(connStr);

// 例 1：CommandDefinition 把 SQL、参数、超时、取消令牌装进一个对象，再交给 XxxAsync
var normal = new CommandDefinition(
    "SELECT RequestNo FROM dbo.PriceRequest WHERE Applicant = @Applicant ORDER BY Id",
    new { Applicant = "张伟" },
    commandTimeout: 10,
    cancellationToken: CancellationToken.None);          // None = 永远不会被取消
var nos = await conn.QueryAsync<string>(normal);
Console.WriteLine($"例 1 正常查询：{string.Join("、", nos)}");

// 例 2：1 秒后自动取消——模拟用户在界面上点了"取消"，或者 HTTP 请求被浏览器断开
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
var slow = new CommandDefinition("WAITFOR DELAY '00:00:05'; SELECT 1;", cancellationToken: cts.Token);
var sw = Stopwatch.StartNew();
try
{
    await conn.ExecuteScalarAsync<int>(slow);
    Console.WriteLine("例 2 不会走到这里");
}
catch (Exception ex) when (cts.IsCancellationRequested)   // 判断"是我取消的"，比猜异常类型更可靠
{
    Console.WriteLine($"例 2 {sw.Elapsed.TotalSeconds:F1} 秒后停下：{ex.GetType().FullName}");
    Console.WriteLine($"     {ex.Message}");
}

// 例 3：令牌在调用前就已经取消——命令根本不会发出去
using var cancelled = new CancellationTokenSource();
cancelled.Cancel();
try
{
    await conn.QueryAsync<string>(new CommandDefinition(
        "SELECT RequestNo FROM dbo.PriceRequest", cancellationToken: cancelled.Token));
}
catch (OperationCanceledException ex)   // TaskCanceledException 也是它的子类，一并接住
{
    Console.WriteLine($"例 3 调用前已取消：{ex.GetType().FullName}");
}

Console.WriteLine($"最后连接状态：{conn.State}");
