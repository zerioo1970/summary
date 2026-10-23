// code/ch03/Step3-Buffered/Program.cs
// 缓冲 vs 流式：看连接什么时候开、什么时候关，以及流式读取时为什么不能再用同一个连接
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);   // 注意：全程不手动 Open，让 Dapper 自己开关

const string sql = "SELECT Id, Code, Name, ListPrice FROM dbo.Product ORDER BY Id";

// 例 1：缓冲（默认 buffered: true）——Query 返回时，数据已全部在内存里，连接已关
Console.WriteLine("== 例 1 缓冲");
var list = conn.Query<Product>(sql);
Console.WriteLine($"Query 返回后：连接 {conn.State}，类型 {list.GetType().Name}");
foreach (var p in list)
    Console.WriteLine($"  读到 {p.Code,-7} 连接 {conn.State}");

// 例 2：流式（buffered: false）——Query 返回时还没执行，foreach 时才一边读一边给你
Console.WriteLine("== 例 2 流式");
var stream = conn.Query<Product>(sql, buffered: false);
Console.WriteLine($"Query 返回后：连接 {conn.State}，类型 {stream.GetType().Name}");
foreach (var p in stream)
    Console.WriteLine($"  读到 {p.Code,-7} 连接 {conn.State}");   // 遍历期间连接一直开着
Console.WriteLine($"遍历结束后：连接 {conn.State}");

// 例 3：流式遍历中，再用同一个连接查别的 —— 报错
Console.WriteLine("== 例 3 流式遍历中再查一次");
try
{
    foreach (var p in conn.Query<Product>(sql, buffered: false))
    {
        // 想顺便查这个产品被申请过几次：上一个 DataReader 还没读完，同一连接不能再开一个
        // （@Id 是参数写法，new { p.Id } 就是 new { Id = p.Id }，第 4 章细讲）
        int times = conn.Query<int>(
            "SELECT COUNT(*) FROM dbo.PriceRequestItem WHERE ProductId = @Id", new { p.Id }).First();
        Console.WriteLine($"  {p.Code} 被申请 {times} 次");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"  {ex.GetType().FullName}: {ex.Message}");
}
Console.WriteLine($"出错后：连接 {conn.State}");

// 例 4：同样的写法换回缓冲 —— 外层查询已读完、DataReader 已关，里面再查没问题
Console.WriteLine("== 例 4 缓冲遍历中再查一次");
foreach (var p in conn.Query<Product>(sql))
{
    int times = conn.Query<int>(
        "SELECT COUNT(*) FROM dbo.PriceRequestItem WHERE ProductId = @Id", new { p.Id }).First();
    Console.WriteLine($"  {p.Code} 被申请 {times} 次");
}

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal ListPrice { get; set; }
}
