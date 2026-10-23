// code/ch12/Step1-PageAndTotal/Program.cs
// QueryMultiple 入门：一次往返同时拿到"当前页数据"和"总条数"，并数一数和服务器往返了几次
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);
conn.Open();                      // 手动打开：下面要读这个连接对象上的统计信息
conn.StatisticsEnabled = true;    // ADO.NET 自带的连接统计，ServerRoundtrips = 与服务器往返的次数

// 当前页：跳过 @Skip 行，取 @Take 行（OFFSET…FETCH 的细节第 13 章讲）
const string pageSql = @"
SELECT Id, RequestNo, Applicant, Status
FROM dbo.PriceRequest
ORDER BY Id
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

// 总条数：用来算"共几页"
const string countSql = @"
SELECT COUNT(*) FROM dbo.PriceRequest;";

var pageArgs = new { Skip = 0, Take = 2 };   // 第 1 页，每页 2 条

// 做法 A：分两次查——你以前的习惯写法
conn.ResetStatistics();
var rowsA = conn.Query<RequestRow>(pageSql, pageArgs).ToList();
int totalA = conn.ExecuteScalar<int>(countSql);
Console.WriteLine($"做法 A 两次查询：本页 {rowsA.Count} 条，总数 {totalA}，往返 {Roundtrips(conn)} 次");

// 做法 B：两条 SELECT 拼成一个批（batch），QueryMultiple 一次发给服务器
conn.ResetStatistics();
using (var grid = conn.QueryMultiple(pageSql + countSql, pageArgs))
{
    List<RequestRow> rows = grid.Read<RequestRow>().ToList();   // 读第 1 个结果集：当前页
    int total = grid.ReadSingle<int>();                          // 读第 2 个结果集：总条数（只有 1 行 1 列）

    Console.WriteLine($"做法 B QueryMultiple：本页 {rows.Count} 条，总数 {total}，往返 {Roundtrips(conn)} 次");

    int pageCount = (total + pageArgs.Take - 1) / pageArgs.Take;        // 向上取整：3 条、每页 2 条 → 2 页
    Console.WriteLine($"第 1 页 / 共 {pageCount} 页（共 {total} 条）");
    foreach (var r in rows)
        Console.WriteLine($"  {r.Id}  {r.RequestNo}  {r.Applicant}  状态={r.Status}");
}

// 从连接统计里取出往返次数（值是 long，装在 object 里）
static long Roundtrips(SqlConnection c) => (long)c.RetrieveStatistics()["ServerRoundtrips"]!;

public class RequestRow
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
    public int Status { get; set; }
}
