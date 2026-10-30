// code/ch04/Exercise/Program.cs
// 第 4 章练习参考答案：练习 1（修复延迟执行的 bug）+ 练习 2（改写成查询语法）
using EfCoreStudy;

var products = SampleData.Products();

// 练习 1：定义时就 ToList()，当场执行，结果固定下来，后面改 minPrice 不再影响它
var minPrice = 10000m;
var expensive = products.Where(p => p.Price > minPrice).ToList();
minPrice = 100000m;   // 后面别的代码改了这个变量
Console.WriteLine($"贵重产品：{expensive.Count} 个");

// 练习 2：第 2 章练习 1 的耗材价格表，改写成查询语法
var priceList = (from p in products
                 where p.Category == "耗材"
                 orderby p.Price
                 select new { p.Name, p.Price }).ToList();
Console.WriteLine("耗材价格表：");
foreach (var x in priceList)
    Console.WriteLine($"  {x.Name}  {x.Price:N2}");
