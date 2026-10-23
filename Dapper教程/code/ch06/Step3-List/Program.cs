// code/ch06/Step3-List/Program.cs
// 把一个集合传给 Execute：Dapper 对每个元素执行一次 SQL，返回受影响行数的总和
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

const string insertItemSql = @"
    INSERT dbo.PriceRequestItem (RequestId, ProductId, Qty, RequestPrice)
    VALUES (@RequestId, @ProductId, @Qty, @RequestPrice)";

// 查服务器计划缓存：上面这条 INSERT 一共执行了几次
// sys.dm_exec_sql_text 第 4 章用过；sys.dm_exec_query_stats 多了一列 execution_count（执行次数）
const string execCountSql = @"
    SELECT SUM(qs.execution_count)
    FROM sys.dm_exec_query_stats qs
    CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) t
    WHERE t.text LIKE '%INSERT dbo.PriceRequestItem%'
      AND t.text NOT LIKE '%dm_exec_query_stats%'";   // 排除这条统计 SQL 自己

// 例 1：给 3 号申请单追加 3 条明细 —— 参数传 List<NewItem>
var items = new List<NewItem>
{
    new NewItem { RequestId = 3, ProductId = 1, Qty = 1, RequestPrice = 170000m },
    new NewItem { RequestId = 3, ProductId = 2, Qty = 1, RequestPrice = 400000m },
    new NewItem { RequestId = 3, ProductId = 3, Qty = 6, RequestPrice =  23800m },
};

int before = conn.ExecuteScalar<int?>(execCountSql) ?? 0;   // 从没执行过时是 NULL，第 5 章讲过用 int?
int rows = conn.Execute(insertItemSql, items);              // 一行代码，背后是 3 次执行
int after = conn.ExecuteScalar<int?>(execCountSql) ?? 0;

Console.WriteLine($"例 1 传 3 个对象：Execute 返回 {rows}，服务器上这条 INSERT 多执行了 {after - before} 次");
Console.WriteLine($"     3 号单现在有 {CountItems(3)} 条明细");

// 例 2：匿名对象数组也行。批量改目录价，第 3 个编码不存在
int changed = conn.Execute(
    "UPDATE dbo.Product SET ListPrice = @ListPrice WHERE Code = @Code",
    new[]
    {
        new { Code = "PB-01", ListPrice = 25000m },
        new { Code = "PB-02", ListPrice = 23000m },
        new { Code = "XX-00", ListPrice = 1m },      // 不存在：这一次影响 0 行
    });
Console.WriteLine($"例 2 改 3 个产品的价：Execute 返回 {changed}（1 + 1 + 0）");

// 例 3：中间一条失败会怎样？给 2 号单插 3 条，第 2 条的 ProductId = 99 不存在
var bad = new List<NewItem>
{
    new NewItem { RequestId = 2, ProductId = 1,  Qty = 1, RequestPrice = 170000m },
    new NewItem { RequestId = 2, ProductId = 99, Qty = 1, RequestPrice = 1m },      // 撞外键
    new NewItem { RequestId = 2, ProductId = 3,  Qty = 1, RequestPrice = 23800m },
};

Console.WriteLine($"例 3 执行前：2 号单有 {CountItems(2)} 条明细");
try
{
    conn.Execute(insertItemSql, bad);
}
catch (SqlException ex)
{
    Console.WriteLine($"     第 2 条失败：Number = {ex.Number}");
}
Console.WriteLine($"     执行后：2 号单有 {CountItems(2)} 条明细  ← 第 1 条已经写进去了，第 3 条没执行");

int CountItems(int requestId) => conn.ExecuteScalar<int>(
    "SELECT COUNT(*) FROM dbo.PriceRequestItem WHERE RequestId = @requestId", new { requestId });

public class NewItem
{
    public int RequestId { get; set; }
    public int ProductId { get; set; }
    public int Qty { get; set; }
    public decimal RequestPrice { get; set; }
}
