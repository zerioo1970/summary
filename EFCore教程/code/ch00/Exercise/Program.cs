// code/ch00/Exercise/Program.cs
// 第 0 章练习参考答案：练习 1（追加一个产品）+ 练习 2（用 foreach 统计探头）
using EfCoreStudy;

var products = SampleData.Products();

// 练习 1：用对象初始化器建好新产品，再 Add 进 List
products.Add(new Product { Id = 9, Code = "PB-T04", Name = "经食道探头", Category = "探头", Price = 58000m, Stock = 0 });
Console.WriteLine($"产品数：{products.Count}，最后一个：{products[products.Count - 1].Name}");

// 练习 2：计数器和"目前最高价"都放在循环外面，循环里只更新它们
int probeCount = 0;
decimal maxProbePrice = 0;
foreach (var p in products)
{
    if (p.Category != "探头")
        continue;               // 不是探头，直接看下一个
    probeCount++;
    if (p.Price > maxProbePrice)
        maxProbePrice = p.Price;
}
Console.WriteLine($"探头：{probeCount} 个，最高单价 {maxProbePrice:N0}");
