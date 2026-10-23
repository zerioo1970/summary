// code/ch05/Exercise/Program.cs
// 第 5 章练习参考答案：练习 1（按单号显示状态文字）+ 练习 2（统计张数与金额）
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 练习 1：单号唯一，用 QuerySingleOrDefault；状态 0 是合法值，所以用 byte? 区分"不存在"
foreach (var no in new[] { "PR20261001", "PR20261015", "PR404" })
{
    byte? status = conn.QuerySingleOrDefault<byte?>(
        "SELECT Status FROM dbo.PriceRequest WHERE RequestNo = @No", new { No = no });
    string text = status switch
    {
        null => "单号不存在",
        0 => "草稿",
        1 => "待审",
        2 => "通过",
        3 => "驳回",
        _ => "未知状态",
    };
    Console.WriteLine($"{no}：{text}");
}

// 练习 2：COUNT 用 int；SUM 可能是 NULL，用 decimal?
int count = conn.ExecuteScalar<int>(
    "SELECT COUNT(*) FROM dbo.PriceRequest WHERE Applicant = @Name", new { Name = "张伟" });
Console.WriteLine($"张伟的申请单：{count} 张");

foreach (var no in new[] { "PR20261008", "PR404" })
{
    decimal? amount = conn.ExecuteScalar<decimal?>(@"
        SELECT SUM(i.Qty * i.RequestPrice)
        FROM dbo.PriceRequestItem AS i
        JOIN dbo.PriceRequest AS r ON r.Id = i.RequestId
        WHERE r.RequestNo = @No", new { No = no });
    Console.WriteLine($"{no} 金额：{(amount is null ? "无明细" : amount.Value.ToString("N2"))}");
}
