// code/ch02/Exercise2-Bug/Program.cs
// 练习 2 的原题：运行后价格全是 0.00。不要改 SQL，找出原因并修好。
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

var products = conn.Query<ProductPrice>("SELECT Code, ListPrice FROM dbo.Product ORDER BY Id");
foreach (var p in products)
    Console.WriteLine($"{p.Code,-8}{p.ListPirce,12:N2}");

public class ProductPrice
{
    public string Code { get; set; } = "";
    public decimal ListPirce { get; set; }
}
