// code/ch20/Step3-DapperErrors/Program.cs
// Dapper 自己抛的异常：SQL 已经执行成功，是"结果装进对象"或"对象变成参数"这一步出了问题
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// ① 类型转不过去：Name 是字符串，属性是 int
Try("① 列转属性", () => conn.Query<BadCustomer>("SELECT Id, Name FROM dbo.Customer").ToList());

// ② 多重映射（第 10 章）：第二个对象的首列不叫 Id，又没写 splitOn
Try("② splitOn", () => conn.Query<Request, Customer, Request>(@"
    SELECT r.Id, r.RequestNo, c.Name, c.City
    FROM dbo.PriceRequest r JOIN dbo.Customer c ON c.Id = r.CustomerId",
    (r, c) => { r.Customer = c; return r; }).ToList());

// ③ QuerySingle 查到 0 行（第 5 章）
Try("③ 0 行", () => conn.QuerySingle<string>("SELECT RequestNo FROM dbo.PriceRequest WHERE Id = 99"));

// ④ QuerySingle 查到多行
Try("④ 多行", () => conn.QuerySingle<string>("SELECT RequestNo FROM dbo.PriceRequest WHERE Applicant = N'张伟'"));

// ⑤ 参数对象里有一个 Dapper 不认识的类型（把整个 Customer 对象当成了参数值）
Try("⑤ 参数类型", () => conn.Query<int>("SELECT Id FROM dbo.PriceRequest WHERE CustomerId = @Customer",
    new { Customer = new Customer { Name = "华东第一医院" } }).ToList());

// 打印异常类型和消息；有 InnerException（内层异常）就再打印一层——真正的原因常常在里面
static void Try(string title, Func<object> action)
{
    try
    {
        action();
        Console.WriteLine($"{title}：没有报错");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{title}：{ex.GetType().FullName}");
        Console.WriteLine($"    {ex.Message}");
        if (ex.InnerException is not null)
            Console.WriteLine($"    内层 {ex.InnerException.GetType().FullName}：{ex.InnerException.Message}");
    }
}

public class BadCustomer
{
    public int Id { get; set; }
    public int Name { get; set; }          // 故意写错类型：应该是 string
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
}

public class Request
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public Customer? Customer { get; set; }
}
