// code/ch13/Step2-DynamicWhere/Program.cs
// 动态 WHERE：用户填了哪个条件就拼哪段"固定的 SQL 片段"，值一律走参数
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 模拟查询页上的 5 种填法（先执行 ch13-more-requests.sql，表里共 10 条）
Search("例 1 什么都不填", new RequestFilter());
Search("例 2 申请人=张伟", new RequestFilter { Applicant = "张伟" });
Search("例 3 申请人=张伟 且 状态=待审", new RequestFilter { Applicant = "张伟", Status = 1 });
Search("例 4 客户=3 且 11-10 之后", new RequestFilter { CustomerId = 3, DateFrom = new DateTime(2026, 11, 10) });
Search("例 5 状态=草稿（值是 0）", new RequestFilter { Status = 0 });

void Search(string title, RequestFilter f)
{
    var sql = new StringBuilder(@"
SELECT Id, RequestNo, CustomerId, Applicant, Status, CreatedAt
FROM dbo.PriceRequest
WHERE 1 = 1");                            // 1 = 1 恒为真：后面每个条件都能统一以 AND 开头

    // 只拼"写死在代码里的片段"，片段里只有 @参数名，用户输入的值从不进入 SQL 文本
    if (!string.IsNullOrWhiteSpace(f.Applicant)) sql.Append(" AND Applicant = @Applicant");
    if (f.Status.HasValue)                       sql.Append(" AND Status = @Status");      // 用 HasValue，0（草稿）也是合法条件
    if (f.CustomerId.HasValue)                   sql.Append(" AND CustomerId = @CustomerId");
    if (f.DateFrom.HasValue)                     sql.Append(" AND CreatedAt >= @DateFrom");
    sql.Append(" ORDER BY Id;");

    // 参数直接传整个 filter 对象：属性名和 @参数名一致就对得上（第 4 章讲过）
    var rows = conn.Query<RequestRow>(sql.ToString(), f).ToList();

    Console.WriteLine($"== {title}：{rows.Count} 条");
    Console.WriteLine("   WHERE 部分：" + sql.ToString().Split("WHERE ")[1].Replace(" ORDER BY Id;", ""));
    foreach (var r in rows)
        Console.WriteLine($"   {r.Id,2}  {r.RequestNo}  客户{r.CustomerId}  {r.Applicant}  状态={r.Status}  {r.CreatedAt:MM-dd}");
}

// 查询页的条件：全部可空，null = 用户没填 = 不限
public class RequestFilter
{
    public string? Applicant { get; set; }
    public int? Status { get; set; }
    public int? CustomerId { get; set; }
    public DateTime? DateFrom { get; set; }
}

public class RequestRow
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public int CustomerId { get; set; }
    public string Applicant { get; set; } = "";
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
