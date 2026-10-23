// code/ch15/Step3-Tvp/Program.cs
// 插入 1000 条申请明细：第 6 章的"传 List 逐条执行" vs 表值参数一次传完，各跑 3 轮比耗时
// 运行前先执行 code/sql/ch15-tvp.sql（创建 dbo.PriceRequestItemType）
using System.Data;
using System.Diagnostics;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

// 造 1000 条明细，全挂在第 3 号申请单（草稿）下，产品 3、4 交替
var items = Enumerable.Range(1, 1000)
    .Select(i => new Item
    {
        RequestId = 3,
        ProductId = i % 2 == 0 ? 3 : 4,
        Qty = 1 + i % 4,                 // 1~4
        RequestPrice = 24000m
    })
    .ToList();

using var conn = new SqlConnection(connStr);
conn.Open();   // 手动打开一次，两种方式都不包含"开连接"的时间

for (int round = 1; round <= 3; round++)
{
    // 方式 A：第 6 章的写法。Dapper 对 List 里每个元素各执行一次 INSERT —— 1000 次往返
    ResetItems(conn);
    var sw = Stopwatch.StartNew();
    int rowsA = conn.Execute(@"
        INSERT dbo.PriceRequestItem (RequestId, ProductId, Qty, RequestPrice)
        VALUES (@RequestId, @ProductId, @Qty, @RequestPrice)", items);
    long msA = sw.ElapsedMilliseconds;

    // 方式 B：表值参数。1000 行装进一个 DataTable，作为一个参数发过去 —— 1 次往返
    ResetItems(conn);
    sw.Restart();
    DataTable table = ToDataTable(items);
    int rowsB = conn.Execute(@"
        INSERT dbo.PriceRequestItem (RequestId, ProductId, Qty, RequestPrice)
        SELECT RequestId, ProductId, Qty, RequestPrice FROM @Items",
        new { Items = table.AsTableValuedParameter("dbo.PriceRequestItemType") });  // 类型名要写全：架构.类型
    long msB = sw.ElapsedMilliseconds;

    Console.WriteLine($"第 {round} 轮  逐条 Execute：{rowsA} 行 {msA,5} ms   表值参数：{rowsB} 行 {msB,4} ms");
}

// 核对最后一次（表值参数）插进去的数据：按产品分组看行数和数量合计
Console.WriteLine("核对：");
foreach (var g in conn.Query(@"
    SELECT ProductId, COUNT(*) AS Cnt, SUM(Qty) AS SumQty
    FROM dbo.PriceRequestItem WHERE Id > 5
    GROUP BY ProductId ORDER BY ProductId"))
    Console.WriteLine($"  产品 {g.ProductId}：{g.Cnt} 行，数量合计 {g.SumQty}");

// 删掉本程序加的明细（初始数据只有 Id 1~5），让下一轮从同样的起点开始
static void ResetItems(SqlConnection conn) =>
    conn.Execute("DELETE FROM dbo.PriceRequestItem WHERE Id > 5");

// DataTable 的列顺序必须和 CREATE TYPE 里的列顺序一致——SQL Server 按位置对，不按名字对
static DataTable ToDataTable(List<Item> list)
{
    var t = new DataTable();
    t.Columns.Add("RequestId", typeof(int));
    t.Columns.Add("ProductId", typeof(int));
    t.Columns.Add("Qty", typeof(int));
    t.Columns.Add("RequestPrice", typeof(decimal));
    foreach (var x in list)
    {
        DataRow r = t.NewRow();
        r["RequestId"] = x.RequestId;        // 按列名赋值，读起来清楚；但发给服务器时只看列的位置
        r["ProductId"] = x.ProductId;
        r["Qty"] = x.Qty;
        r["RequestPrice"] = x.RequestPrice;
        t.Rows.Add(r);
    }
    return t;
}

public class Item
{
    public int RequestId { get; set; }
    public int ProductId { get; set; }
    public int Qty { get; set; }
    public decimal RequestPrice { get; set; }
}
