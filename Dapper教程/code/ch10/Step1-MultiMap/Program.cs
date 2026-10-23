// code/ch10/Step1-MultiMap/Program.cs
// 多重映射：一条 JOIN 查询，每一行拆成"申请单 + 客户"两个对象，再由你把客户挂到申请单上
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 前 4 列属于申请单，从 c.Id 开始的 3 列属于客户
const string sql = @"
    SELECT r.Id, r.RequestNo, r.Applicant, r.Status,
           c.Id, c.Name, c.City
    FROM dbo.PriceRequest r
    JOIN dbo.Customer c ON c.Id = r.CustomerId
    ORDER BY r.Id";

// 三个类型参数：<第一个对象, 第二个对象, 最终返回的类型>
var requests = conn.Query<PriceRequest, Customer, PriceRequest>(
    sql,
    (request, customer) =>          // Dapper 每读一行调用一次这个函数
    {
        request.Customer = customer;   // 怎么"挂"上去由你决定，Dapper 不会替你做
        return request;                // 返回值就是结果集合里的一个元素
    }).ToList();                       // 没写 splitOn：默认在名叫 Id 的列处切开

foreach (var r in requests)
    Console.WriteLine($"{r.RequestNo}  {r.Applicant}  状态={r.Status}  " +
                      $"客户={r.Customer!.Name}（{r.Customer.City}，Id={r.Customer.Id}）");

// 1 号和 3 号单的客户都是"华东第一医院"，它们拿到的是同一个 Customer 对象吗？
Console.WriteLine($"1 号和 3 号单的 Customer 是同一个对象吗？" +
                  $"{ReferenceEquals(requests[0].Customer, requests[2].Customer)}");

public class PriceRequest
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
    public int Status { get; set; }
    public Customer? Customer { get; set; }   // 一对一：每张申请单属于一个客户。不在表里，靠映射函数赋值
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
}
