// code/ch20/Exercise/Program.cs
// 第 20 章练习参考答案：练习 1（按错误号给出友好提示）+ 练习 2（读堆栈修好 QuerySingle 多行的问题）
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

// 练习 1：三次新增，分别撞上唯一键、截断、正常
var products = new ProductRepository(connStr);
Console.WriteLine(products.Add("US-100", "便携超声主机", 180000m));
Console.WriteLine(products.Add("PB-03", new string('长', 51), 28000m));   // Name 是 NVARCHAR(50)，51 个字
Console.WriteLine(products.Add("PB-03", "微凸探头", 28000m));

// 练习 2：取张伟最近的一张申请单
var repo = new RequestRepository(connStr);
var latest = repo.GetLatestByApplicant("张伟");
Console.WriteLine($"张伟最近的申请单：{latest?.RequestNo}（{latest?.CreatedAt:yyyy-MM-dd}）");

public class ProductRepository
{
    private readonly string _connStr;

    public ProductRepository(string connStr) => _connStr = connStr;

    public string Add(string code, string name, decimal listPrice)
    {
        using var conn = new SqlConnection(_connStr);
        try
        {
            int id = conn.QuerySingle<int>(@"
                INSERT dbo.Product (Code, Name, ListPrice)
                OUTPUT INSERTED.Id
                VALUES (@Code, @Name, @ListPrice)",
                new { Code = code, Name = name, ListPrice = listPrice });
            return $"新增成功：{code}，Id = {id}";
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            return $"产品编码 {code} 已经存在";
        }
        catch (SqlException ex) when (ex.Number == 2628 || ex.Number == 8152)   // 8152 是 SQL Server 2017 及以前的截断错误号
        {
            return $"有字段超长：编码最多 20 个字符，名称最多 50 个字";
        }
        // 其他错误号不处理：没有 catch 到，就原样抛给调用方
    }
}

public class RequestRepository
{
    private readonly string _connStr;

    public RequestRepository(string connStr) => _connStr = connStr;

    // 原题用 QuerySingle：张伟有 2 张单，"最多一行"的约定被打破，抛 Sequence contains more than one element。
    // 改法：SQL 里 TOP (1) 只取最新一行；方法换成 QueryFirstOrDefault，查不到时返回 null
    public RequestBrief? GetLatestByApplicant(string applicant)
    {
        using var conn = new SqlConnection(_connStr);
        return conn.QueryFirstOrDefault<RequestBrief>(@"
            SELECT TOP (1) Id, RequestNo, CreatedAt
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
