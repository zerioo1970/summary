// code/ch03/Step1-Aggregate/Program.cs
using EfCoreStudy;

var products = SampleData.Products();

// 汇总方法返回"一个值"，不是集合，所以末尾不用 ToList()
int total = products.Count();                                  // COUNT(*)
int probes = products.Count(p => p.Category == "探头");         // COUNT(*) ... WHERE
decimal stockValue = products.Sum(p => p.Price * p.Stock);     // SUM(Price * Stock)
decimal maxPrice = products.Max(p => p.Price);                 // MAX(Price)
decimal avgConsumable = products
    .Where(p => p.Category == "耗材")
    .Average(p => p.Price);                                    // AVG(Price) ... WHERE

Console.WriteLine($"产品总数：{total}");
Console.WriteLine($"探头个数：{probes}");
Console.WriteLine($"库存总金额：{stockValue:N0}");
Console.WriteLine($"最高单价：{maxPrice:N0}");
Console.WriteLine($"耗材平均单价：{avgConsumable:N2}");

// Any 和 Count 的区别：用一个带大括号的 lambda，把"检查了谁"打印出来
Console.WriteLine("Any：有缺货的吗？");
bool anyEmpty = products.Any(p =>
{
    Console.WriteLine($"  检查 {p.Code}");
    return p.Stock == 0;
});
Console.WriteLine($"  结果：{anyEmpty}");

Console.WriteLine("Count(...) > 0：有缺货的吗？");
bool countEmpty = products.Count(p =>
{
    Console.WriteLine($"  检查 {p.Code}");
    return p.Stock == 0;
}) > 0;
Console.WriteLine($"  结果：{countEmpty}");
