// code/ch01/Exercise/Program.cs
// 第 1 章练习参考答案：练习 1（产品列表）+ 练习 2（待审单数）
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);
conn.Open();

// 练习 1：产品列表
var products = new List<Product>();
using (var cmd = new SqlCommand("SELECT Id, Code, Name, ListPrice FROM dbo.Product ORDER BY Id", conn))
using (var reader = cmd.ExecuteReader())
{
    while (reader.Read())
    {
        products.Add(new Product
        {
            Id        = reader.GetInt32(reader.GetOrdinal("Id")),
            Code      = reader.GetString(reader.GetOrdinal("Code")),
            Name      = reader.GetString(reader.GetOrdinal("Name")),
            ListPrice = reader.GetDecimal(reader.GetOrdinal("ListPrice")),  // DECIMAL 列要用 GetDecimal
        });
    }
}
foreach (var p in products)
    Console.WriteLine($"{p.Code,-8}{p.Name,-10}{p.ListPrice,12:N2}");   // N2 = 千分位 + 两位小数

// 练习 2：待审（Status = 1）的申请单数量
using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.PriceRequest WHERE Status = 1", conn))
{
    var pending = (int)cmd.ExecuteScalar()!;   // COUNT(*) 返回 int；COUNT_BIG 才是 long
    Console.WriteLine($"待审申请单：{pending} 张");
}

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal ListPrice { get; set; }
}
