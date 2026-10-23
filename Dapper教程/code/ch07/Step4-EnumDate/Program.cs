// code/ch07/Step4-EnumDate/Program.cs
// TINYINT 状态列 ↔ 枚举，DATE 列 ↔ DateOnly；读和写都演示；最后补充 record 构造函数映射
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 例 1：读 —— Status(TINYINT) 进枚举；CAST(... AS date) 得到 DATE 列，进 DateOnly
var list = conn.Query<RequestView>(@"
    SELECT Id, RequestNo, Status, CAST(CreatedAt AS date) AS CreatedDate
    FROM dbo.PriceRequest
    ORDER BY Id");
foreach (var r in list)
    Console.WriteLine($"例 1 {r.RequestNo}  {r.Status,-8}（{(byte)r.Status}）  {r.CreatedDate:yyyy-MM-dd}");

// 例 2：写 —— 枚举和 DateOnly 直接当参数用
var drafts = conn.Query<string>(@"
    SELECT RequestNo FROM dbo.PriceRequest
    WHERE Status = @Status AND CreatedAt >= @From",
    new { Status = RequestStatus.Draft, From = new DateOnly(2026, 10, 10) });
Console.WriteLine("例 2 10 月 10 日以后的草稿：" + string.Join("、", drafts));

// 例 3：库里出现了枚举没定义的值（9），或者列里存的是文字 —— 都不报错
var odd = conn.QuerySingle<StatusOnly>("SELECT CAST(9 AS tinyint) AS Status");
Console.WriteLine($"例 3a 值 9：Status = {odd.Status}，枚举里有定义吗？{Enum.IsDefined(odd.Status)}");
var text = conn.QuerySingle<StatusOnly>("SELECT 'Approved' AS Status");
Console.WriteLine($"例 3b 文字 'Approved'：Status = {text.Status}（{(byte)text.Status}）");

// 例 4（补充）：record 没有无参构造函数，Dapper 改用构造函数：
// 找一个参数个数、名字、顺序、类型都和结果集对得上的构造函数，把列值按顺序传进去
var rec = conn.QuerySingle<RequestRecord>(
    "SELECT Id, RequestNo, Status FROM dbo.PriceRequest WHERE Id = 2");
Console.WriteLine($"例 4 record：{rec}");

// 值从 0 开始，和表上的注释"0 草稿 1 待审 2 通过 3 驳回"逐个对应
// 写 : byte 让枚举的底层类型和 TINYINT 一致
public enum RequestStatus : byte
{
    Draft = 0,       // 草稿
    Pending = 1,     // 待审
    Approved = 2,    // 通过
    Rejected = 3,    // 驳回
}

public class RequestView
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public RequestStatus Status { get; set; }
    public DateOnly CreatedDate { get; set; }       // 只要日期，不要时分秒
}

public class StatusOnly
{
    public RequestStatus Status { get; set; }
}

// 构造函数参数的类型必须和列的类型严格一致：Status 列是 TINYINT，这里只能写 byte
public record RequestRecord(int Id, string RequestNo, byte Status);
