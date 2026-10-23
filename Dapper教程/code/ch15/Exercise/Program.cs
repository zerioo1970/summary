// code/ch15/Exercise/Program.cs
// 第 15 章练习参考答案：练习 1（表值参数按一批编码查产品）+ 练习 2（流式统计 20 万行）
// 运行前先执行 code/sql/ch15-tvp.sql（创建 dbo.ProductCodeType）
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 练习 1：一批编码装进 DataTable，用表值参数一次传过去，和 dbo.Product 做 JOIN
string[] codes = { "US-200", "PB-01", "XX-999" };
var codeTable = new DataTable();
codeTable.Columns.Add("Code", typeof(string));      // 只有一列，和 dbo.ProductCodeType 一致
foreach (var c in codes)
    codeTable.Rows.Add(c);

var found = conn.Query<ProductRow>(@"
    SELECT p.Code, p.Name, p.ListPrice
    FROM dbo.Product p
    JOIN @Codes c ON c.Code = p.Code               -- 类型里 Code 是 VARCHAR(20)，和表列一致，没有隐式转换
    ORDER BY p.Id",
    new { Codes = codeTable.AsTableValuedParameter("dbo.ProductCodeType") }).ToList();

foreach (var p in found)
    Console.WriteLine($"{p.Code,-8}{p.Name,-8}{p.ListPrice,12:N2}");
var missing = codes.Except(found.Select(p => p.Code));
Console.WriteLine($"找到 {found.Count} 个，没找到：{string.Join("、", missing)}");

// 练习 2：流式读 20 万行，只统计 Amount >= 900 的行，读到一半看内存
const string sql = @"
SELECT Id,
       CONCAT('PR', 1000000 + Id)       AS RequestNo,
       CAST(Id % 1000 AS DECIMAL(18,2)) AS Amount
FROM (SELECT TOP (200000) CAST(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS INT) AS Id
      FROM sys.all_objects a CROSS JOIN sys.all_objects b) t";

long before = GC.GetTotalMemory(forceFullCollection: true);
long halfway = 0;
int seen = 0, bigCount = 0;
decimal bigTotal = 0;

foreach (var row in conn.Query<ReportRow>(sql, buffered: false))   // 不缓冲：边读边算，读过的行随时可回收
{
    seen++;
    if (row.Amount >= 900)
    {
        bigCount++;
        bigTotal += row.Amount;
    }
    if (seen == 100_000)
        halfway = GC.GetTotalMemory(forceFullCollection: true) - before;
}

Console.WriteLine($"Amount >= 900：{bigCount:N0} 行，合计 {bigTotal:N0}");
Console.WriteLine($"读到一半时内存不到 1 MB：{halfway < 1024 * 1024}");

public class ProductRow
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal ListPrice { get; set; }
}

public class ReportRow
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public decimal Amount { get; set; }
}
