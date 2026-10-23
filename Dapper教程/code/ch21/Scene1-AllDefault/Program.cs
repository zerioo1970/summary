// code/ch21/Scene1-AllDefault/Program.cs
// 场景 1：属性全是默认值——先打印结果集的真实列名，再和类的属性名逐个对照
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

// 从老报表里原样复制来的 SQL：为了报表表头好看，列别名写成了中文
const string oldSql = @"
    SELECT r.RequestNo AS 单号, r.Applicant AS 申请人, c.Name AS 客户
    FROM dbo.PriceRequest r
    JOIN dbo.Customer c ON c.Id = r.CustomerId
    ORDER BY r.Id";

using var conn = new SqlConnection(connStr);

Console.WriteLine("== 故障现象：三行都查到了，但每个属性都是空的");
foreach (var row in conn.Query<RequestReport>(oldSql))
    Console.WriteLine($"[{row.RequestNo}] [{row.Applicant}] [{row.CustomerName}]");

Console.WriteLine("== 定位：结果集里真实的列名");
PrintColumns(connStr, oldSql);

// 修复：别名改成属性名。属性名本来就和列名一样的两列，干脆不起别名
const string fixedSql = @"
    SELECT r.RequestNo, r.Applicant, c.Name AS CustomerName
    FROM dbo.PriceRequest r
    JOIN dbo.Customer c ON c.Id = r.CustomerId
    ORDER BY r.Id";

Console.WriteLine("== 修复后");
foreach (var row in conn.Query<RequestReport>(fixedSql))
    Console.WriteLine($"[{row.RequestNo}] [{row.Applicant}] [{row.CustomerName}]");

// 只读"表头"不读数据：用你熟悉的 ADO.NET，SchemaOnly 让服务器只返回列信息
static void PrintColumns(string connStr, string sql)
{
    using var conn = new SqlConnection(connStr);
    conn.Open();
    using var cmd = new SqlCommand(sql, conn);
    using var reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly);
    for (int i = 0; i < reader.FieldCount; i++)
        Console.WriteLine($"  第 {i} 列：{reader.GetName(i)}（{reader.GetFieldType(i).Name}）");
}

public class RequestReport
{
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
    public string CustomerName { get; set; } = "";
}
