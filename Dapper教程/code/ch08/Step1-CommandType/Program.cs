// code/ch08/Step1-CommandType/Program.cs
// 调用只有输入参数的存储过程：过程名 + 匿名对象 + commandType: CommandType.StoredProcedure
// 运行前先执行 code/sql/ch08-procs.sql
using System.Data;                 // CommandType 在这个命名空间里
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 例 1：正规写法。第一个参数只写过程名，参数对象的属性名 = 过程的参数名（去掉 @）
var r1 = conn.QuerySingleOrDefault<PriceRequestRow>(
    "dbo.Select_PriceRequestByNo_Proc",
    new { RequestNo = "PR20261008" },
    commandType: CommandType.StoredProcedure);
Console.WriteLine($"例 1 找到：{r1?.RequestNo} {r1?.Applicant} 状态={r1?.Status} " +
                  $"备注={(r1?.Remark is null ? "null" : r1.Remark)}");

// 例 2：查不到时过程返回 0 行 → QuerySingleOrDefault 返回 null（第 5 章讲过）
var r2 = conn.QuerySingleOrDefault<PriceRequestRow>(
    "dbo.Select_PriceRequestByNo_Proc",
    new { RequestNo = "PR99999999" },
    commandType: CommandType.StoredProcedure);
Console.WriteLine($"例 2 查不到：结果是 null 吗？{r2 is null}");

// 例 3：另一种写法 —— 当普通 SQL 文本执行 EXEC，参数要自己在文本里一一对应
var r3 = conn.QuerySingleOrDefault<PriceRequestRow>(
    "EXEC dbo.Select_PriceRequestByNo_Proc @RequestNo = @RequestNo",
    new { RequestNo = "PR20261001" });
Console.WriteLine($"例 3 EXEC 文本：{r3?.RequestNo} {r3?.Applicant} 状态={r3?.Status}");

public class PriceRequestRow
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public int CustomerId { get; set; }
    public string Applicant { get; set; } = "";
    public byte Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Remark { get; set; }           // 允许 NULL，第 7 章讲过
}
