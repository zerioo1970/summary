// code/ch11/Step3-TwoQueries/Program.cs
// 方案二：先查主表（可以放心分页），再用 IN @ids 一次查出这些单的明细，在内存里分组挂上去
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 第 1 次查询：只查主表。TOP 2 数的是"单据"，不会被明细行数干扰
List<PriceRequest> requests = conn.Query<PriceRequest>(@"
    SELECT TOP 2 Id, RequestNo, Applicant
    FROM dbo.PriceRequest
    ORDER BY Id").ToList();

// 第 2 次查询：一次查出这几张单的全部明细。IN @ids 不加括号，Dapper 会展开（第 4 章讲过）
List<int> ids = requests.Select(r => r.Id).ToList();
List<RequestItem> items = conn.Query<RequestItem>(@"
    SELECT Id, RequestId, ProductId, Qty, RequestPrice
    FROM dbo.PriceRequestItem
    WHERE RequestId IN @ids
    ORDER BY Id", new { ids }).ToList();
Console.WriteLine($"主表 {requests.Count} 行，明细 {items.Count} 行，共 2 次查询");

// 在内存里按 RequestId 分组。ToLookup 查不存在的 key 返回空序列，不会抛异常
ILookup<int, RequestItem> itemsByRequest = items.ToLookup(i => i.RequestId);
foreach (var r in requests)
    r.Items = itemsByRequest[r.Id].ToList();

foreach (var r in requests)
{
    Console.WriteLine($"{r.RequestNo}  {r.Applicant}  明细 {r.Items.Count} 条  合计 {r.Items.Sum(x => x.Amount),12:N2}");
    foreach (var i in r.Items)
        Console.WriteLine($"    产品 {i.ProductId}：{i.Qty} × {i.RequestPrice:N2}");
}

public class PriceRequest
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
    public List<RequestItem> Items { get; set; } = new();
}

public class RequestItem
{
    public int Id { get; set; }
    public int RequestId { get; set; }   // 方案二要靠它在内存里分组，所以必须查出来
    public int ProductId { get; set; }
    public int Qty { get; set; }
    public decimal RequestPrice { get; set; }
    public decimal Amount => Qty * RequestPrice;
}
