// code/ch13/Step3-DynamicParameters/Program.cs
// 完整的查询页：动态 WHERE + DynamicParameters 按需加参数 + 排序白名单 + 分页 + 总数
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 排序白名单：界面只传"排序键"，真正的 ORDER BY 片段由代码决定
// 每一项最后都带 Id，保证顺序唯一（13.2 节讲过翻页不重不漏的前提）
var sortMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    ["date"]      = "CreatedAt DESC, Id DESC",   // 默认：最新的在前
    ["no"]        = "RequestNo, Id",
    ["applicant"] = "Applicant, CreatedAt DESC, Id",
};

// 先执行 ch13-more-requests.sql，表里共 10 条
Run("例 1 李娜，11-01 到 11-18（含当天）",
    new SearchForm { Applicant = "李娜", DateFrom = new DateTime(2026, 11, 1), DateTo = new DateTime(2026, 11, 18) });
Run("例 2 单号以 PR202611 开头、待审，按单号，第 1 页",
    new SearchForm { RequestNoPrefix = "PR202611", Status = 1, Sort = "no", PageIndex = 1, PageSize = 2 });
Run("例 3 同上，第 2 页",
    new SearchForm { RequestNoPrefix = "PR202611", Status = 1, Sort = "no", PageIndex = 2, PageSize = 2 });
Run("例 4 排序键被人改成了一段 SQL",
    new SearchForm
    {
        Applicant = "王强",
        // 有人绕过界面，直接在请求里把排序键改成了这一串（含一条 DROP TABLE）
        Sort = "Id OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY; DROP TABLE dbo.ApprovalLog; SELECT 1 AS Id ORDER BY Id",
    });

// 例 4 之后检查：审批记录表还在不在
bool exists = conn.ExecuteScalar<int?>("SELECT OBJECT_ID('dbo.ApprovalLog')") is not null;
Console.WriteLine($"审批记录表 dbo.ApprovalLog 还在吗？{exists}");

void Run(string title, SearchForm f)
{
    var where = new StringBuilder(" WHERE 1 = 1");
    var p = new DynamicParameters();     // 一个空的参数袋：用到哪个条件，就往里放哪个参数

    if (!string.IsNullOrWhiteSpace(f.Applicant))
    {
        where.Append(" AND Applicant = @Applicant");
        p.Add("Applicant", f.Applicant);
    }
    if (f.Status.HasValue)
    {
        where.Append(" AND Status = @Status");
        p.Add("Status", f.Status.Value);
    }
    if (!string.IsNullOrWhiteSpace(f.RequestNoPrefix))
    {
        where.Append(" AND RequestNo LIKE @NoPrefix");
        p.Add("NoPrefix", f.RequestNoPrefix + "%");          // 值要加工：在 C# 里拼 %，SQL 文本不变
    }
    if (f.DateFrom.HasValue)
    {
        where.Append(" AND CreatedAt >= @DateFrom");
        p.Add("DateFrom", f.DateFrom.Value.Date);
    }
    if (f.DateTo.HasValue)
    {
        where.Append(" AND CreatedAt < @DateToNext");        // "含当天" = 小于第二天 0 点
        p.Add("DateToNext", f.DateTo.Value.Date.AddDays(1));  // 参数名和表单属性名不同，匿名对象做不到这么自由
    }

    // 排序键不在白名单里就用默认值——绝不把 f.Sort 本身拼进 SQL
    string orderBy = sortMap.TryGetValue(f.Sort ?? "", out var o) ? o : sortMap["date"];

    int pageIndex = Math.Max(f.PageIndex, 1);
    p.Add("Skip", (pageIndex - 1) * f.PageSize);
    p.Add("Take", f.PageSize);

    string sql = $@"
SELECT Id, RequestNo, Applicant, Status, CreatedAt
FROM dbo.PriceRequest{where}
ORDER BY {orderBy}
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;

SELECT COUNT(*) FROM dbo.PriceRequest{where};";   // 总数用同一段 WHERE、同一批参数

    using var grid = conn.QueryMultiple(sql, p);
    var rows = grid.Read<RequestRow>().ToList();
    int total = grid.ReadSingle<int>();

    Console.WriteLine($"== {title}");
    Console.WriteLine($"   参数：{string.Join(", ", p.ParameterNames)}");   // 看看这次实际放了哪些参数
    Console.WriteLine($"   排序：{orderBy}");
    Console.WriteLine($"   第 {pageIndex} 页，本页 {rows.Count} 条 / 共 {total} 条");
    foreach (var r in rows)
        Console.WriteLine($"   {r.Id,2}  {r.RequestNo}  {r.Applicant}  状态={r.Status}  {r.CreatedAt:MM-dd HH:mm}");
}

// 查询页表单：条件全部可空；分页、排序给默认值
public class SearchForm
{
    public string? Applicant { get; set; }
    public int? Status { get; set; }
    public string? RequestNoPrefix { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Sort { get; set; } = "date";
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class RequestRow
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
