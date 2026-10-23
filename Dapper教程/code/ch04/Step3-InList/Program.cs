// code/ch04/Step3-InList/Program.cs
// IN @Ids：传一个集合，Dapper 自动把它展开成每个元素一个参数
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 例 1：整数列表。注意 IN 后面直接写 @Ids，不加括号
int[] ids = { 1, 3 };
var r1 = conn.Query<RequestBrief>(
    "SELECT Id, RequestNo, Applicant FROM dbo.PriceRequest WHERE Id IN @Ids ORDER BY Id",
    new { Ids = ids });
Console.WriteLine("== 例 1 Id IN (1, 3)");
foreach (var r in r1) Console.WriteLine($"{r.Id} {r.RequestNo} {r.Applicant}");

// 例 2：字符串列表，List<string> 也可以（任何 IEnumerable 都行，string 本身除外）
var nos = new List<string> { "PR20261008", "PR20261015", "PR29999999" };
var r2 = conn.Query<RequestBrief>(
    "SELECT Id, RequestNo, Applicant FROM dbo.PriceRequest WHERE RequestNo IN @Nos ORDER BY Id",
    new { Nos = nos });
Console.WriteLine("== 例 2 RequestNo IN (3 个单号，其中 1 个不存在)");
foreach (var r in r2) Console.WriteLine($"{r.Id} {r.RequestNo} {r.Applicant}");

// 例 3：空列表——不报错，查到 0 行
var r3 = conn.Query<RequestBrief>(
    "SELECT Id, RequestNo, Applicant FROM dbo.PriceRequest WHERE Id IN @Ids",
    new { Ids = Array.Empty<int>() });
Console.WriteLine($"== 例 3 IN 空列表：{r3.Count()} 行");

// 例 4：NOT IN 空列表——一行都没排除，返回全部
var r4 = conn.Query<RequestBrief>(
    "SELECT Id, RequestNo, Applicant FROM dbo.PriceRequest WHERE Id NOT IN @Ids",
    new { Ids = Array.Empty<int>() });
Console.WriteLine($"== 例 4 NOT IN 空列表：{r4.Count()} 行");

// 例 5：集合和普通参数混用
var r5 = conn.Query<RequestBrief>(@"
    SELECT Id, RequestNo, Applicant FROM dbo.PriceRequest
    WHERE Applicant = @Applicant AND Status IN @Statuses ORDER BY Id",
    new { Applicant = "张伟", Statuses = new[] { 0, 1 } });
Console.WriteLine("== 例 5 张伟的草稿和待审单");
foreach (var r in r5) Console.WriteLine($"{r.Id} {r.RequestNo} {r.Applicant}");

public class RequestBrief
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
}
