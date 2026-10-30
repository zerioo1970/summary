// code/ch02/Exercise/Program.cs
// 第 2 章练习参考答案：练习 1（耗材价格表）+ 练习 2（最贵的探头、按编码查）
using EfCoreStudy;

var products = SampleData.Products();

// 练习 1：Where → OrderBy → Select，顺序和 SQL 的 WHERE → ORDER BY → SELECT 一致
var priceList = products
    .Where(p => p.Category == "耗材")
    .OrderBy(p => p.Price)
    .Select(p => new { p.Name, p.Price })
    .ToList();
Console.WriteLine("耗材价格表：");
foreach (var x in priceList)
    Console.WriteLine($"  {x.Name}  {x.Price:N2}");

// 练习 2-1：先按单价降序排好，第一个就是最贵的
var top = products
    .Where(p => p.Category == "探头")
    .OrderByDescending(p => p.Price)
    .First();
Console.WriteLine($"最贵的探头：{top.Name} {top.Price:N0}");

// 练习 2-2：编码可能不存在，用 SingleOrDefault（编码唯一）再判断 null
foreach (var code in new[] { "PB-C02", "PB-X99" })
{
    var p = products.SingleOrDefault(x => x.Code == code);
    Console.WriteLine(p is null ? $"{code}：不存在" : $"{code}：{p.Name}");
}
