// code/ch01/Step1-Lambda/Program.cs
using EfCoreStudy;

var products = SampleData.Products();

// 写法 B：同一个判断写成 lambda，存进变量。Func<Product, bool> = 输入 Product、返回 bool
Func<Product, bool> isExpensive = p => p.Price > 10000m;

foreach (var p in products)
{
    // 两种写法结果必须完全一样
    Console.WriteLine($"{p.Code,-8} 方法：{IsExpensive(p),-6} lambda：{isExpensive(p)}");
}

// 写法 A：具名方法，你一直以来的写法
static bool IsExpensive(Product p)
{
    return p.Price > 10000m;
}
