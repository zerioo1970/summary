// code/ch14/Step1-Async/Program.cs
// Dapper 的异步方法：名字加 Async、调用前加 await；SQL、参数、映射规则全都不变
using System.Diagnostics;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

// 顶级语句里可以直接写 await：编译器自动生成 async Task Main
await using var conn = new SqlConnection(connStr);   // await using：异步释放连接

// 例 1：Query → QueryAsync。返回的是 Task<IEnumerable<T>>，await 之后才是 IEnumerable<T>
var products = (await conn.QueryAsync<Product>(
    "SELECT Id, Code, Name, ListPrice FROM dbo.Product ORDER BY Id")).ToList();
Console.WriteLine($"例 1 QueryAsync：{products.Count} 个产品，第一个是 {products[0].Name}");

// 例 2：单行、单值、增删改也都有 Async 版本，参数写法和同步版一模一样
var req = await conn.QuerySingleAsync<RequestRow>(
    "SELECT Id, RequestNo, Applicant FROM dbo.PriceRequest WHERE Id = @Id", new { Id = 2 });
int pending = await conn.ExecuteScalarAsync<int>(
    "SELECT COUNT(*) FROM dbo.PriceRequest WHERE Status = 1");
int affected = await conn.ExecuteAsync(
    "UPDATE dbo.PriceRequest SET Remark = @Remark WHERE Id = @Id", new { Remark = "异步改的备注", Id = 3 });
Console.WriteLine($"例 2 QuerySingleAsync：{req.RequestNo} {req.Applicant}；待审 {pending} 张；改了 {affected} 行");

// 例 3：async 不会让 SQL 变快——两条各等 1 秒的语句，依次 await，总共还是 2 秒
var sw = Stopwatch.StartNew();
await conn.ExecuteAsync("WAITFOR DELAY '00:00:01'");
await conn.ExecuteAsync("WAITFOR DELAY '00:00:01'");
Console.WriteLine($"例 3 一个连接、依次 await：{sw.Elapsed.TotalSeconds:F1} 秒");

// 例 4：想让两条同时跑，要两个连接——一个连接同一时刻只能执行一条命令
await using var connA = new SqlConnection(connStr);
await using var connB = new SqlConnection(connStr);
sw.Restart();
Task<int> taskA = connA.ExecuteAsync("WAITFOR DELAY '00:00:01'");   // 不 await：先把两个都发出去
Task<int> taskB = connB.ExecuteAsync("WAITFOR DELAY '00:00:01'");
await Task.WhenAll(taskA, taskB);                                   // 再一起等
Console.WriteLine($"例 4 两个连接、同时等待：{sw.Elapsed.TotalSeconds:F1} 秒");

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal ListPrice { get; set; }
}

public class RequestRow
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
}
