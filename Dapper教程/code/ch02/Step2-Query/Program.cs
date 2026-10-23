// code/ch02/Step2-Query/Program.cs
// Query<T> 的四种常见用法：查对象、查单列、查不到、连接状态
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 例 1：一行 → 一个对象，多行 → 多个对象
IEnumerable<Product> products = conn.Query<Product>(
    "SELECT Id, Code, Name, ListPrice FROM dbo.Product ORDER BY Id");
Console.WriteLine("== 例 1 产品");
foreach (var p in products)
    Console.WriteLine($"{p.Code,-8}{p.Name,-10}{p.ListPrice,12:N2}");

// 例 2：只查一列时，T 直接写列的类型，拿到的就是 List<string> / List<decimal>
List<string> cities = conn.Query<string>(
    "SELECT DISTINCT City FROM dbo.Customer ORDER BY City").ToList();
Console.WriteLine("== 例 2 城市：" + string.Join("、", cities));

List<decimal> prices = conn.Query<decimal>(
    "SELECT RequestPrice FROM dbo.PriceRequestItem WHERE RequestId = 1").ToList();
Console.WriteLine("== 例 2 第 1 单申请价合计：" + prices.Sum().ToString("N2"));

// 例 3：查不到数据时，返回的是空集合，不是 null——可以直接 foreach / Count
var none = conn.Query<Product>("SELECT Id, Code, Name, ListPrice FROM dbo.Product WHERE 1 = 0");
Console.WriteLine($"== 例 3 查不到：Count = {none.Count()}，是 null 吗？{none is null}");

// 例 4：连接状态——没 Open 时 Dapper 自己开、用完自己关；你 Open 了它就不替你关
Console.WriteLine($"== 例 4 查询前：{conn.State}");
conn.Query<int>("SELECT 1");
Console.WriteLine($"   Dapper 查完（没手动 Open）：{conn.State}");
conn.Open();
conn.Query<int>("SELECT 1");
Console.WriteLine($"   手动 Open 后再查：{conn.State}");

// 返回类型是 IEnumerable<T>，但默认已经全部读进内存（缓冲模式，第 3 章讲），实际类型是：
Console.WriteLine($"== 实际类型：{products.GetType().Name}");

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal ListPrice { get; set; }
}
