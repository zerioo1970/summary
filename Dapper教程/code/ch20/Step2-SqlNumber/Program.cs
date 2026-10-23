// code/ch20/Step2-SqlNumber/Program.cs
// SqlException.Number：把最常见的 9 个错误号各触发一次；再用 catch + when 只处理"单号重复"这一种
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 连接类：分别把用户名 / 库名改错
var badLogin = new SqlConnectionStringBuilder(connStr)
    { IntegratedSecurity = false, UserID = "nobody", Password = "wrong" }.ConnectionString;
var badDb = new SqlConnectionStringBuilder(connStr) { InitialCatalog = "PriceDemoX" }.ConnectionString;

Console.WriteLine("== 连接类");
Try("登录失败", () => new SqlConnection(badLogin).Open());
Try("库不存在", () => new SqlConnection(badDb).Open());
Try("命令超时", () => conn.Execute("WAITFOR DELAY '00:00:03'", commandTimeout: 1));   // 第 14 章讲过

Console.WriteLine("== SQL 类");
Try("列名无效", () => conn.Query<int>("SELECT Idd FROM dbo.Customer"));
Try("对象名无效", () => conn.Query<int>("SELECT Id FROM dbo.Custmer"));
Try("参数没传", () => conn.Query<int>("SELECT Id FROM dbo.Customer WHERE Id = @Id"));   // 忘了传 new { Id = 1 }
Try("外键冲突", () => conn.Execute("DELETE dbo.Customer WHERE Id = 1"));               // 1 号客户有申请单
Try("唯一键冲突", () => conn.Execute(
    "INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant) VALUES (@No, 1, N'王芳')",
    new { No = "PR20261001" }));                                                      // 单号已存在
Try("字符串截断", () => conn.Execute(
    "INSERT dbo.Customer (Name, City, Level) VALUES (N'测试医院', @City, 'A')",
    new { City = "这是一个超过二十个字的城市名称用来演示截断报错" }));                   // City 是 NVARCHAR(20)

// 业务代码里的用法：只把"单号重复"翻译成用户看得懂的话，其他错误照样往上抛
Console.WriteLine("== catch + when");
Console.WriteLine(CreateRequest(conn, "PR20261001"));
Console.WriteLine(CreateRequest(conn, "PR20261099"));

static string CreateRequest(SqlConnection conn, string requestNo)
{
    try
    {
        int id = conn.QuerySingle<int>(@"
            INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant)
            OUTPUT INSERTED.Id
            VALUES (@RequestNo, 1, N'王芳')", new { RequestNo = requestNo });
        return $"新建成功：{requestNo}，Id = {id}";
    }
    catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)   // 2627 唯一约束 / 2601 唯一索引
    {
        return $"单号 {requestNo} 已经存在，请换一个";
    }
}

// 打印 SqlException 的四个关键属性：Number（错误号）、Class（严重级别）、LineNumber（SQL 第几行）、Message
static void Try(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"{title}：没有报错");
    }
    catch (SqlException ex)
    {
        Console.WriteLine($"{title,-6} Number={ex.Number,-6} Class={ex.Class,-3} Line={ex.LineNumber}");
        Console.WriteLine($"    {ex.Message}");
    }
}
