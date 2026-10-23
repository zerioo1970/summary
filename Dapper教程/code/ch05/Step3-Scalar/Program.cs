// code/ch05/Step3-Scalar/Program.cs
// ExecuteScalar<T>：只要第一行第一列的一个值——统计、求和、取最大单号
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 例 1：COUNT —— 一定有一行一列，用 int 接
int pending = conn.ExecuteScalar<int>(
    "SELECT COUNT(*) FROM dbo.PriceRequest WHERE Status = @Status", new { Status = 1 });
Console.WriteLine($"例 1 待审申请单：{pending} 张");

// 例 2：SUM —— 第 1 号单有明细
const string sumSql = "SELECT SUM(Qty * RequestPrice) FROM dbo.PriceRequestItem WHERE RequestId = @Id";
decimal? total1 = conn.ExecuteScalar<decimal?>(sumSql, new { Id = 1 });
Console.WriteLine($"例 2 第 1 号单金额：{total1:N2}");

// 例 3：SUM 没有匹配行时，SQL 返回的是 NULL（不是 0）——用 decimal? 接，就能看出"没有明细"
decimal? total99 = conn.ExecuteScalar<decimal?>(sumSql, new { Id = 99 });
Console.WriteLine($"例 3 第 99 号单金额：{(total99 is null ? "null（没有明细）" : total99.Value.ToString("N2"))}");

// 例 4：同样的 NULL，用 decimal 接——不报错，悄悄变成 0
decimal total99b = conn.ExecuteScalar<decimal>(sumSql, new { Id = 99 });
Console.WriteLine($"例 4 用 decimal 接：{total99b:N2}");

// 例 5：多行多列——只取第一行第一列，其余丢掉，不报错
string? firstNo = conn.ExecuteScalar<string>(
    "SELECT RequestNo, Applicant FROM dbo.PriceRequest ORDER BY Id DESC");
Console.WriteLine($"例 5 最新单号：{firstNo}");

// 例 6：0 行——返回 default(T)：string 是 null
string? none = conn.ExecuteScalar<string>(
    "SELECT RequestNo FROM dbo.PriceRequest WHERE Applicant = @Name", new { Name = "王五" });
Console.WriteLine($"例 6 王五的单号：{none ?? "null"}");
