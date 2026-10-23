// code/ch04/Exercise/Program.cs
// 第 4 章练习参考答案：练习 1（LIKE 模糊查询参数化）+ 练习 2（IN 列表 + GROUP BY）
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 练习 1：% 写在 SQL 里，用 + 和参数连起来；参数值只放用户输入的关键字本身
const string likeSql = @"
    SELECT Id, Name FROM dbo.Customer
    WHERE Name LIKE N'%' + @Kw + N'%'
    ORDER BY Id";
foreach (var kw in new[] { "检测", "医院", "' OR '1'='1" })
{
    var list = conn.Query<CustomerBrief>(likeSql, new { Kw = kw }).ToList();
    if (list.Count == 0)
        Console.WriteLine($"{kw} → 0 行");
    foreach (var c in list)
        Console.WriteLine($"{kw} → {c.Id} {c.Name}");
}

// 练习 2：IN 后面直接写 @Codes，不加括号
var codes = new[] { "PB-01", "PB-02" };
var counts = conn.Query<CodeCount>(@"
    SELECT p.Code, COUNT(*) AS Cnt
    FROM dbo.PriceRequestItem AS i
    JOIN dbo.Product AS p ON p.Id = i.ProductId
    WHERE p.Code IN @Codes
    GROUP BY p.Code
    ORDER BY p.Code", new { Codes = codes });
foreach (var c in counts)
    Console.WriteLine($"{c.Code} 出现在 {c.Cnt} 条明细里");

public class CustomerBrief
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class CodeCount
{
    public string Code { get; set; } = "";
    public int Cnt { get; set; }
}
