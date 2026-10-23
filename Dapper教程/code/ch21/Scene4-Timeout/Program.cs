// code/ch21/Scene4-Timeout/Program.cs
// 场景 4：查询偶尔超时——另一个连接开着事务没提交，锁住了要读的行
using System.Diagnostics;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

// 模拟"别人"：改了第 2 号单据，事务一直没提交（比如代码在 Commit 前等用户确认、或者抛异常没回滚）
using var blocker = new SqlConnection(connStr);
blocker.Open();
int blockerSpid = blocker.QuerySingle<int>("SELECT @@SPID");
var tx = blocker.BeginTransaction();
blocker.Execute("UPDATE dbo.PriceRequest SET Remark = N'改了一半' WHERE Id = 2", transaction: tx);
Console.WriteLine($"阻塞方会话号：{blockerSpid}（事务未提交）");

Console.WriteLine("== 故障现象：列表查询 3 秒后超时");
var listTask = LoadListAsync(connStr);         // 先发出查询，不等它，下面趁它被卡住时去看原因
await Task.Delay(1000);

Console.WriteLine("== 定位：查服务器上谁在等谁（需要 VIEW SERVER STATE 权限）");
using (var dba = new SqlConnection(connStr))
{
    var waits = dba.Query<WaitInfo>(@"
        SELECT r.session_id          AS SessionId,
               r.blocking_session_id AS BlockingSessionId,
               r.wait_type           AS WaitType,
               t.text                AS SqlText
        FROM sys.dm_exec_requests r
        CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) t
        WHERE r.blocking_session_id <> 0");
    foreach (var w in waits)
        Console.WriteLine($"  会话 {w.SessionId} 被会话 {w.BlockingSessionId} 挡住，等待类型 {w.WaitType}\n  它在执行：{w.SqlText.Trim()}");
}

try
{
    await listTask;
}
catch (SqlException ex)
{
    Console.WriteLine($"SqlException {ex.Number}：{ex.Message.Split('\n')[0]}");
}

Console.WriteLine("== 修复：阻塞方尽快提交（或回滚），事务里不做耗时的事");
tx.Commit();
var sw = Stopwatch.StartNew();
var rows = await LoadListAsync(connStr);
Console.WriteLine($"查到 {rows.Count} 行，用时 {sw.ElapsedMilliseconds} 毫秒");

// 列表页的查询本身没有任何问题：一张 3 行的表
static async Task<List<string>> LoadListAsync(string connStr)
{
    using var conn = new SqlConnection(connStr);
    var list = await conn.QueryAsync<string>(
        "SELECT RequestNo FROM dbo.PriceRequest ORDER BY Id",
        commandTimeout: 3);                    // 默认 30 秒，这里缩短到 3 秒，方便演示（第 14 章）
    return list.ToList();
}

public class WaitInfo
{
    public short SessionId { get; set; }       // session_id 列是 SMALLINT
    public short BlockingSessionId { get; set; }
    public string WaitType { get; set; } = "";
    public string SqlText { get; set; } = "";
}
