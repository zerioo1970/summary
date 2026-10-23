// code/ch11/Exercise/Program.cs
// 第 11 章练习参考答案：客户 + 他的申请单。练习 1 用 JOIN + 字典，练习 2 用两次查询
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// ===== 练习 1：LEFT JOIN + 字典去重 =====
var byId = new Dictionary<int, Customer>();
var customers1 = new List<Customer>();

conn.Query<Customer, PriceRequest, Customer>(@"
    SELECT c.Id, c.Name, c.City,
           r.Id, r.RequestNo
    FROM dbo.Customer c
    LEFT JOIN dbo.PriceRequest r ON r.CustomerId = c.Id
    ORDER BY c.Id, r.Id",
    (customer, request) =>
    {
        if (!byId.TryGetValue(customer.Id, out var head))
        {
            head = customer;
            byId.Add(head.Id, head);
            customers1.Add(head);
        }
        if (request is not null)          // 北方影像诊所没有申请单 → request 是 null
            head.Requests.Add(request);
        return head;
    });

Console.WriteLine("练习 1");
Print(customers1);

// ===== 练习 2：先查客户，再按 CustomerId IN @ids 查申请单 =====
var customers2 = conn.Query<Customer>(
    "SELECT Id, Name, City FROM dbo.Customer ORDER BY Id").ToList();

var ids = customers2.Select(c => c.Id).ToList();
var requestsByCustomer = conn.Query<PriceRequest>(@"
    SELECT Id, CustomerId, RequestNo
    FROM dbo.PriceRequest
    WHERE CustomerId IN @ids
    ORDER BY Id", new { ids }).ToLookup(r => r.CustomerId);

foreach (var c in customers2)
    c.Requests = requestsByCustomer[c.Id].ToList();   // 没有申请单的客户得到空列表

Console.WriteLine("练习 2");
Print(customers2);

static void Print(List<Customer> list)
{
    foreach (var c in list)
        Console.WriteLine($"  {c.Name}（{c.City}）：{c.Requests.Count} 张 " +
                          string.Join("、", c.Requests.Select(r => r.RequestNo)));
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
    public List<PriceRequest> Requests { get; set; } = new();
}

public class PriceRequest
{
    public int Id { get; set; }
    public int CustomerId { get; set; }   // 练习 2 分组要用；练习 1 没查这一列，保持 0 也不影响
    public string RequestNo { get; set; } = "";
}
