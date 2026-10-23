// code/ch10/Exercise/Program.cs
// 第 10 章练习参考答案：练习 1（LEFT JOIN 审批"通过"记录，没有时为 null）+ 练习 2（修好城市为空）
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// ===== 练习 1 =====
// LEFT JOIN：没有"通过"记录的单据，a.Id 是 NULL → Dapper 给第二个对象传 null
var list = conn.Query<PriceRequest, ApprovalLog, PriceRequest>(@"
    SELECT r.Id, r.RequestNo,
           a.Id, a.Approver, a.Comment
    FROM dbo.PriceRequest r
    LEFT JOIN dbo.ApprovalLog a ON a.RequestId = r.Id AND a.Action = N'通过'
    ORDER BY r.Id",
    (request, approval) => { request.Approval = approval; return request; });

foreach (var r in list)
{
    string text = r.Approval is null
        ? "（还没有通过记录）"                                   // 必须先判断 null
        : $"{r.Approval.Approver}（{r.Approval.Comment}）";
    Console.WriteLine($"{r.RequestNo}  通过人：{text}");
}

// ===== 练习 2 =====
// 原题把 c.City 写在了 c.Id 前面：切分点在 c.Id，City 落到了申请单那一段（申请单没有 City 属性，被丢掉）
// 修法：客户的列从切分列 c.Id 开始，全部写在它后面
var requests = conn.Query<PriceRequest, Customer, PriceRequest>(@"
    SELECT r.Id, r.RequestNo, c.Id, c.Name, c.City
    FROM dbo.PriceRequest r
    JOIN dbo.Customer c ON c.Id = r.CustomerId
    ORDER BY r.Id",
    (request, customer) => { request.Customer = customer; return request; });

foreach (var r in requests)
    Console.WriteLine($"{r.RequestNo}  {r.Customer!.Name}  [{r.Customer.City}]");

public class PriceRequest
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public Customer? Customer { get; set; }
    public ApprovalLog? Approval { get; set; }   // 可能没有，所以是可空类型
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
}

public class ApprovalLog
{
    public int Id { get; set; }
    public string Approver { get; set; } = "";
    public string? Comment { get; set; }
}
