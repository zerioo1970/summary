// code/ch18/Step1-Before/Program.cs
// 改需求之前的样子：明细仓储有 3 个方法，表里还没有 Discount 列。这是本章的起点
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

var repo = new ItemRepository(connStr);

// 方法 1：一张申请单的全部明细
foreach (var i in repo.GetByRequestId(1))
    Console.WriteLine($"明细 {i.Id}：产品 {i.ProductId}  {i.Qty} × {i.RequestPrice:N2} = {i.Amount:N2}");

// 方法 2：按明细 Id 查一行
var one = repo.GetById(3);
Console.WriteLine($"明细 3：申请单 {one?.RequestId}  产品 {one?.ProductId}  金额 {one?.Amount:N2}");

// 方法 3：合计在 SQL 里算。C# 里逐行相加，两边应该一致
decimal sumInCode = repo.GetByRequestId(1).Sum(i => i.Amount);
decimal sumInSql = repo.GetTotal(1);
Console.WriteLine($"第 1 单合计：C# 算 {sumInCode:N2}，SQL 算 {sumInSql:N2}，一致吗？{sumInCode == sumInSql}");

// 一张表一个仓储（第 16 章）。本章为了"一个文件就能跑"，直接把连接串传进来；正式项目用第 16 章的连接工厂
public class ItemRepository
{
    private readonly string _connStr;

    public ItemRepository(string connStr) => _connStr = connStr;

    // 列清单只写一处：两个查询共用，加列时只改这一行
    private const string Columns = "Id, RequestId, ProductId, Qty, RequestPrice";

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

    // 金额公式写在 SQL 里——改需求时最容易漏掉的就是这种"不在列清单里"的地方
    public decimal GetTotal(int requestId)
    {
        using var conn = new SqlConnection(_connStr);
        return conn.ExecuteScalar<decimal>(
            "SELECT ISNULL(SUM(Qty * RequestPrice), 0) FROM dbo.PriceRequestItem WHERE RequestId = @RequestId",
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
    public decimal Amount => Qty * RequestPrice;   // 只读计算属性，Dapper 不会往里写
}
