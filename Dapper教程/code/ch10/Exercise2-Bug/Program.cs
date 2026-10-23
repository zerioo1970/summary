// code/ch10/Exercise2-Bug/Program.cs
// 练习 2 的原题：客户名称能显示，城市却全是空的。类不许改，找出原因并修好。
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

var requests = conn.Query<PriceRequest, Customer, PriceRequest>(@"
    SELECT r.Id, r.RequestNo, c.City, c.Id, c.Name
    FROM dbo.PriceRequest r
    JOIN dbo.Customer c ON c.Id = r.CustomerId
    ORDER BY r.Id",
    (request, customer) => { request.Customer = customer; return request; });

foreach (var r in requests)
    Console.WriteLine($"{r.RequestNo}  {r.Customer!.Name}  [{r.Customer.City}]");

public class PriceRequest
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public Customer? Customer { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
}
