// code/ch11/Step1-Duplicate/Program.cs
// 照搬第 10 章一对一的写法查"申请单 + 明细"：JOIN 后一行一个对象，主对象重复了
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 例 1：先看 JOIN 之后数据库返回了几行
int rowCount = conn.ExecuteScalar<int>(@"
    SELECT COUNT(*)
    FROM dbo.PriceRequest r
    JOIN dbo.PriceRequestItem i ON i.RequestId = r.Id");
Console.WriteLine($"例 1 申请单 3 张，JOIN 明细后返回 {rowCount} 行");

// 例 2：用第 10 章的写法映射——每一行都会 new 一个 PriceRequest
const string sql = @"
    SELECT r.Id, r.RequestNo, r.Applicant,
           i.Id, i.ProductId, i.Qty, i.RequestPrice
    FROM dbo.PriceRequest r
    JOIN dbo.PriceRequestItem i ON i.RequestId = r.Id
    ORDER BY r.Id, i.Id";

var requests = conn.Query<PriceRequest, RequestItem, PriceRequest>(
    sql,
    (request, item) =>
    {
        request.Items.Add(item);   // 把这一行的明细加进这一行的申请单
        return request;
    }).ToList();

Console.WriteLine($"例 2 得到 {requests.Count} 个 PriceRequest 对象：");
foreach (var r in requests)
    Console.WriteLine($"  {r.RequestNo}  明细 {r.Items.Count} 条  合计 {r.Items.Sum(x => x.Amount),12:N2}");

// 例 3：列表页想取"前 2 张单据"，直接在 JOIN 上写 TOP 2
var top2 = conn.Query<PriceRequest, RequestItem, PriceRequest>(@"
    SELECT TOP 2 r.Id, r.RequestNo, r.Applicant,
           i.Id, i.ProductId, i.Qty, i.RequestPrice
    FROM dbo.PriceRequest r
    JOIN dbo.PriceRequestItem i ON i.RequestId = r.Id
    ORDER BY r.Id, i.Id",
    (request, item) => { request.Items.Add(item); return request; }).ToList();
Console.WriteLine("例 3 TOP 2 拿到的单号：" + string.Join("、", top2.Select(r => r.RequestNo)));

public class PriceRequest
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
    public List<RequestItem> Items { get; set; } = new();   // 一对多：一张单有多条明细
}

public class RequestItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Qty { get; set; }
    public decimal RequestPrice { get; set; }
    public decimal Amount => Qty * RequestPrice;   // 金额 = 数量 × 申请价
}
