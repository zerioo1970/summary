// code/ch04/Step1-Injection/Program.cs
// 拼接 SQL 被注入 vs 参数化：同样的输入，结果完全不同
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 假设这是查询页"申请单号"输入框里用户填的内容
string input = "' OR '1'='1";

// 错误写法：把输入直接拼进 SQL
string badSql = "SELECT RequestNo, Applicant FROM dbo.PriceRequest WHERE RequestNo = '" + input + "'";
Console.WriteLine("== 拼接写法");
Console.WriteLine("实际执行的 SQL：" + badSql);
var bad = conn.Query<RequestBrief>(badSql).ToList();
Console.WriteLine($"查到 {bad.Count} 行：" + string.Join("，", bad.Select(r => r.RequestNo)));

// 正确写法：SQL 里只写 @No 占位，值放在匿名对象里单独传
const string goodSql = "SELECT RequestNo, Applicant FROM dbo.PriceRequest WHERE RequestNo = @No";
Console.WriteLine("== 参数化写法");
Console.WriteLine("实际执行的 SQL：" + goodSql);
var good = conn.Query<RequestBrief>(goodSql, new { No = input }).ToList();
Console.WriteLine($"查到 {good.Count} 行");   // 整串输入被当成"单号"去比较，没有这个单号

// 参数化写法，正常输入照样能查到
var normal = conn.Query<RequestBrief>(goodSql, new { No = "PR20261008" }).ToList();
Console.WriteLine($"输入 PR20261008：查到 {normal.Count} 行，申请人 {normal[0].Applicant}");

public class RequestBrief
{
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
}
