// code/ch18/Step4-SmokeTests/ItemRepository.cs
// 被测试的仓储：和 Step2-AfterChange 里的 ItemRepository、RequestItem 完全相同（复制过来，让测试项目自己就能跑）
using Dapper;
using Microsoft.Data.SqlClient;

public class ItemRepository
{
    private readonly string _connStr;

    public ItemRepository(string connStr) => _connStr = connStr;

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
    public decimal? Discount { get; set; }
    public decimal Amount => Qty * RequestPrice;
    public decimal NetAmount => Qty * RequestPrice * (Discount ?? 1m);
}
