// code/ch12/Exercise/Program.cs
// 第 12 章练习参考答案：客户概览——客户信息、该客户的申请单、申请总金额，一次 QueryMultiple 取回
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

const string sql = @"
SELECT Id, Name, City, Level
FROM dbo.Customer
WHERE Id = @CustomerId;                         -- 第 1 个：客户（1 行）

SELECT RequestNo, Status
FROM dbo.PriceRequest
WHERE CustomerId = @CustomerId
ORDER BY Id;                                    -- 第 2 个：申请单（多行）

SELECT SUM(i.Qty * i.RequestPrice)
FROM dbo.PriceRequestItem i
JOIN dbo.PriceRequest r ON r.Id = i.RequestId
WHERE r.CustomerId = @CustomerId;               -- 第 3 个：总金额（1 行 1 列，可能是 NULL）";

using (var grid = conn.QueryMultiple(sql, new { CustomerId = 1 }))
{
    var customer = grid.ReadSingle<Customer>();
    var requests = grid.Read<RequestBrief>().ToList();
    decimal? amount = grid.ReadSingle<decimal?>();   // 客户没有任何明细时 SUM 是 NULL，用 decimal? 接

    Console.WriteLine($"客户：{customer.Name}（{customer.City}，{customer.Level} 级）");
    foreach (var r in requests)
        Console.WriteLine($"  {r.RequestNo}  状态={r.Status}");
    Console.WriteLine($"申请总金额：{amount ?? 0:N2}");
    Console.WriteLine($"全部读完：IsConsumed={grid.IsConsumed}");
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
    public string Level { get; set; } = "";
}

public class RequestBrief
{
    public string RequestNo { get; set; } = "";
    public int Status { get; set; }
}
