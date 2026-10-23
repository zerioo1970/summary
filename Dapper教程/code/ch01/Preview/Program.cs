// code/ch01/Preview/Program.cs
// 第 1 章结尾的预览：和 PriceDemo 读同一张表，换成 Dapper 写。第 2 章再逐行讲解。
using Dapper;                      // 引入 Dapper 的扩展方法
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);   // 不用手动 Open：Dapper 发现连接是关闭的，会自己打开再关上

// 这一行替代了 PriceDemo 里 while (reader.Read()) 那一整段
var customers = conn.Query<Customer>("SELECT Id, Name, City, Level FROM dbo.Customer ORDER BY Id").ToList();

foreach (var c in customers)
    Console.WriteLine($"{c.Id}  {c.Name}  {c.City}  {c.Level}");

Console.WriteLine($"共 {customers.Count} 个客户");

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
    public string Level { get; set; } = "";
}
