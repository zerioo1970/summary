// code/ch00/Step2-List/Program.cs
using EfCoreStudy;

// ① 建一个空 List，逐个 Add。<Product> 表示"这个 List 只能装 Product"
var list = new List<Product>();
list.Add(new Product { Id = 1, Code = "US-100", Name = "便携超声主机", Price = 180000m });
list.Add(new Product { Id = 6, Code = "GEL-250", Name = "耦合剂", Price = 35m });
Console.WriteLine($"① 个数：{list.Count}，第 1 个：{list[0].Name}，第 2 个：{list[1].Name}");

// ② 集合表达式 [ ... ]（C# 12 起）：建 List 时直接写好内容。SampleData.cs 用的就是这种写法
List<string> categories = ["主机", "探头", "耗材"];
Console.WriteLine($"② 分类：{string.Join("、", categories)}");

// ③ 示例数据：后面每一章都用这 8 个产品
var products = SampleData.Products();
Console.WriteLine($"③ 示例数据：{products.Count} 个产品，最后一个是 {products[products.Count - 1].Name}");

// ④ 取出来的元素就是 Product，直接点属性，不用强制转换
Product first = products[0];
Console.WriteLine($"④ 第 1 个产品的单价：{first.Price:N0}");
