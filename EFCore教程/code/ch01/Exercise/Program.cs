// code/ch01/Exercise/Program.cs
// 第 1 章练习参考答案：练习 1（需补货清单）+ 练习 2（按价格区间筛选）
using EfCoreStudy;

var products = SampleData.Products();

// 练习 1：有货但不足 10 件。两个条件写在同一个 lambda 里，用 && 连接
var toRestock = products.Where(p => p.Stock > 0 && p.Stock < 10).ToList();
Console.WriteLine("需补货：");
foreach (var p in toRestock)
    Console.WriteLine($"  {p.Code}  {p.Name}  库存 {p.Stock}");

// 练习 2：区间来自"用户输入"，lambda 直接用外面的两个变量
var minPrice = decimal.Parse("20000");
var maxPrice = decimal.Parse("30000");
var inRange = products.Where(p => p.Price >= minPrice && p.Price <= maxPrice).ToList();
Console.WriteLine($"{minPrice:N0} 到 {maxPrice:N0} 之间：");
foreach (var p in inRange)
    Console.WriteLine($"  {p.Code}  {p.Name}  {p.Price:N0}");
