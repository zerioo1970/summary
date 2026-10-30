// code/ch00/Step1-Class/Program.cs
using EfCoreStudy;

// ① 先 new，再一行一行赋值（你熟悉的写法）
var p1 = new Product();
p1.Id = 1;
p1.Code = "US-100";
p1.Name = "便携超声主机";
p1.Category = "主机";
p1.Price = 180000m;
p1.Stock = 3;

// ② 对象初始化器：new 和赋值写在一起，效果和 ① 完全相同
var p2 = new Product { Id = 3, Code = "PB-L01", Name = "线阵探头", Category = "探头", Price = 26000m, Stock = 8 };

// ③ 没赋值的属性取默认值：数字是 0，字符串是 Product 里写的 ""
var p3 = new Product { Code = "TEMP" };

Console.WriteLine($"① {p1.Code}  {p1.Name}  单价 {p1.Price:N0}  库存 {p1.Stock}");
Console.WriteLine($"② {p2.Code}  {p2.Name}  单价 {p2.Price:N0}  库存 {p2.Stock}");
Console.WriteLine($"③ {p3.Code}  名称=[{p3.Name}]  单价 {p3.Price}  库存 {p3.Stock}");

// ④ 变量里放的是"指向对象的引用"：p4 和 p2 指向同一个产品，改 p4 就是改 p2
var p4 = p2;
p4.Stock = 0;
Console.WriteLine($"④ 改了 p4 的库存之后，p2 的库存：{p2.Stock}");
