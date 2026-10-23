// code/ch07/Step3-Null/Program.cs
// 数据库里的 NULL 到了 C# 变成什么：string? 、有初始值的属性、decimal 与 decimal?
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

const string remarkSql = "SELECT RequestNo, Remark FROM dbo.PriceRequest ORDER BY Id";

// 例 1：Remark 列允许 NULL（2 号单就是 NULL），属性写成 string? —— 拿到的就是 null
Console.WriteLine("例 1 string?：");
foreach (var r in conn.Query<RemarkNullable>(remarkSql))
    Console.WriteLine($"  {r.RequestNo}  Remark = {(r.Remark is null ? "null" : r.Remark)}");

// 例 2：属性写成 string 并给了初始值 —— 遇到 NULL 时 Dapper 不赋值，初始值原样保留
Console.WriteLine("例 2 有初始值的 string：");
foreach (var r in conn.Query<RemarkWithDefault>(remarkSql))
    Console.WriteLine($"  {r.RequestNo}  Remark = {r.Remark}");

// 例 3：报表 —— 每个客户的申请总金额。LEFT JOIN 后没申请过的客户，SUM 结果是 NULL
const string totalSql = @"
    SELECT c.Name, SUM(i.Qty * i.RequestPrice) AS Total
    FROM dbo.Customer c
    LEFT JOIN dbo.PriceRequest r     ON r.CustomerId = c.Id
    LEFT JOIN dbo.PriceRequestItem i ON i.RequestId  = r.Id
    GROUP BY c.Id, c.Name
    ORDER BY c.Id";

Console.WriteLine("例 3a 用 decimal 接：");
foreach (var t in conn.Query<CustomerTotal>(totalSql))
    Console.WriteLine($"  {t.Name}  {t.Total:N2}");          // NULL → 0.00，看不出是"没申请"

Console.WriteLine("例 3b 用 decimal? 接：");
foreach (var t in conn.Query<CustomerTotalNullable>(totalSql))
    Console.WriteLine($"  {t.Name}  {(t.Total is null ? "（无申请）" : t.Total.Value.ToString("N2"))}");

// 例 4：只取一个值时，ExecuteScalar<decimal> 遇到 NULL 也悄悄给 0（第 5 章讲过要用 decimal?）
decimal scalar = conn.ExecuteScalar<decimal>(@"
    SELECT SUM(i.Qty * i.RequestPrice)
    FROM dbo.PriceRequestItem i
    JOIN dbo.PriceRequest r ON r.Id = i.RequestId
    WHERE r.CustomerId = 3");
Console.WriteLine($"例 4 ExecuteScalar<decimal> 北方影像诊所：{scalar:N2}");

public class RemarkNullable
{
    public string RequestNo { get; set; } = "";
    public string? Remark { get; set; }                  // 可空：NULL 就是 null
}

public class RemarkWithDefault
{
    public string RequestNo { get; set; } = "";
    public string Remark { get; set; } = "（无备注）";     // NULL 时保留这个初始值
}

public class CustomerTotal
{
    public string Name { get; set; } = "";
    public decimal Total { get; set; }                   // 非可空：NULL 时保持默认值 0
}

public class CustomerTotalNullable
{
    public string Name { get; set; } = "";
    public decimal? Total { get; set; }                  // 可空：NULL 和 0 能分开
}
