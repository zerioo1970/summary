// code/ch05/Step1-FirstSingle/Program.cs
// 0 行 / 1 行 / 多行 × 4 个方法：逐一实测返回什么、抛什么异常
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

const string sql = "SELECT Id, RequestNo, Applicant FROM dbo.PriceRequest WHERE Applicant = @Name ORDER BY Id";

// 三种情况：王五 0 张单，李娜 1 张，张伟 2 张
foreach (var name in new[] { "王五", "李娜", "张伟" })
{
    Console.WriteLine($"== 申请人 {name}");
    Show("QueryFirst          ", () => conn.QueryFirst<RequestBrief>(sql, new { Name = name }));
    Show("QueryFirstOrDefault ", () => conn.QueryFirstOrDefault<RequestBrief>(sql, new { Name = name }));
    Show("QuerySingle         ", () => conn.QuerySingle<RequestBrief>(sql, new { Name = name }));
    Show("QuerySingleOrDefault", () => conn.QuerySingleOrDefault<RequestBrief>(sql, new { Name = name }));
}

// 调用一个方法：成功就打印单号（null 打印 "null"），失败就打印异常类型和消息
static void Show(string method, Func<RequestBrief?> call)
{
    try
    {
        var r = call();
        Console.WriteLine($"{method} → {(r is null ? "null" : r.RequestNo)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{method} → {ex.GetType().Name}: {ex.Message}");
    }
}

public class RequestBrief
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
}
