// code/ch01/Step2-Where/Program.cs
using EfCoreStudy;

var products = SampleData.Products();

// 老写法：foreach + if + 新建 List
var oldWay = new List<Product>();
foreach (var p in products)
{
    if (p.Category == "探头" && p.Stock > 0)
        oldWay.Add(p);
}
Console.WriteLine($"foreach 写法：{oldWay.Count} 个");

// LINQ 写法：条件就是一个 lambda，Where 替你写好了循环和 if
var linqWay = products.Where(p => p.Category == "探头" && p.Stock > 0).ToList();
Console.WriteLine($"Where 写法：{linqWay.Count} 个");
foreach (var p in linqWay)
    Console.WriteLine($"  {p.Code}  {p.Name}  库存 {p.Stock}");

// Where 返回的是新结果，不会删改原来的 List
Console.WriteLine($"原列表仍是 {products.Count} 个");
