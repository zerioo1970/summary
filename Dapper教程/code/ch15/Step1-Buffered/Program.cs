// code/ch15/Step1-Buffered/Program.cs
// 同样读 20 万行：缓冲（默认）和流式（buffered: false）各跑 3 轮，比较内存和耗时
using System.Diagnostics;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

// 不建表：用系统视图 sys.all_objects 自己交叉连接自己，凭空造出 20 万行"报表明细"
const string sql = @"
SELECT Id,
       CONCAT('PR', 1000000 + Id)       AS RequestNo,
       CAST(Id % 1000 AS DECIMAL(18,2)) AS Amount
FROM (SELECT TOP (200000) CAST(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS INT) AS Id
      FROM sys.all_objects a CROSS JOIN sys.all_objects b) t";

// 跑 3 轮：第 1 轮包含"第一次执行"的额外开销（第 3 章讲过的映射代码生成），看第 2、3 轮更准
for (int round = 1; round <= 3; round++)
{
    Console.WriteLine($"== 第 {round} 轮");
    Measure("缓冲 buffered: true ", buffered: true);
    Measure("流式 buffered: false", buffered: false);
}

void Measure(string label, bool buffered)
{
    using var conn = new SqlConnection(connStr);

    // forceFullCollection: true —— 先做一次完整垃圾回收，量到的才是"真正还活着"的对象占用
    long before = GC.GetTotalMemory(forceFullCollection: true);
    var sw = Stopwatch.StartNew();

    int count = 0;
    decimal total = 0;
    long halfway = 0;
    string stateAtHalf = "";

    foreach (var row in conn.Query<ReportRow>(sql, buffered: buffered))
    {
        count++;
        total += row.Amount;
        if (count == 100_000)
        {
            // 读到一半时量一次：缓冲模式此时 20 万个对象已经全在内存里；流式模式只有手上这一个
            halfway = GC.GetTotalMemory(forceFullCollection: true) - before;
            stateAtHalf = conn.State.ToString();
        }
    }
    sw.Stop();

    Console.WriteLine($"{label}  行数 {count:N0}  合计 {total:N0}  " +
                      $"读到一半时内存 +{halfway / 1024.0 / 1024:F1} MB  连接 {stateAtHalf}  耗时 {sw.ElapsedMilliseconds} ms");
}

public class ReportRow
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public decimal Amount { get; set; }
}
