// code/ch09/Step2-WithTransaction/Program.cs
// 同样的"主表 + 2 条明细"放进一个事务：第 2 条失败 → 回滚 → 库里什么都不留
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 清掉上次运行留下的单据（第 1 步的例子可能留下了残留）
conn.Execute(@"
    DELETE i FROM dbo.PriceRequestItem i
    JOIN dbo.PriceRequest r ON r.Id = i.RequestId
    WHERE r.RequestNo = 'PR20261020';
    DELETE FROM dbo.PriceRequest WHERE RequestNo = 'PR20261020';");

// ① 事务要求连接先打开。BeginTransaction 是 ADO.NET 的方法，Dapper 的"自动打开"管不到它
conn.Open();

// ② 开始事务，拿到事务对象 tran
using var tran = conn.BeginTransaction();
try
{
    // ③ 每一条命令都把 tran 传进去：transaction: tran
    int newId = conn.ExecuteScalar<int>(@"
        INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant)
        OUTPUT INSERTED.Id
        VALUES (@RequestNo, @CustomerId, @Applicant)",
        new { RequestNo = "PR20261020", CustomerId = 2, Applicant = "李娜" },
        transaction: tran);
    Console.WriteLine($"第 1 步 主表已插入（未提交），新 Id = {newId}");

    var items = new[]
    {
        new { RequestId = newId, ProductId = 1,  Qty = 1, RequestPrice = 170000m },
        new { RequestId = newId, ProductId = 99, Qty = 2, RequestPrice = 25000m },  // 99 不存在 → 外键报错
    };
    conn.Execute(@"
        INSERT dbo.PriceRequestItem (RequestId, ProductId, Qty, RequestPrice)
        VALUES (@RequestId, @ProductId, @Qty, @RequestPrice)",
        items, transaction: tran);

    // ④ 全部成功才提交
    tran.Commit();
    Console.WriteLine("已提交");
}
catch (SqlException ex)
{
    // ⑤ 任何一步失败：回滚，前面已经执行的 INSERT 全部撤销
    tran.Rollback();
    Console.WriteLine($"出错了，错误号 {ex.Number}，已回滚");
}

// 事务已经结束（回滚了），后面的查询不用再传 tran
int heads = conn.ExecuteScalar<int>(
    "SELECT COUNT(*) FROM dbo.PriceRequest WHERE RequestNo = 'PR20261020'");
int lines = conn.ExecuteScalar<int>(@"
    SELECT COUNT(*) FROM dbo.PriceRequestItem i
    JOIN dbo.PriceRequest r ON r.Id = i.RequestId
    WHERE r.RequestNo = 'PR20261020'");
Console.WriteLine($"残留：主表 {heads} 行，明细 {lines} 行");

// 自增值不会随回滚退回去：再插一张看看新 Id 是多少
int nextId = conn.ExecuteScalar<int>(@"
    INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant)
    OUTPUT INSERTED.Id
    VALUES ('PR20261020', 2, N'李娜')");
Console.WriteLine($"回滚后再插一张，新 Id = {nextId}");
conn.Execute("DELETE FROM dbo.PriceRequest WHERE Id = @Id", new { Id = nextId });   // 看完就删，不留垃圾
