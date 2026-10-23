// code/ch07/Step1-Alias/Program.cs
// 列名是下划线风格、属性是驼峰风格：先看对不上的样子，再用 SQL 别名逐个对上
// 运行前先执行 code/sql/ch07-legacy-view.sql
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 例 1：原样查老表 —— request_no 和 RequestNo 名字不一样（多了下划线），对不上
var a = conn.QuerySingle<LegacyRequest>(@"
    SELECT request_id, request_no, applicant_name, created_at
    FROM dbo.legacy_price_request
    WHERE request_id = 1");
Console.WriteLine($"例 1 不处理：Id={a.RequestId}，单号=[{a.RequestNo}]，申请人=[{a.ApplicantName}]，" +
                  $"创建={a.CreatedAt:yyyy-MM-dd}");

// 例 2：办法一 —— 每一列用 AS 起一个和属性同名的别名（第 2 章讲过别名）
var b = conn.QuerySingle<LegacyRequest>(@"
    SELECT request_id     AS RequestId,
           request_no     AS RequestNo,
           applicant_name AS ApplicantName,
           created_at     AS CreatedAt
    FROM dbo.legacy_price_request
    WHERE request_id = 1");
Console.WriteLine($"例 2 起别名：Id={b.RequestId}，单号=[{b.RequestNo}]，申请人=[{b.ApplicantName}]，" +
                  $"创建={b.CreatedAt:yyyy-MM-dd}");

public class LegacyRequest
{
    public int RequestId { get; set; }
    public string RequestNo { get; set; } = "";
    public string ApplicantName { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
