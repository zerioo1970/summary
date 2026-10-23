// code/ch16/Step1-Pooling/Program.cs
// 连接池：反复 Open/Close 其实一直用同一条物理连接；关掉连接池再比耗时、看服务器上的连接数
using System.Diagnostics;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

// 关掉连接池的版本：用 SqlConnectionStringBuilder 改一个键，比手工拼字符串安全
string noPoolStr = new SqlConnectionStringBuilder(connStr) { Pooling = false }.ConnectionString;

// 例 1：连续 3 次 new → Open → Close，看每次拿到的是不是同一条物理连接
Console.WriteLine("== 例 1 物理连接编号（ClientConnectionId 前 8 位）");
ShowIds("默认（有连接池）", connStr);
ShowIds("Pooling=false   ", noPoolStr);

// 例 2：1000 次 Open/Close 的耗时，跑 3 轮
Console.WriteLine("== 例 2 1000 次 Open/Close 耗时");
for (int round = 1; round <= 3; round++)
{
    long pooled = OpenClose(connStr, 1000);
    long noPool = OpenClose(noPoolStr, 1000);
    Console.WriteLine($"第 {round} 轮  默认：{pooled,5} ms   Pooling=false：{noPool,5} ms");
}

// 例 3：从服务器这边数一数，到底有几条连接
Console.WriteLine("== 例 3 服务器上的连接数");
SqlConnection.ClearAllPools();                       // 先清空，从 0 开始
var a = new SqlConnection(connStr);
var b = new SqlConnection(connStr);
var c = new SqlConnection(connStr);
a.Open(); b.Open(); c.Open();                        // 同时开着 3 个：池里只能给出 3 条不同的物理连接
Console.WriteLine($"3 个连接同时 Open：{CountSessions()} 条");
a.Close(); b.Close(); c.Close();                     // Close 只是"还给池"，物理连接还在
Console.WriteLine($"3 个连接都 Close 后：{CountSessions()} 条");
SqlConnection.ClearAllPools();                       // 清空连接池，这时才真正断开
Thread.Sleep(500);                                   // 给服务器一点时间清理会话
Console.WriteLine($"ClearAllPools 之后：{CountSessions()} 条");

void ShowIds(string label, string cs)
{
    var ids = new List<string>();
    for (int i = 0; i < 3; i++)
    {
        using var conn = new SqlConnection(cs);
        conn.Open();
        ids.Add(conn.ClientConnectionId.ToString()[..8]);   // 每条物理连接有唯一编号
    }
    Console.WriteLine($"{label}：{string.Join("  ", ids)}");
}

static long OpenClose(string cs, int times)
{
    var sw = Stopwatch.StartNew();
    for (int i = 0; i < times; i++)
    {
        using var conn = new SqlConnection(cs);
        conn.Open();
    }   // 每次循环结束 Dispose → Close：有连接池时只是还回池里，不断开
    return sw.ElapsedMilliseconds;
}

// 用一条不进池的连接去查：PriceDemo 库里、除了我自己以外的用户会话有几个
int CountSessions()
{
    using var monitor = new SqlConnection(noPoolStr);
    return monitor.ExecuteScalar<int>(@"
        SELECT COUNT(*) FROM sys.dm_exec_sessions
        WHERE is_user_process = 1
          AND database_id = DB_ID('PriceDemo')
          AND session_id <> @@SPID");
}
