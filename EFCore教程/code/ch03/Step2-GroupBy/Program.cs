// code/ch03/Step2-GroupBy/Program.cs
using EfCoreStudy;

var products = SampleData.Products();

// ① 先看清分组长什么样：每组有一个 Key（分类名），组里装着这个分类的全部产品
var groups = products.GroupBy(p => p.Category).ToList();
foreach (var g in groups)
{
    Console.WriteLine($"【{g.Key}】{g.Count()} 个");
    foreach (var p in g)                 // 组本身就是一个集合，可以 foreach
        Console.WriteLine($"    {p.Code}  {p.Name}");
}

// ② 报表：每个分类的产品数、库存总量、最高单价。相当于
//    SELECT Category, COUNT(*), SUM(Stock), MAX(Price) FROM Products GROUP BY Category
var report = products
    .GroupBy(p => p.Category)
    .Select(g => new
    {
        Category = g.Key,
        Count = g.Count(),               // 汇总方法作用在"组"上，不是整个 products
        TotalStock = g.Sum(p => p.Stock),
        MaxPrice = g.Max(p => p.Price),
    })
    .ToList();
foreach (var r in report)
    Console.WriteLine($"{r.Category}  {r.Count} 个  库存 {r.TotalStock}  最高价 {r.MaxPrice:N0}");
