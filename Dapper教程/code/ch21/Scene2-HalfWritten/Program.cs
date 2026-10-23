// code/ch21/Scene2-HalfWritten/Program.cs
// 场景 2：数据只写了一半——主表插进去了，明细只进了一条
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 为了能反复运行：先删掉上一次运行留下的两张演示单据（先删明细再删主表）
conn.Execute(@"
    DELETE i FROM dbo.PriceRequestItem i JOIN dbo.PriceRequest r ON r.Id = i.RequestId
    WHERE r.RequestNo IN ('PR20261101', 'PR20261102');
    DELETE dbo.PriceRequest WHERE RequestNo IN ('PR20261101', 'PR20261102');");

const string insertHead = @"
    INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant)
    OUTPUT INSERTED.Id
    VALUES (@RequestNo, @CustomerId, @Applicant)";
const string insertItem = @"
    INSERT dbo.PriceRequestItem (RequestId, ProductId, Qty, RequestPrice)
    VALUES (@RequestId, @ProductId, @Qty, @RequestPrice)";

Console.WriteLine("== 故障现象：没有事务");
try
{
    int id = conn.QuerySingle<int>(insertHead,
        new { RequestNo = "PR20261101", CustomerId = 2, Applicant = "王芳" });
    // 传数组：Dapper 逐条执行（第 6 章）。第 2 条的产品 99 不存在，违反外键
    conn.Execute(insertItem, new[]
    {
        new { RequestId = id, ProductId = 1, Qty = 1, RequestPrice = 170000m },
        new { RequestId = id, ProductId = 99, Qty = 1, RequestPrice = 20000m },
    });
}
catch (SqlException ex)
{
    Console.WriteLine($"SqlException {ex.Number}：{ex.Message}");
}

Console.WriteLine("== 定位：每张单子各有几条明细");
PrintItemCounts(conn);

Console.WriteLine("== 修复：主表和明细放进同一个事务（第 9 章）");
conn.Open();                                   // 事务要求连接先打开
using (var tx = conn.BeginTransaction())
{
    try
    {
        int id = conn.QuerySingle<int>(insertHead,
            new { RequestNo = "PR20261102", CustomerId = 2, Applicant = "王芳" },
            transaction: tx);                  // 每条命令都要带上 transaction
        conn.Execute(insertItem, new[]
        {
            new { RequestId = id, ProductId = 1, Qty = 1, RequestPrice = 170000m },
            new { RequestId = id, ProductId = 99, Qty = 1, RequestPrice = 20000m },
        }, transaction: tx);
        tx.Commit();
    }
    catch (SqlException ex)
    {
        tx.Rollback();                         // 任何一条失败，三条一起撤销
        Console.WriteLine($"已回滚。SqlException {ex.Number}");
    }
}
PrintItemCounts(conn);

static void PrintItemCounts(SqlConnection conn)
{
    var rows = conn.Query<HeadCount>(@"
        SELECT r.RequestNo, COUNT(i.Id) AS ItemCount
        FROM dbo.PriceRequest r
        LEFT JOIN dbo.PriceRequestItem i ON i.RequestId = r.Id
        GROUP BY r.Id, r.RequestNo
        ORDER BY r.Id");
    foreach (var r in rows)
        Console.WriteLine($"  {r.RequestNo}  明细 {r.ItemCount} 条");
}

public class HeadCount
{
    public string RequestNo { get; set; } = "";
    public int ItemCount { get; set; }
}
