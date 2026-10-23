// code/ch18/Exercise/ItemRepository.cs
// 练习参考答案：列 Discount 改名为 DiscountRate 后，只改 SQL（用别名），C# 属性名不动
using Dapper;
using Microsoft.Data.SqlClient;

public class ItemRepository
{
    private readonly string _connStr;

    public ItemRepository(string connStr) => _connStr = connStr;

    // 改动 1：列清单里用别名，把新列名 DiscountRate 映射回原来的属性 Discount
    private const string Columns = "Id, RequestId, ProductId, Qty, RequestPrice, DiscountRate AS Discount";

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

    // 改动 2：合计公式不走列清单，要单独改。"在文件中查找 Discount" 能把它找出来
    public decimal GetTotal(int requestId)
    {
        using var conn = new SqlConnection(_connStr);
        return conn.ExecuteScalar<decimal>(
            "SELECT ISNULL(SUM(Qty * RequestPrice * ISNULL(DiscountRate, 1)), 0) FROM dbo.PriceRequestItem WHERE RequestId = @RequestId",
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
    public decimal? Discount { get; set; }      // 不改名：调用方（页面、报表）一行都不用动
    public decimal Amount => Qty * RequestPrice;
    public decimal NetAmount => Qty * RequestPrice * (Discount ?? 1m);
}
