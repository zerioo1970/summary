// code/ch06/Step2-Identity/Program.cs
// 插入后拿到数据库生成的自增 Id：OUTPUT INSERTED、SCOPE_IDENTITY、一次拿多列、一次拿多行
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 写法 A：OUTPUT INSERTED.Id —— INSERT 语句自己"吐出"一个单值结果集，用 ExecuteScalar<int> 接
int idA = conn.ExecuteScalar<int>(@"
    INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant)
    OUTPUT INSERTED.Id
    VALUES (@RequestNo, @CustomerId, @Applicant)",
    new { RequestNo = "PR20261021", CustomerId = 1, Applicant = "张伟" });
Console.WriteLine($"写法 A  OUTPUT + ExecuteScalar<int>：新 Id = {idA}");

// 写法 B：同一条 SQL，用 QuerySingle<int> 接（第 5 章：必须正好 1 行，否则抛异常）
int idB = conn.QuerySingle<int>(@"
    INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant)
    OUTPUT INSERTED.Id
    VALUES (@RequestNo, @CustomerId, @Applicant)",
    new { RequestNo = "PR20261022", CustomerId = 2, Applicant = "李娜" });
Console.WriteLine($"写法 B  OUTPUT + QuerySingle<int>：新 Id = {idB}");

// 写法 C：老办法 SCOPE_IDENTITY()。两条语句放在同一次调用里，它返回 decimal，所以 CAST 成 int
int idC = conn.ExecuteScalar<int>(@"
    INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant)
    VALUES (@RequestNo, @CustomerId, @Applicant);
    SELECT CAST(SCOPE_IDENTITY() AS int);",
    new { RequestNo = "PR20261023", CustomerId = 3, Applicant = "王芳" });
Console.WriteLine($"写法 C  SCOPE_IDENTITY()：新 Id = {idC}");

// 写法 D：OUTPUT 多列 —— 连同数据库填的默认值（Status、CreatedAt）一起带回来，用对象接
NewRequest row = conn.QuerySingle<NewRequest>(@"
    INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant)
    OUTPUT INSERTED.Id, INSERTED.RequestNo, INSERTED.Status, INSERTED.CreatedAt
    VALUES (@RequestNo, @CustomerId, @Applicant)",
    new { RequestNo = "PR20261024", CustomerId = 1, Applicant = "张伟" });
Console.WriteLine($"写法 D  OUTPUT 多列：Id = {row.Id}，单号 = {row.RequestNo}，" +
                  $"状态 = {row.Status}，创建时间 = {row.CreatedAt:yyyy-MM-dd HH:mm:ss}");

// 写法 E：一条 INSERT 插多行，OUTPUT 每插一行吐一行 —— 用 Query<int> 拿到全部新 Id
List<int> itemIds = conn.Query<int>(@"
    INSERT dbo.PriceRequestItem (RequestId, ProductId, Qty, RequestPrice)
    OUTPUT INSERTED.Id
    VALUES (@RequestId, 3, 2, 24000), (@RequestId, 4, 2, 22000)",
    new { RequestId = row.Id }).ToList();
Console.WriteLine($"写法 E  一次插 2 条明细：新 Id = {string.Join("、", itemIds)}");

public class NewRequest
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public byte Status { get; set; }           // TINYINT 对应 byte
    public DateTime CreatedAt { get; set; }
}
