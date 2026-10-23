// code/ch21/Scene6-SplitOn/Program.cs
// 场景 6：多重映射属性错位——客户城市有值，客户名称却是空的
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 同事加需求时，把 c.Name 插到了 c.Id 前面
const string badSql = @"
    SELECT r.Id, r.RequestNo, r.Applicant,
           c.Name, c.Id, c.City
    FROM dbo.PriceRequest r
    JOIN dbo.Customer c ON c.Id = r.CustomerId
    ORDER BY r.Id";

Console.WriteLine("== 故障现象");
Print(conn, badSql);

Console.WriteLine("== 定位：在每个切分点前后打印列名");
var columns = new[] { "r.Id", "r.RequestNo", "r.Applicant", "c.Name", "c.Id", "c.City" };
int split = Array.LastIndexOf(columns, "c.Id");    // splitOn 默认是 "Id"，从右往左找第一个（第 10 章）
Console.WriteLine($"  给 PriceRequest：{string.Join(", ", columns[..split])}");
Console.WriteLine($"  给 Customer    ：{string.Join(", ", columns[split..])}");

Console.WriteLine("== 修复：第二个对象的列，以切分列 c.Id 打头");
Print(conn, @"
    SELECT r.Id, r.RequestNo, r.Applicant,
           c.Id, c.Name, c.City
    FROM dbo.PriceRequest r
    JOIN dbo.Customer c ON c.Id = r.CustomerId
    ORDER BY r.Id");

static void Print(SqlConnection conn, string sql)
{
    var list = conn.Query<PriceRequest, Customer, PriceRequest>(sql,
        (r, c) => { r.Customer = c; return r; });    // splitOn 不写，默认 "Id"
    foreach (var r in list)
        Console.WriteLine($"  {r.RequestNo}  {r.Applicant}  客户 {r.Customer!.Id} [{r.Customer.Name}] {r.Customer.City}");
}

public class PriceRequest
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
    public Customer? Customer { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
}
