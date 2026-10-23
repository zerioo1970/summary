// code/ch18/Step2-AfterChange/Program.cs
// 按"改 SQL → 改类 → 验证"三步加上折扣：列清单、合计公式、RequestItem 类各改一处
// 运行前先执行 code/sql/ch18-add-discount.sql
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

var repo = new ItemRepository(connStr);

// 方法 1：一张申请单的全部明细，现在多了折扣
foreach (var i in repo.GetByRequestId(1))
    Console.WriteLine($"明细 {i.Id}：产品 {i.ProductId}  {i.Qty} × {i.RequestPrice:N2}" +
                      $" × {(i.Discount is null ? "无折扣" : i.Discount.Value.ToString("0.00"))} = {i.NetAmount:N2}");

// 方法 2：按明细 Id 查一行
var one = repo.GetById(3);
Console.WriteLine($"明细 3：申请单 {one?.RequestId}  产品 {one?.ProductId}  折扣 {one?.Discount}  折后 {one?.NetAmount:N2}");

// 第 3 步"验证"：同一个数用两条路算一遍，对不上就说明有地方漏改了
decimal sumInCode = repo.GetByRequestId(1).Sum(i => i.NetAmount);
decimal sumInSql = repo.GetTotal(1);
Console.WriteLine($"第 1 单折后合计：C# 算 {sumInCode:N2}，SQL 算 {sumInSql:N2}，一致吗？{sumInCode == sumInSql}");

public class ItemRepository
{
    private readonly string _connStr;

    public ItemRepository(string connStr) => _connStr = connStr;

    // 第 1 步（改 SQL）：列清单加上 Discount。两个查询共用这一行，所以只改一处
    private const string Columns = "Id, RequestId, ProductId, Qty, RequestPrice, Discount";

    public List<RequestItem> GetByRequestId(int requestId)
    {
        using var conn = new SqlConnection(_connStr);
        return conn.Query<RequestItem>(
            $"SELECT {Columns} FROM dbo.PriceRequestItem WHERE RequestId = @RequestId ORDER BY Id",
            new { RequestId = requestId }).ToList();
    }

    public RequestItem? GetById(int id)
    {
        using var conn = new SqlConnection(_connStr);
        return conn.QuerySingleOrDefault<RequestItem>(
            $"SELECT {Columns} FROM dbo.PriceRequestItem WHERE Id = @Id", new { Id = id });
    }

    // 第 1 步（改 SQL）的第二处：合计公式也要乘折扣。NULL 表示不打折，按 1 算
    public decimal GetTotal(int requestId)
    {
        using var conn = new SqlConnection(_connStr);
        return conn.ExecuteScalar<decimal>(
            "SELECT ISNULL(SUM(Qty * RequestPrice * ISNULL(Discount, 1)), 0) FROM dbo.PriceRequestItem WHERE RequestId = @RequestId",
            new { RequestId = requestId });
    }
}

public class RequestItem
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public int ProductId { get; set; }
    public int Qty { get; set; }
    public decimal RequestPrice { get; set; }
    public decimal? Discount { get; set; }                              // 第 2 步（改类）：列允许 NULL，所以用 decimal?（第 7 章）
    public decimal Amount => Qty * RequestPrice;                        // 原价金额，保留不动：别处可能还在用
    public decimal NetAmount => Qty * RequestPrice * (Discount ?? 1m);  // 折后金额：规则和 SQL 里的 ISNULL(Discount, 1) 一致
}
