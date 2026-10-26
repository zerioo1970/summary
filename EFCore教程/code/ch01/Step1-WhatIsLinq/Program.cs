// code/ch01/Step1-WhatIsLinq/Program.cs
using EfCoreStudy;

var products = SampleData.Products();

// ① 平常看到的 LINQ 写法：把规则 IsLowStock 交给 Where
var a = products.Where(IsLowStock).ToList();

// ② 同一句的"真面目"：Where 其实是 Enumerable 类里的静态方法，集合是第一个参数
var b = Enumerable.Where(products, IsLowStock).ToList();

// ③ 自己写的 MyWhere（见 MyLinq.cs）：里面就是 foreach + if
var c = products.MyWhere(IsLowStock);

Print("① products.Where", a);
Print("② Enumerable.Where", b);
Print("③ 自己写的 MyWhere", c);

// 规则：给它一个产品，它回答"库存是否不足 5 件"
static bool IsLowStock(Product p)
{
    return p.Stock < 5;
}

static void Print(string title, List<Product> list)
{
    Console.Write(title + "：");
    foreach (var p in list)
        Console.Write(p.Code + "  ");
    Console.WriteLine();
}
