// code/ch10/Step2-SplitOn/Program.cs
// splitOn 的两个实验：第二个对象的第一列不叫 Id 时要写 splitOn；列顺序写错会静默错位
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 实验 1：列表页不需要客户 Id，客户部分从 c.Name 开始 → 告诉 Dapper 在 Name 列处切开
var list1 = conn.Query<PriceRequest, Customer, PriceRequest>(@"
    SELECT r.Id, r.RequestNo, r.Applicant, r.Status,
           c.Name, c.City
    FROM dbo.PriceRequest r
    JOIN dbo.Customer c ON c.Id = r.CustomerId
    ORDER BY r.Id",
    (request, customer) => { request.Customer = customer; return request; },
    splitOn: "Name");                  // 切分列 = 第二个对象的第一列

Console.WriteLine("实验 1 splitOn: \"Name\"");
foreach (var r in list1)
    Console.WriteLine($"  {r.RequestNo}  状态={r.Status}  客户={r.Customer!.Name}（{r.Customer.City}）");

// 实验 2：有人把 r.Status 加到了 SELECT 的最后——它落在切分点右边，被分给了客户
var list2 = conn.Query<PriceRequest, Customer, PriceRequest>(@"
    SELECT r.Id, r.RequestNo, r.Applicant,
           c.Id, c.Name, c.City,
           r.Status
    FROM dbo.PriceRequest r
    JOIN dbo.Customer c ON c.Id = r.CustomerId
    ORDER BY r.Id",
    (request, customer) => { request.Customer = customer; return request; });

Console.WriteLine("实验 2 r.Status 放到了最后");
foreach (var r in list2)
    Console.WriteLine($"  {r.RequestNo}  状态={r.Status}  客户={r.Customer!.Name}（{r.Customer.City}）");

public class PriceRequest
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
    public int Status { get; set; }
    public Customer? Customer { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
}
