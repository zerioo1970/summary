// code/ch02/Step3-First/Program.cs
using EfCoreStudy;

var products = SampleData.Products();

// ① First：取第一个满足条件的。相当于 SELECT TOP 1 ... WHERE
var firstProbe = products.First(p => p.Category == "探头");
Console.WriteLine($"① First 第一个探头：{firstProbe.Code}");

// ② FirstOrDefault：找不到时返回 null，不报错
var notFound = products.FirstOrDefault(p => p.Code == "XX-999");
Console.WriteLine($"② FirstOrDefault 找 XX-999：{(notFound is null ? "null" : notFound.Name)}");

// ③ Single：断言"恰好只有一个"。按编码查，编码本来就不该重复
var byCode = products.Single(p => p.Code == "PB-L01");
Console.WriteLine($"③ Single 按编码 PB-L01：{byCode.Name}");

// ④ 编码来自用户输入，可能不存在：用 SingleOrDefault，再判断 null
var input = "GEL-250";
var found = products.SingleOrDefault(p => p.Code == input);
if (found is null)
    Console.WriteLine($"④ 编码 {input} 不存在");
else
    Console.WriteLine($"④ SingleOrDefault 按编码 {input}：{found.Name}，库存 {found.Stock}");
