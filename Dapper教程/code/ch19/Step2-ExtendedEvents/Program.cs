// code/ch19/Step2-ExtendedEvents/Program.cs
// 发 4 种典型命令给服务器，再用扩展事件看服务器实际收到的语句
// 运行前先执行 code/sql/ch19-xe-start.sql；运行后执行 code/sql/ch19-xe-read.sql 查看抓到的内容
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

// 给连接起个"应用程序名"：扩展事件会话只抓这个名字发来的语句，SSMS 等其他程序的语句不会混进来
var builder = new SqlConnectionStringBuilder(connStr) { ApplicationName = "Ch19Demo" };
using var conn = new SqlConnection(builder.ConnectionString);

// ① 没有参数
int n1 = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.Customer");
Console.WriteLine($"① 无参数：客户 {n1} 个");

// ② 匿名对象：一个 string、一个 int
var r2 = conn.Query<string>(
    "SELECT RequestNo FROM dbo.PriceRequest WHERE Applicant = @Applicant AND Status >= @MinStatus",
    new { Applicant = "张伟", MinStatus = 1 }).ToList();
Console.WriteLine($"② 带参数：{string.Join("、", r2)}");

// ③ IN 列表：看服务器收到的参数个数
var r3 = conn.Query<string>(
    "SELECT RequestNo FROM dbo.PriceRequest WHERE Id IN @Ids ORDER BY Id",
    new { Ids = new[] { 1, 3 } }).ToList();
Console.WriteLine($"③ IN 列表：{string.Join("、", r3)}");

// ④ 传集合给 Execute：看服务器收到几条命令（备注改成它自己，不改变数据）
int n4 = conn.Execute(
    "UPDATE dbo.PriceRequest SET Remark = Remark WHERE Id = @Id",
    new[] { new { Id = 1 }, new { Id = 2 } });
Console.WriteLine($"④ 传集合：受影响 {n4} 行");
