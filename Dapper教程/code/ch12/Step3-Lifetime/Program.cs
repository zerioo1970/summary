// code/ch12/Step3-Lifetime/Program.cs
// GridReader 的生命周期：它活着的时候占着连接；读完最后一个结果集或 Dispose 时才放手
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);   // 注意：没有手动 Open，交给 Dapper 开关

const string sql = @"
SELECT COUNT(*) FROM dbo.Customer;
SELECT COUNT(*) FROM dbo.Product;";

// 实验 1：老老实实读完两个结果集
Console.WriteLine($"实验 1 调用前：{conn.State}");
using (var grid = conn.QueryMultiple(sql))
{
    Console.WriteLine($"  拿到 grid：{conn.State}，IsConsumed={grid.IsConsumed}");
    int customers = grid.ReadSingle<int>();
    Console.WriteLine($"  读完第 1 个（客户 {customers}）：{conn.State}，IsConsumed={grid.IsConsumed}");
    int products = grid.ReadSingle<int>();
    Console.WriteLine($"  读完第 2 个（产品 {products}）：{conn.State}，IsConsumed={grid.IsConsumed}");
}

// 实验 2：只读第 1 个就离开 using——剩下的结果集被丢弃，连接在 Dispose 时关闭
using (var grid = conn.QueryMultiple(sql))
{
    grid.ReadSingle<int>();
    Console.WriteLine($"实验 2 只读 1 个，还在 using 里：{conn.State}");
}
Console.WriteLine($"  离开 using 之后：{conn.State}");

// 实验 3：grid 没用完时想再查别的？先把需要的数据读进 List，离开 using 再查
List<int> counts;
using (var grid = conn.QueryMultiple(sql))
{
    counts = new List<int> { grid.ReadSingle<int>(), grid.ReadSingle<int>() };
}
int requests = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.PriceRequest");   // grid 已释放，连接空出来了
Console.WriteLine($"实验 3 客户 {counts[0]}、产品 {counts[1]}、申请单 {requests}，最后：{conn.State}");
