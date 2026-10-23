// code/ch04/Step2-ObjectParam/Program.cs
// 传参的几种写法：ADO.NET 对照、匿名对象、大小写、实体对象、同一参数用两次
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

const string sql = @"
    SELECT Id, RequestNo, Applicant, Status
    FROM dbo.PriceRequest
    WHERE Applicant = @Applicant AND Status = @Status";

// 例 0：ADO.NET 写法——每个参数三行：名字和类型、长度、值
Console.WriteLine("== 例 0 ADO.NET");
conn.Open();
using (var cmd = new SqlCommand(sql, conn))
{
    cmd.Parameters.Add("@Applicant", SqlDbType.NVarChar, 20).Value = "张伟";
    cmd.Parameters.Add("@Status", SqlDbType.TinyInt).Value = 2;
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
        Console.WriteLine($"{reader.GetString(reader.GetOrdinal("RequestNo"))} {reader.GetString(reader.GetOrdinal("Applicant"))}");
}
conn.Close();

// 例 1：匿名对象——属性名就是参数名（不带 @），值的类型决定参数类型
Console.WriteLine("== 例 1 匿名对象");
foreach (var r in conn.Query<RequestBrief>(sql, new { Applicant = "张伟", Status = 2 }))
    Console.WriteLine($"{r.RequestNo} {r.Applicant}");

// 例 2：属性名和 @参数名大小写不同——照样对上
Console.WriteLine("== 例 2 大小写不同");
foreach (var r in conn.Query<RequestBrief>(sql, new { applicant = "张伟", STATUS = 2 }))
    Console.WriteLine($"{r.RequestNo} {r.Applicant}");

// 例 3：实体对象——把查询条件类直接传进去；类里多出来的属性 SQL 没用到，不会被发送
Console.WriteLine("== 例 3 实体对象");
var filter = new RequestFilter { Applicant = "张伟", Status = 0, PageSize = 20 };
foreach (var r in conn.Query<RequestBrief>(sql, filter))
    Console.WriteLine($"{r.RequestNo} {r.Applicant} 状态={r.Status}");

// 例 4：同一个参数在 SQL 里出现两次——对象里只写一次
Console.WriteLine("== 例 4 同一参数用两次");
var both = conn.Query<RequestBrief>(@"
    SELECT Id, RequestNo, Applicant, Status
    FROM dbo.PriceRequest
    WHERE Applicant = @Name OR RequestNo = @Name", new { Name = "李娜" });
foreach (var r in both)
    Console.WriteLine($"{r.RequestNo} {r.Applicant}");

public class RequestBrief
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
    public int Status { get; set; }
}

public class RequestFilter          // 查询页的条件，可能比某一条 SQL 用到的多
{
    public string Applicant { get; set; } = "";
    public int Status { get; set; }
    public int PageSize { get; set; }   // 这条 SQL 没有 @PageSize，Dapper 不会把它当参数发出去
}
