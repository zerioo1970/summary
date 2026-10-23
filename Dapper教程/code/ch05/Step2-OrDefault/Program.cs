// code/ch05/Step2-OrDefault/Program.cs
// OrDefault 查不到时返回 default(T)：类是 null，int 是 0——而 0 可能正好是一个合法值
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 例 1：T 是类——查不到返回 null，判断一下就能区分
Console.WriteLine("== 例 1 T 是类");
foreach (var no in new[] { "PR20261008", "PR404" })
{
    var r = conn.QueryFirstOrDefault<RequestBrief>(
        "SELECT RequestNo, Applicant, Status FROM dbo.PriceRequest WHERE RequestNo = @No", new { No = no });
    Console.WriteLine(r is null ? $"{no}：不存在" : $"{no}：申请人 {r.Applicant}，状态 {r.Status}");
}

// 例 2：陷阱——T 是 int，查不到返回 0；而状态 0 正好表示"草稿"
Console.WriteLine("== 例 2 T 是 int");
foreach (var no in new[] { "PR20261015", "PR404" })
{
    int status = conn.QueryFirstOrDefault<int>(
        "SELECT Status FROM dbo.PriceRequest WHERE RequestNo = @No", new { No = no });
    Console.WriteLine($"{no}：Status = {status}");   // 两行都是 0，分不出"草稿"和"不存在"
}

// 例 3：改成 int?——查不到返回 null，查到 0 就是真的 0
Console.WriteLine("== 例 3 T 是 int?");
foreach (var no in new[] { "PR20261015", "PR404" })
{
    int? status = conn.QueryFirstOrDefault<int?>(
        "SELECT Status FROM dbo.PriceRequest WHERE RequestNo = @No", new { No = no });
    Console.WriteLine(status is null ? $"{no}：不存在" : $"{no}：Status = {status}");
}

// 例 4：string 也有类似问题——"查不到"和"列值是 NULL"都返回 null
Console.WriteLine("== 例 4 T 是 string?");
foreach (var no in new[] { "PR20261001", "PR20261008", "PR404" })
{
    string? remark = conn.QueryFirstOrDefault<string?>(
        "SELECT Remark FROM dbo.PriceRequest WHERE RequestNo = @No", new { No = no });
    Console.WriteLine($"{no}：Remark = {remark ?? "null"}");   // PR20261008 的备注本来就是 NULL
}

public class RequestBrief
{
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
    public int Status { get; set; }
}
