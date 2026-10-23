// code/ch13/Step1-OffsetFetch/Program.cs
// OFFSET…FETCH 分页：第 N 页 = 跳过 (N-1)×每页条数 行，再取"每页条数"行；总数用第 12 章的 QueryMultiple 一起取
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

const int pageSize = 4;                   // 每页 4 条；先执行 ch13-more-requests.sql，表里共 10 条

for (int page = 1; page <= 4; page++)     // 10 条、每页 4 条 → 共 3 页；第 4 页故意越界
    ShowPage(page);
ShowPage(0);                              // 页码 0：界面传错参数时也不能崩

void ShowPage(int pageIndex)
{
    var (rows, total) = GetPage(pageIndex, pageSize);
    int pageCount = (total + pageSize - 1) / pageSize;   // 向上取整：10 / 4 → 3
    Console.WriteLine($"请求第 {pageIndex} 页 / 共 {pageCount} 页（共 {total} 条），本页 {rows.Count} 条");
    foreach (var r in rows)
        Console.WriteLine($"  {r.Id,2}  {r.RequestNo}  {r.Applicant}  {r.CreatedAt:yyyy-MM-dd}");
}

(List<RequestRow> Rows, int Total) GetPage(int pageIndex, int size)
{
    if (pageIndex < 1) pageIndex = 1;     // 页码 0 或负数会算出负的 OFFSET，服务器直接报错（见"改坏一处试试"）

    // ORDER BY 是 OFFSET 的硬性前提；最后加 Id 作为"决胜列"，保证顺序唯一、翻页不重不漏
    const string sql = @"
SELECT Id, RequestNo, Applicant, Status, CreatedAt
FROM dbo.PriceRequest
ORDER BY CreatedAt DESC, Id DESC
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;

SELECT COUNT(*) FROM dbo.PriceRequest;";

    using var grid = conn.QueryMultiple(sql, new { Skip = (pageIndex - 1) * size, Take = size });
    var rows = grid.Read<RequestRow>().ToList();   // 按顺序：先读当前页
    int total = grid.ReadSingle<int>();            // 再读总数
    return (rows, total);
}

public class RequestRow
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
