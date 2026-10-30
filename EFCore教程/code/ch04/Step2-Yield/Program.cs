// code/ch04/Step2-Yield/Program.cs
using EfCoreStudy;

// 只取前 4 个产品，输出短一些。库存依次是 3、2、8、0
var products = SampleData.Products().Take(4).ToList();

Console.WriteLine("=== MyWhereNow（List 版）");
var now = products.MyWhereNow(p =>
{
    Console.WriteLine($"  检查 {p.Code}");
    return p.Stock < 5;
});
Console.WriteLine("  -- 方法已返回，开始 foreach --");
foreach (var p in now)
    Console.WriteLine($"  拿到 {p.Code}");

Console.WriteLine("=== MyWhereLazy（yield 版）");
var lazy = products.MyWhereLazy(p =>
{
    Console.WriteLine($"  检查 {p.Code}");
    return p.Stock < 5;
});
Console.WriteLine("  -- 方法已返回，开始 foreach --");
foreach (var p in lazy)
    Console.WriteLine($"  拿到 {p.Code}");
