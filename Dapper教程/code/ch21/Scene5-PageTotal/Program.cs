// code/ch21/Scene5-PageTotal/Program.cs
// 场景 5：分页总数不对——列表只有 2 张单子，页脚却写"共 3 条"
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 查询条件：包含探头类产品（编码以 PB 开头）的申请单。每页 10 条，看第 1 页
var filter = new { CodePrefix = "PB%", Skip = 0, Take = 10 };

Console.WriteLine("== 故障现象");
// 总数和当前页用 JOIN 写了同样的条件，一次往返取回（第 12 章 QueryMultiple）
using (var grid = conn.QueryMultiple(@"
    SELECT COUNT(*)
    FROM dbo.PriceRequest r
    JOIN dbo.PriceRequestItem i ON i.RequestId = r.Id
    JOIN dbo.Product p ON p.Id = i.ProductId
    WHERE p.Code LIKE @CodePrefix;

    SELECT DISTINCT r.Id, r.RequestNo
    FROM dbo.PriceRequest r
    JOIN dbo.PriceRequestItem i ON i.RequestId = r.Id
    JOIN dbo.Product p ON p.Id = i.ProductId
    WHERE p.Code LIKE @CodePrefix
    ORDER BY r.Id
    OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;", filter))
{
    int total = grid.ReadSingle<int>();
    var page = grid.Read<RequestRow>().ToList();
    foreach (var r in page) Console.WriteLine($"  {r.Id}  {r.RequestNo}");
    Console.WriteLine($"  本页 {page.Count} 条，共 {total} 条");
}

Console.WriteLine("== 定位：去掉 COUNT，看 JOIN 之后到底有几行");
foreach (var x in conn.Query<JoinRow>(@"
    SELECT r.RequestNo, p.Code
    FROM dbo.PriceRequest r
    JOIN dbo.PriceRequestItem i ON i.RequestId = r.Id
    JOIN dbo.Product p ON p.Id = i.ProductId
    WHERE p.Code LIKE @CodePrefix
    ORDER BY r.Id, p.Code", filter))
    Console.WriteLine($"  {x.RequestNo}  {x.Code}");

Console.WriteLine("== 修复：用 EXISTS 判断\"有没有\"，主表一行就只算一行");
using (var grid = conn.QueryMultiple(@"
    SELECT COUNT(*)
    FROM dbo.PriceRequest r
    WHERE EXISTS (SELECT 1 FROM dbo.PriceRequestItem i
                  JOIN dbo.Product p ON p.Id = i.ProductId
                  WHERE i.RequestId = r.Id AND p.Code LIKE @CodePrefix);

    SELECT r.Id, r.RequestNo
    FROM dbo.PriceRequest r
    WHERE EXISTS (SELECT 1 FROM dbo.PriceRequestItem i
                  JOIN dbo.Product p ON p.Id = i.ProductId
                  WHERE i.RequestId = r.Id AND p.Code LIKE @CodePrefix)
    ORDER BY r.Id
    OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;", filter))
{
    int total = grid.ReadSingle<int>();
    var page = grid.Read<RequestRow>().ToList();
    foreach (var r in page) Console.WriteLine($"  {r.Id}  {r.RequestNo}");
    Console.WriteLine($"  本页 {page.Count} 条，共 {total} 条");
}

public class RequestRow
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
}

public class JoinRow
{
    public string RequestNo { get; set; } = "";
    public string Code { get; set; } = "";
}
