// code/ch03/Exercise/Program.cs
// 第 3 章练习参考答案：练习 1（汇总）+ 练习 2（相当于 HAVING 的分组筛选）
using EfCoreStudy;

var products = SampleData.Products();

// 练习 1：三个汇总，各返回一个值
int outOfStock = products.Count(p => p.Stock == 0);
bool over500k = products.Any(p => p.Price > 500000m);
decimal avgProbe = products.Where(p => p.Category == "探头").Average(p => p.Price);
Console.WriteLine($"缺货产品：{outOfStock} 个");
Console.WriteLine($"有单价超过 50 万的吗：{over500k}");
Console.WriteLine($"探头平均单价：{avgProbe:N0}");

// 练习 2：GroupBy 之后再 Where，筛的是"组"，相当于 SQL 的 HAVING SUM(Stock) > 10
var bigStock = products
    .GroupBy(p => p.Category)
    .Select(g => new { Category = g.Key, TotalStock = g.Sum(p => p.Stock) })
    .Where(x => x.TotalStock > 10)
    .ToList();
Console.WriteLine("库存总量超过 10 的分类：");
foreach (var x in bigStock)
    Console.WriteLine($"  {x.Category}  {x.TotalStock}");
