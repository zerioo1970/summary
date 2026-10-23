// code/ch08/Step4-ResultSet/Program.cs
// 存储过程既返回结果集、又带 OUTPUT 参数：分页查申请单 + 总行数
// 运行前先执行 code/sql/ch08-procs.sql
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

for (int page = 1; page <= 3; page++)
{
    // 输入参数直接用匿名对象传给 DynamicParameters 的构造函数，省得一个个 Add
    var p = new DynamicParameters(new { Applicant = "张伟", PageIndex = page, PageSize = 1 });
    p.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

    // 过程返回的结果集，和普通 SELECT 一样用 Query<T> 接，映射规则也一样
    List<RequestListRow> rows = conn.Query<RequestListRow>(
        "dbo.Select_PriceRequestPage_Proc", p,
        commandType: CommandType.StoredProcedure).ToList();

    int total = p.Get<int>("@TotalCount");      // Query 默认缓冲：返回时结果已读完，OUTPUT 已到手
    string text = rows.Count == 0
        ? "（这一页没有数据）"
        : string.Join("、", rows.Select(r => $"{r.RequestNo} {r.Status} {r.CreatedAt:MM-dd}"));
    Console.WriteLine($"第 {page} 页 / 共 {total} 行：{text}");
}

public enum RequestStatus : byte { Draft = 0, Pending = 1, Approved = 2, Rejected = 3 }

public class RequestListRow
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
    public RequestStatus Status { get; set; }     // TINYINT → 枚举，第 7 章讲过
    public DateTime CreatedAt { get; set; }
}
