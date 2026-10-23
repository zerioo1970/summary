// code/ch12/Step2-ThreeGrids/Program.cs
// 单据详情页：一次取回"申请单 + 明细 + 审批记录"三个结果集，按 SQL 里的顺序逐个 Read
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 三条 SELECT 共用同一个参数 @Id；结果集的顺序 = SELECT 在文本里出现的顺序
const string sql = @"
SELECT r.Id, r.RequestNo, r.Applicant, r.Status, c.Name AS CustomerName
FROM dbo.PriceRequest r
JOIN dbo.Customer c ON c.Id = r.CustomerId
WHERE r.Id = @Id;                                   -- 第 1 个：表头（0 或 1 行）

SELECT i.Id, p.Name AS ProductName, i.Qty, i.RequestPrice
FROM dbo.PriceRequestItem i
JOIN dbo.Product p ON p.Id = i.ProductId
WHERE i.RequestId = @Id
ORDER BY i.Id;                                      -- 第 2 个：明细（多行）

SELECT Approver, Action, Comment, ActedAt
FROM dbo.ApprovalLog
WHERE RequestId = @Id
ORDER BY ActedAt;                                   -- 第 3 个：审批记录（可能 0 行）";

ShowRequest(1);   // 有明细、有 2 条审批记录
ShowRequest(3);   // 有明细、没有审批记录（草稿）
ShowRequest(99);  // 不存在的单据

void ShowRequest(int id)
{
    using var grid = conn.QueryMultiple(sql, new { Id = id });

    // 读的顺序必须和 SQL 里 SELECT 的顺序一致：表头 → 明细 → 审批
    RequestHeader? header = grid.ReadSingleOrDefault<RequestHeader>();   // 0 行返回 null，2 行以上报错
    List<ItemLine> items = grid.Read<ItemLine>().ToList();
    List<LogLine> logs = grid.Read<LogLine>().ToList();

    Console.WriteLine($"===== 单据 Id={id} =====");
    if (header is null)
    {
        Console.WriteLine("  查无此单");   // 后两个结果集也是空的，照样读掉，没有坏处
        return;
    }
    Console.WriteLine($"  {header.RequestNo}  {header.CustomerName}  申请人={header.Applicant}  状态={header.Status}");
    foreach (var i in items)
        Console.WriteLine($"  明细 {i.Id}：{i.ProductName} × {i.Qty} @ {i.RequestPrice:N2}");
    Console.WriteLine($"  审批记录 {logs.Count} 条");
    foreach (var l in logs)
        Console.WriteLine($"    {l.ActedAt:MM-dd HH:mm}  {l.Approver}  {l.Action}  {l.Comment ?? "（无意见）"}");
}

public class RequestHeader
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
    public int Status { get; set; }
    public string CustomerName { get; set; } = "";   // 来自 JOIN 的客户名，用别名对上
}

public class ItemLine
{
    public int Id { get; set; }
    public string ProductName { get; set; } = "";
    public int Qty { get; set; }
    public decimal RequestPrice { get; set; }
}

public class LogLine
{
    public string Approver { get; set; } = "";
    public string Action { get; set; } = "";
    public string? Comment { get; set; }             // 列允许 NULL，用 string?（第 7 章讲过）
    public DateTime ActedAt { get; set; }
}
