// code/ch02/Exercise/Program.cs
// 第 2 章练习参考答案：练习 1（别名映射申请明细）+ 练习 2（找出价格全是 0 的原因）
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 练习 1：第 3 号申请单的明细。列 RequestPrice 起别名 Price，对上属性 Price
var items = conn.Query<ItemView>(@"
    SELECT ProductId, Qty, RequestPrice AS Price
    FROM dbo.PriceRequestItem
    WHERE RequestId = 3
    ORDER BY Id").ToList();

foreach (var i in items)
    Console.WriteLine($"产品 {i.ProductId}：{i.Qty} × {i.Price:N2} = {i.Amount:N2}");
Console.WriteLine($"合计：{items.Sum(i => i.Amount):N2}");

// 练习 2：原题里属性拼成了 ListPirce，Dapper 找不到同名列，静默保持默认值 0。
// 改成与列同名的 ListPrice 后价格恢复正常。
var products = conn.Query<ProductPrice>("SELECT Code, ListPrice FROM dbo.Product ORDER BY Id");
foreach (var p in products)
    Console.WriteLine($"{p.Code,-8}{p.ListPrice,12:N2}");

public class ItemView
{
    public int ProductId { get; set; }
    public int Qty { get; set; }
    public decimal Price { get; set; }
    public decimal Amount => Qty * Price;   // 只读计算属性：没有 set，Dapper 不会往里写，也不会报错
}

public class ProductPrice
{
    public string Code { get; set; } = "";
    public decimal ListPrice { get; set; }  // 原题是 ListPirce
}
