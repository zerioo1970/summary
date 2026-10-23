// code/ch12/Step4-VsTwoQueries/Program.cs
// 与第 11 章"方案二"对比：主表 + 明细分两个结果集，一次往返取回，再在内存里按 RequestId 分组
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 第 11 章方案二要先查主表拿到 ids，再发第二条 WHERE RequestId IN @ids。
// 这里把"筛主表的条件"原样写进第二条的子查询，两条 SQL 就不再互相等待，可以放进同一个批。
const string sql = @"
SELECT Id, RequestNo, Applicant, Status
FROM dbo.PriceRequest
WHERE Applicant = @Applicant
ORDER BY Id;

SELECT RequestId, ProductId, Qty, RequestPrice
FROM dbo.PriceRequestItem
WHERE RequestId IN (SELECT Id FROM dbo.PriceRequest WHERE Applicant = @Applicant)
ORDER BY RequestId, Id;";

List<Request> requests;
List<Item> allItems;
using (var grid = conn.QueryMultiple(sql, new { Applicant = "张伟" }))
{
    requests = grid.Read<Request>().ToList();
    allItems = grid.Read<Item>().ToList();
}

// 内存分组：和第 11 章方案二的做法完全一样
var itemsByRequest = allItems.ToLookup(i => i.RequestId);
foreach (var r in requests)
    r.Items = itemsByRequest[r.Id].ToList();   // 某单没有明细时得到空集合，不是 null

foreach (var r in requests)
{
    Console.WriteLine($"{r.RequestNo}  {r.Applicant}  状态={r.Status}  明细 {r.Items.Count} 条");
    foreach (var i in r.Items)
        Console.WriteLine($"    产品 {i.ProductId} × {i.Qty} @ {i.RequestPrice:N2}");
}

public class Request
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
    public int Status { get; set; }
    public List<Item> Items { get; set; } = new();   // 结果集里没有 Items 列，Dapper 不碰它
}

public class Item
{
    public int RequestId { get; set; }
    public int ProductId { get; set; }
    public int Qty { get; set; }
    public decimal RequestPrice { get; set; }
}
