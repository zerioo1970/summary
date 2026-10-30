// code/ch04/Step3-QuerySyntax/Program.cs
using EfCoreStudy;

var products = SampleData.Products();

// ① 方法语法：前 3 章一直用的写法
var m = products
    .Where(p => p.Category == "探头" && p.Stock > 0)
    .OrderByDescending(p => p.Price)
    .Select(p => new { p.Code, p.Price })
    .ToList();

// ② 查询语法：同一个查询。像 SQL，但 from 在最前、select 在最后
var q = (from p in products
         where p.Category == "探头" && p.Stock > 0
         orderby p.Price descending
         select new { p.Code, p.Price }).ToList();

Console.WriteLine("方法语法：" + string.Join("  ", m.Select(x => $"{x.Code} {x.Price:N0}")));
Console.WriteLine("查询语法：" + string.Join("  ", q.Select(x => $"{x.Code} {x.Price:N0}")));

// ③ 分组：group ... by ... into g，之后用 g 写 select
var report = (from p in products
              group p by p.Category into g
              select new { Category = g.Key, Count = g.Count() }).ToList();
Console.WriteLine("分组：" + string.Join("  ", report.Select(x => $"{x.Category} {x.Count} 个")));

// ④ 查询语法里没有 Count、First、ToList 这些关键字：整句加括号，再接方法
int outOfStock = (from p in products where p.Stock == 0 select p).Count();
Console.WriteLine($"缺货：{outOfStock} 个");
