// code/ch02/Step3-Mapping/Program.cs
// 映射规则的五个实验：大小写、多出来的列、缺少的列、别名、类型转换
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 实验 1：列名大小写不同 —— 照样对上（不区分大小写）
var r1 = conn.Query<RequestBrief>(
    "SELECT ID, REQUESTNO, applicant, Status FROM dbo.PriceRequest WHERE Id = 1").First();
Console.WriteLine($"实验 1 大小写：{r1.Id} {r1.RequestNo} {r1.Applicant} 状态={r1.Status}");

// 实验 2：SQL 多查了类里没有的列（CustomerId、CreatedAt、Remark）—— 静默忽略，不报错
var r2 = conn.Query<RequestBrief>("SELECT * FROM dbo.PriceRequest WHERE Id = 1").First();
Console.WriteLine($"实验 2 多列：{r2.Id} {r2.RequestNo} {r2.Applicant}");

// 实验 3：类里有、SQL 没查的属性（Applicant）—— 也不报错，保持默认值 ""
var r3 = conn.Query<RequestBrief>(
    "SELECT Id, RequestNo, Status FROM dbo.PriceRequest WHERE Id = 1").First();
Console.WriteLine($"实验 3 少列：Applicant=[{r3.Applicant}]  ← 空的，没有任何提示");

// 实验 4：列名和属性名不一样 —— 在 SQL 里用 AS 起别名
var r4 = conn.Query<RequestRow>(
    "SELECT RequestNo AS No, Applicant AS Who FROM dbo.PriceRequest ORDER BY Id").ToList();
Console.WriteLine("实验 4 别名：" + string.Join("，", r4.Select(x => $"{x.No}/{x.Who}")));

// 实验 5：类型不完全一致 —— Status 列是 TINYINT（C# 里是 byte），属性是 int，自动转换
Console.WriteLine($"实验 5 类型：Status 属性类型 {r1.Status.GetType().Name}，值 {r1.Status}");

public class RequestBrief
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
    public int Status { get; set; }          // 列是 TINYINT，用 int 接也行
}

public class RequestRow
{
    public string No { get; set; } = "";     // 对应别名 No
    public string Who { get; set; } = "";    // 对应别名 Who
}
