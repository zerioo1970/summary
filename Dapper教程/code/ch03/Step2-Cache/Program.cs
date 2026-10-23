// code/ch03/Step2-Cache/Program.cs
// 看映射缓存：第一次慢、之后快；SQL 文本一变就是一条新缓存
using System.Diagnostics;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);
conn.Open();   // 先手动打开：把"建立连接"的时间排除在外，下面只量查询本身

// 热身：先用 ADO.NET 跑一条，让 SqlClient 自己的首次加载也不计入
using (var cmd = new SqlCommand("SELECT 1", conn)) cmd.ExecuteScalar();

const string sql = "SELECT Id, Code, Name, ListPrice FROM dbo.Product ORDER BY Id";

// 实验 1：同一条 SQL + 同一个类型，连跑 5 次
Console.WriteLine("== 实验 1：Query<Product>，同一条 SQL 跑 5 次");
for (int i = 1; i <= 5; i++)
{
    var sw = Stopwatch.StartNew();
    conn.Query<Product>(sql);
    sw.Stop();
    Console.WriteLine($"第 {i} 次：{sw.Elapsed.TotalMilliseconds,8:F3} ms");
}
Console.WriteLine($"缓存条数：{SqlMapper.GetCachedSQLCount()}");

// 实验 2：SQL 一字不改，只换接收的类型 —— 又要重新生成一次映射代码
Console.WriteLine("== 实验 2：同一条 SQL，换成 Query<ProductBrief>，跑 3 次");
for (int i = 1; i <= 3; i++)
{
    var sw = Stopwatch.StartNew();
    conn.Query<ProductBrief>(sql);
    sw.Stop();
    Console.WriteLine($"第 {i} 次：{sw.Elapsed.TotalMilliseconds,8:F3} ms");
}
Console.WriteLine($"缓存条数：{SqlMapper.GetCachedSQLCount()}");

// 实验 3：把 Id 拼进 SQL —— 每个 Id 都是一段不同的 SQL 文本，每段一条缓存
var sw3 = Stopwatch.StartNew();
for (int id = 1; id <= 100; id++)
    conn.Query<Product>("SELECT Id, Code, Name, ListPrice FROM dbo.Product WHERE Id = " + id);
sw3.Stop();
Console.WriteLine($"== 实验 3：拼接 SQL 循环 100 次，共 {sw3.Elapsed.TotalMilliseconds:F1} ms，缓存条数：{SqlMapper.GetCachedSQLCount()}");

// 实验 4：参数写法（第 4 章细讲）—— SQL 文本始终不变，只多 1 条缓存
var sw4 = Stopwatch.StartNew();
for (int id = 1; id <= 100; id++)
    conn.Query<Product>("SELECT Id, Code, Name, ListPrice FROM dbo.Product WHERE Id = @Id", new { Id = id });
sw4.Stop();
Console.WriteLine($"== 实验 4：参数写法循环 100 次，共 {sw4.Elapsed.TotalMilliseconds:F1} ms，缓存条数：{SqlMapper.GetCachedSQLCount()}");

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal ListPrice { get; set; }
}

public class ProductBrief      // 只要两个属性，用来证明"换类型 = 换缓存"
{
    public string Code { get; set; } = "";
    public decimal ListPrice { get; set; }
}
