// code/ch02/Step2-OrderBy/Program.cs
using EfCoreStudy;

var products = SampleData.Products();

// ① 按单价从低到高：相当于 ORDER BY Price
var byPrice = products.OrderBy(p => p.Price).ToList();
Print("① 单价升序", byPrice);

// ② 从高到低：相当于 ORDER BY Price DESC
var byPriceDesc = products.OrderByDescending(p => p.Price).ToList();
Print("② 单价降序", byPriceDesc);

// ③ 先按分类，同一分类里再按单价从高到低：相当于 ORDER BY Category, Price DESC
//    第二个排序条件必须用 ThenBy / ThenByDescending，不能再写一个 OrderBy
var byCategory = products
    .OrderBy(p => p.Category)
    .ThenByDescending(p => p.Price)
    .ToList();
Print("③ 分类 + 单价降序", byCategory);

static void Print(string title, List<Product> list)
{
    Console.WriteLine(title + "：");
    foreach (var p in list)
        Console.WriteLine($"  {p.Category}  {p.Code,-8}{p.Price,10:N0}");
}
