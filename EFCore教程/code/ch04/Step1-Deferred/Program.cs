// code/ch04/Step1-Deferred/Program.cs
using EfCoreStudy;

var products = SampleData.Products();

// ① 定义查询。lambda 里加了一行打印，用来看它"什么时候"真正被调用
Console.WriteLine("1. 定义查询");
var query = products.Where(p =>
{
    Console.Write($"[检查 {p.Code}] ");
    return p.Stock == 0;
});
Console.WriteLine("2. 查询定义完了，上面没有任何“检查”");

// ② 定义之后，往数据源里再加一个缺货产品
products.Add(new Product { Id = 9, Code = "PB-T04", Name = "经食道探头", Category = "探头", Price = 58000m, Stock = 0 });
Console.WriteLine("3. 往 products 里加了一个缺货产品 PB-T04");

// ③ 到 ToList() 这一刻，才真正开始逐个检查
Console.WriteLine("4. 调用 ToList()：");
var result = query.ToList();
Console.WriteLine();
Console.WriteLine("5. 缺货产品：" + string.Join("、", result.Select(p => p.Code)));
