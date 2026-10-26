// code/ch01/Step1-Lambda/Program.cs
using EfCoreStudy;

var products = SampleData.Products();

// 写法 A：把写好的方法交给 Where。只写方法名、不加括号——方法由 Where 去调用
var listA = products.Where(IsExpensive).ToList();

// 写法 B：同一条规则写成 lambda，直接写在括号里
var listB = products.Where(p => p.Price > 10000m).ToList();

Console.WriteLine($"写法 A：{listA.Count} 个");
Console.WriteLine($"写法 B：{listB.Count} 个");
foreach (var p in listB)
    Console.WriteLine($"  {p.Code}  {p.Name}  {p.Price:N0}");

// 价格下限往往是运行时才知道的（这里用字符串模拟用户在页面上的输入）
// lambda 可以直接用外面的变量 minPrice；写法 A 的方法只收一个 Product 参数，做不到
var minPrice = decimal.Parse("30000");
var listC = products.Where(p => p.Price > minPrice).ToList();
Console.WriteLine($"高于 {minPrice:N0} 的：{listC.Count} 个");

// 写法 A 用到的具名方法：给它一个产品，它回答 true 或 false
static bool IsExpensive(Product p)
{
    return p.Price > 10000m;
}
