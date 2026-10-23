// code/ch20/Exercise2-Bug/Program.cs
// 练习 2 的原题：运行后程序崩溃。只看报错和堆栈，找到出问题的那一行自己的代码，改好它
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

var repo = new RequestRepository(connStr);
var latest = repo.GetLatestByApplicant("张伟");
Console.WriteLine($"张伟最近的申请单：{latest.RequestNo}（{latest.CreatedAt:yyyy-MM-dd}）");

public class RequestRepository
{
    private readonly string _connStr;

    public RequestRepository(string connStr) => _connStr = connStr;

    // 需求：取某个申请人最近的一张申请单
    public RequestBrief GetLatestByApplicant(string applicant)
    {
        using var conn = new SqlConnection(_connStr);
        return conn.QuerySingle<RequestBrief>(@"
            SELECT Id, RequestNo, CreatedAt
            FROM dbo.PriceRequest
            WHERE Applicant = @Applicant
            ORDER BY CreatedAt DESC", new { Applicant = applicant });
    }
}

public class RequestBrief
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
