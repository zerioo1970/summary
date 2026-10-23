// code/ch07/Exercise/Program.cs
// 第 7 章练习参考答案：SQL 一字不改，靠全局开关 + 可空类型 + 枚举 + DateOnly 把客户汇总映射出来
using Dapper;
using Microsoft.Data.SqlClient;

// 列名都是下划线风格，SQL 又不许改 → 打开全局开关，而且要在第一次查询之前
DefaultTypeMap.MatchNamesWithUnderscores = true;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 题目给定的 SQL，不许改
const string sql = @"
    SELECT c.Name                          AS customer_name,
           COUNT(r.Id)                     AS request_count,
           MAX(CAST(r.CreatedAt AS date))  AS last_request_date,
           (SELECT TOP 1 r2.Status
            FROM dbo.PriceRequest r2
            WHERE r2.CustomerId = c.Id
            ORDER BY r2.CreatedAt DESC)    AS last_status
    FROM dbo.Customer c
    LEFT JOIN dbo.PriceRequest r ON r.CustomerId = c.Id
    GROUP BY c.Id, c.Name
    ORDER BY c.Id";

foreach (var s in conn.Query<CustomerSummary>(sql))
{
    string date = s.LastRequestDate?.ToString("yyyy-MM-dd") ?? "（无）";
    string status = s.LastStatus?.ToString() ?? "（无）";
    Console.WriteLine($"{s.CustomerName}：{s.RequestCount} 单，最近 {date}，状态 {status}");
}

public enum RequestStatus : byte
{
    Draft = 0,
    Pending = 1,
    Approved = 2,
    Rejected = 3,
}

public class CustomerSummary
{
    public string CustomerName { get; set; } = "";    // customer_name
    public int RequestCount { get; set; }             // request_count：COUNT 不会是 NULL
    public DateOnly? LastRequestDate { get; set; }    // 没申请过的客户是 NULL → 必须可空
    public RequestStatus? LastStatus { get; set; }    // 同上
}
