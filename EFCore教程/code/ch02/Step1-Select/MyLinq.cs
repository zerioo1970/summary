// code/ch02/Step1-Select/MyLinq.cs
namespace EfCoreStudy;

// 自己写一个 Select，和第 1 章的 MyWhere 对照着看
public static class MyLinq
{
    // MyWhere（第 1 章）：规则回答 true/false，决定这个产品"留不留"
    public static List<Product> MyWhere(this List<Product> source, Func<Product, bool> rule)
    {
        var result = new List<Product>();
        foreach (var p in source)
        {
            if (rule(p))
                result.Add(p);
        }
        return result;
    }

    // MySelect：规则回答"要它的什么"，把规则的返回值收集起来。
    // 为了好读只写返回 string 的版本；.NET 自带的 Select 能返回任何类型
    public static List<string> MySelect(this List<Product> source, Func<Product, string> pick)
    {
        var result = new List<string>();
        foreach (var p in source)
            result.Add(pick(p));    // 不做判断，每个都要；加进去的是 pick(p) 的结果，不是 p 本身
        return result;
    }
}
