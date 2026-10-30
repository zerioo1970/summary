// code/ch01/Step1-WhatIsLinq/MyLinq.cs
namespace EfCoreStudy;

// 自己写一个 Where，看清 LINQ 的原理。
// .NET 自带的 Enumerable.Where 能用于任何类型的集合，这里为了好读只写 Product 版
public static class MyLinq
{
    // this：让它能写成 products.MyWhere(...)，看起来像 List 自己的方法（扩展方法）
    // rule：调用方交进来的"规则"。Func<Product, bool> 读作"收一个 Product、返回 bool 的方法"
    public static List<Product> MyWhere(this List<Product> source, Func<Product, bool> rule)
    {
        var result = new List<Product>();
        foreach (var p in source)
        {
            if (rule(p))          // 每个产品都拿去问一下规则
                result.Add(p);    // 回答 true 的留下
        }
        return result;
    }
}
