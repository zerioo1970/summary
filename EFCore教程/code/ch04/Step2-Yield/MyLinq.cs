// code/ch04/Step2-Yield/MyLinq.cs
namespace EfCoreStudy;

public static class MyLinq
{
    // 第 1 章的写法：立刻把循环全部跑完，结果装进 List 再返回
    public static List<Product> MyWhereNow(this List<Product> source, Func<Product, bool> rule)
    {
        var result = new List<Product>();
        foreach (var p in source)
        {
            if (rule(p))
                result.Add(p);
        }
        return result;
    }

    // .NET 的 Where 真正的写法：yield return。
    // 返回类型是 IEnumerable<Product>（"可以 foreach 的东西"），而不是装好结果的 List
    public static IEnumerable<Product> MyWhereLazy(this IEnumerable<Product> source, Func<Product, bool> rule)
    {
        foreach (var p in source)
        {
            if (rule(p))
                yield return p;   // 交出一个结果后"停在这里"；调用方要下一个时，再从这里接着往下跑
        }
    }
}
