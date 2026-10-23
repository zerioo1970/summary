// code/ch13/Exercise/Program.cs
// 第 13 章练习参考答案：按"客户等级 + 状态列表"动态查询，按创建时间升序分页
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 先执行 ch13-more-requests.sql
Search("练习 1 状态=待审或通过，第 2 页", level: null, statuses: new[] { 1, 2 }, pageIndex: 2, pageSize: 3);
Search("练习 2 客户等级=A，状态不限，第 1 页", level: "A", statuses: Array.Empty<int>(), pageIndex: 1, pageSize: 3);

void Search(string title, string? level, int[] statuses, int pageIndex, int pageSize)
{
    var where = new StringBuilder(" WHERE 1 = 1");
    var p = new DynamicParameters();

    if (!string.IsNullOrWhiteSpace(level))
    {
        where.Append(" AND c.Level = @Level");
        p.Add("Level", level);
    }
    if (statuses.Length > 0)                     // 空数组 = 不限：根本不拼这个条件
    {
        where.Append(" AND r.Status IN @Statuses"); // IN @列表 不写括号（第 4 章讲过）
        p.Add("Statuses", statuses);
    }

    p.Add("Skip", (Math.Max(pageIndex, 1) - 1) * pageSize);
    p.Add("Take", pageSize);

    string from = $@"
FROM dbo.PriceRequest r
JOIN dbo.Customer c ON c.Id = r.CustomerId{where}";

    string sql = $@"
SELECT r.Id, r.RequestNo, c.Name AS CustomerName, r.Status, r.CreatedAt{from}
ORDER BY r.CreatedAt, r.Id
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;

SELECT COUNT(*){from};";

    using var grid = conn.QueryMultiple(sql, p);
    var rows = grid.Read<RequestRow>().ToList();
    int total = grid.ReadSingle<int>();

    Console.WriteLine($"== {title}：共 {total} 条，本页 {rows.Count} 条");
    foreach (var r in rows)
        Console.WriteLine($"   {r.RequestNo}  {r.CustomerName}  状态={r.Status}  {r.CreatedAt:MM-dd}");
}

public class RequestRow
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
