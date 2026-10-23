// code/ch21/Exercise1-Bug/Program.cs
// 练习 1 的原题：查第 1、3 号单据的金额。先报错，修掉报错后金额又全是 0。
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

var rows = conn.Query<RequestAmount>(@"
    SELECT r.RequestNo, SUM(i.Qty * i.RequestPrice) AS 金额
    FROM dbo.PriceRequest r
    JOIN dbo.PriceRequestItem i ON i.RequestId = r.Id
    WHERE r.Id IN (@Ids)
    GROUP BY r.Id, r.RequestNo
    ORDER BY r.Id", new { Ids = new[] { 1, 3 } });

foreach (var r in rows)
    Console.WriteLine($"{r.RequestNo}{r.Amount,14:N2}");

public class RequestAmount
{
    public string RequestNo { get; set; } = "";
    public decimal Amount { get; set; }
}
