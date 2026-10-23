// code/ch21/Exercise/Program.cs
// 第 21 章练习 1 参考答案：去掉 IN 后面的括号（场景 3），金额别名改成属性名（场景 1）
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

var rows = conn.Query<RequestAmount>(@"
    SELECT r.RequestNo, SUM(i.Qty * i.RequestPrice) AS Amount   -- 别名 = 属性名
    FROM dbo.PriceRequest r
    JOIN dbo.PriceRequestItem i ON i.RequestId = r.Id
    WHERE r.Id IN @Ids                                           -- 不写括号，Dapper 自己展开
    GROUP BY r.Id, r.RequestNo
    ORDER BY r.Id", new { Ids = new[] { 1, 3 } });

foreach (var r in rows)
    Console.WriteLine($"{r.RequestNo}{r.Amount,14:N2}");

public class RequestAmount
{
    public string RequestNo { get; set; } = "";
    public decimal Amount { get; set; }
}
