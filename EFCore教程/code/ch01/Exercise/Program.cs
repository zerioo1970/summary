// code/ch01/Exercise/Program.cs
// 第 1 章练习参考答案：练习 1（需补货清单）+ 练习 2（耗材含税价）
using EfCoreStudy;

var products = SampleData.Products();

// 练习 1：有货但不足 10 件。两个条件写在同一个 lambda 里，用 && 连接
var toRestock = products
    .Where(p => p.Stock > 0 && p.Stock < 10)
    .Select(p => new { p.Code, p.Name, p.Stock })
    .ToList();
Console.WriteLine("需补货：");
foreach (var x in toRestock)
    Console.WriteLine($"  {x.Code}  {x.Name}  库存 {x.Stock}");

// 练习 2：计算列。1.13m 必须带 m，否则是 double，和 decimal 相乘会编译报错
var withTax = products
    .Where(p => p.Category == "耗材")
    .Select(p => new { p.Name, TaxPrice = p.Price * 1.13m })
    .ToList();
Console.WriteLine("耗材含税价：");
foreach (var x in withTax)
    Console.WriteLine($"  {x.Name}  {x.TaxPrice:N2}");
