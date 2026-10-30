// code/ch00/Step3-Foreach/Program.cs
using EfCoreStudy;

var products = SampleData.Products();

// ① foreach：依次取出每个产品，p 每一轮代表"当前这个"
foreach (var p in products)
{
    // {p.Code,-8}：左对齐占 8 格；{p.Price,10:N0}：右对齐占 10 格、千分位、无小数
    Console.WriteLine($"{p.Code,-8}{p.Price,10:N0}  {p.Name}");
}

// ② 边循环边算：库存总金额，同时把缺货的产品挑出来
decimal total = 0;
var outOfStock = new List<Product>();
foreach (var p in products)
{
    total += p.Price * p.Stock;
    if (p.Stock == 0)
        outOfStock.Add(p);
}
Console.WriteLine($"库存总金额：{total:N0}");
Console.WriteLine($"缺货：{outOfStock.Count} 个，是 {outOfStock[0].Name}");
