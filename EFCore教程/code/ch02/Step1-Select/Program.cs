// code/ch02/Step1-Select/Program.cs
using EfCoreStudy;

var products = SampleData.Products();

// ① 原理：自己写的 MySelect 和 .NET 自带的 Select，结果相同
var names1 = products.MySelect(p => p.Name);
var names2 = products.Select(p => p.Name).ToList();
Console.WriteLine("MySelect：" + string.Join("、", names1));
Console.WriteLine("Select：" + string.Join("、", names2));

// ② 要几列：用匿名类型 new { ... }，不用为一次查询专门写一个类
var cheap = products
    .Where(p => p.Price < 100m)                     // 先筛行
    .Select(p => new { p.Code, p.Name, p.Price })   // 再挑列
    .ToList();
foreach (var x in cheap)
    Console.WriteLine($"{x.Code}  {x.Name}  {x.Price}");

// ③ 计算列：相当于 SQL 的 Price * Stock AS Amount
var amounts = products
    .Where(p => p.Category == "主机")
    .Select(p => new { p.Name, Amount = p.Price * p.Stock })
    .ToList();
foreach (var x in amounts)
    Console.WriteLine($"{x.Name}  库存金额 {x.Amount:N0}");
