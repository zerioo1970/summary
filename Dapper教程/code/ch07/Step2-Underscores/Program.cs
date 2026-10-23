// code/ch07/Step2-Underscores/Program.cs
// 全局开关 MatchNamesWithUnderscores：打开后 request_no 能自动对上 RequestNo，SQL 不用改
// 运行前先执行 code/sql/ch07-legacy-view.sql
using Dapper;
using Microsoft.Data.SqlClient;

// 程序一启动就打开，必须在第一次查询之前（原因见"改坏一处试试"）
DefaultTypeMap.MatchNamesWithUnderscores = true;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 例 1：SQL 和 7.2 例 1 一字不差，这次能对上了
var list = conn.Query<LegacyRequest>(@"
    SELECT request_id, request_no, applicant_name, created_at
    FROM dbo.legacy_price_request
    ORDER BY request_id");
foreach (var r in list)
    Console.WriteLine($"例 1 {r.RequestId} {r.RequestNo} {r.ApplicantName} {r.CreatedAt:yyyy-MM-dd}");

// 例 2：开关打开后，普通的同名列照样能对上，不受影响
var p = conn.QuerySingle<ProductBrief>(
    "SELECT Code, ListPrice FROM dbo.Product WHERE Id = 1");
Console.WriteLine($"例 2 普通列：{p.Code} {p.ListPrice:N2}");

// 例 3："全局"的意思：整个程序里所有类、所有查询都按这个规则匹配，包括别的类
var c = conn.QuerySingle<CustomerBrief>(
    "SELECT Name AS customer_name, City AS customer_city FROM dbo.Customer WHERE Id = 2");
Console.WriteLine($"例 3 别的类：{c.CustomerName} {c.CustomerCity}");

public class LegacyRequest
{
    public int RequestId { get; set; }
    public string RequestNo { get; set; } = "";
    public string ApplicantName { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class ProductBrief
{
    public string Code { get; set; } = "";
    public decimal ListPrice { get; set; }
}

public class CustomerBrief
{
    public string CustomerName { get; set; } = "";
    public string CustomerCity { get; set; } = "";
}
