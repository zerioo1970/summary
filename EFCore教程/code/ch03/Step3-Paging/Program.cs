// code/ch03/Step3-Paging/Program.cs
using EfCoreStudy;

var products = SampleData.Products();
const int pageSize = 3;

// 共几页：8 个、每页 3 个 → 8 ÷ 3 = 2.67，向上取整是 3 页（最后一页只有 2 个）
int pageCount = (int)Math.Ceiling(products.Count / (double)pageSize);
Console.WriteLine($"共 {products.Count} 个，每页 {pageSize} 个，共 {pageCount} 页");

for (int page = 1; page <= pageCount; page++)
{
    var items = products
        .OrderBy(p => p.Id)                  // 分页前先排序，"第 2 页是哪几条"才是确定的
        .Skip((page - 1) * pageSize)         // 跳过前面几页：第 1 页跳 0 个，第 2 页跳 3 个，第 3 页跳 6 个
        .Take(pageSize)                      // 再取一页的量；不够 3 个就有几个取几个
        .ToList();
    Console.Write($"第 {page} 页：");
    foreach (var p in items)
        Console.Write($"{p.Id}.{p.Code}  ");
    Console.WriteLine();
}
