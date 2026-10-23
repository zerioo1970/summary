// code/ch09/Step1-NoTransaction/Program.cs
// 不用事务新建"主表 + 2 条明细"：第 2 条明细失败后，主表和第 1 条明细留在了库里
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 先清掉上次运行留下的单据，保证这个例子可以反复运行（先删明细再删主表，外键要求这个顺序）
conn.Execute(@"
    DELETE i FROM dbo.PriceRequestItem i
    JOIN dbo.PriceRequest r ON r.Id = i.RequestId
    WHERE r.RequestNo = 'PR20261020';
    DELETE FROM dbo.PriceRequest WHERE RequestNo = 'PR20261020';");

try
{
    // 第 1 步：插主表。OUTPUT INSERTED.Id 取回新单据的 Id（第 6 章讲过）
    int newId = conn.ExecuteScalar<int>(@"
        INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant)
        OUTPUT INSERTED.Id
        VALUES (@RequestNo, @CustomerId, @Applicant)",
        new { RequestNo = "PR20261020", CustomerId = 2, Applicant = "李娜" });
    Console.WriteLine($"第 1 步 主表已插入，新 Id = {newId}");

    // 第 2 步：插两条明细。传数组 → Dapper 逐条执行 INSERT（第 6 章讲过）
    // 第 2 条的 ProductId = 99，产品表里没有这个产品 → 违反外键
    var items = new[]
    {
        new { RequestId = newId, ProductId = 1,  Qty = 1, RequestPrice = 170000m },
        new { RequestId = newId, ProductId = 99, Qty = 2, RequestPrice = 25000m },
    };
    conn.Execute(@"
        INSERT dbo.PriceRequestItem (RequestId, ProductId, Qty, RequestPrice)
        VALUES (@RequestId, @ProductId, @Qty, @RequestPrice)", items);
    Console.WriteLine("第 2 步 明细已插入");      // 走不到这一行
}
catch (SqlException ex)
{
    Console.WriteLine($"出错了，错误号 {ex.Number}：");
    Console.WriteLine(ex.Message);
}

// 第 3 步：看看库里留下了什么
int heads = conn.ExecuteScalar<int>(
    "SELECT COUNT(*) FROM dbo.PriceRequest WHERE RequestNo = 'PR20261020'");
int lines = conn.ExecuteScalar<int>(@"
    SELECT COUNT(*) FROM dbo.PriceRequestItem i
    JOIN dbo.PriceRequest r ON r.Id = i.RequestId
    WHERE r.RequestNo = 'PR20261020'");
Console.WriteLine($"残留：主表 {heads} 行，明细 {lines} 行");
