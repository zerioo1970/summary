// code/ch06/Exercise/Program.cs
// 第 6 章练习参考答案：新建申请单拿到 Id → 用 List 写 2 条明细 → 提交两次看受影响行数
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 第 1 步：新建主表，OUTPUT INSERTED.Id 把新 Id 带回来
int newId = conn.ExecuteScalar<int>(@"
    INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant, Remark)
    OUTPUT INSERTED.Id
    VALUES (@RequestNo, @CustomerId, @Applicant, @Remark)",
    new { RequestNo = "PR20261101", CustomerId = 2, Applicant = "李娜", Remark = "探头补货" });
Console.WriteLine($"新单 Id：{newId}");

// 第 2 步：明细的 RequestId 用刚拿到的新 Id；传 List，Dapper 逐条执行
var items = new List<NewItem>
{
    new NewItem { RequestId = newId, ProductId = 3, Qty = 5, RequestPrice = 23500m },
    new NewItem { RequestId = newId, ProductId = 4, Qty = 5, RequestPrice = 21500m },
};
int itemRows = conn.Execute(@"
    INSERT dbo.PriceRequestItem (RequestId, ProductId, Qty, RequestPrice)
    VALUES (@RequestId, @ProductId, @Qty, @RequestPrice)", items);
Console.WriteLine($"明细写入：{itemRows} 行");

// 第 3 步：提交两次。条件里带 Status = 0，第 2 次就命中不了
for (int i = 1; i <= 2; i++)
{
    int rows = conn.Execute(
        "UPDATE dbo.PriceRequest SET Status = 1 WHERE Id = @Id AND Status = 0",
        new { Id = newId });
    string result = rows == 1 ? "提交成功" : "没命中：单号不存在或已不是草稿";
    Console.WriteLine($"第 {i} 次提交：{rows} 行 → {result}");
}

public class NewItem
{
    public int RequestId { get; set; }
    public int ProductId { get; set; }
    public int Qty { get; set; }
    public decimal RequestPrice { get; set; }
}
