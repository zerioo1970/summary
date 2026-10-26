// code/ch01/Step3-Select/Program.cs
using EfCoreStudy;

var products = SampleData.Products();

// ① 只要一列：结果是 List<string>
var names = products.Select(p => p.Name).ToList();
Console.WriteLine("全部名称：" + string.Join("、", names));

// ② 要几列：用匿名类型 new { ... }，不用为了一次查询专门写一个类
var cheap = products
    .Where(p => p.Price < 100m)                        // 先筛行
    .Select(p => new { p.Code, p.Name, p.Price })      // 再挑列
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
