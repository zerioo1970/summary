// code/ch09/Step4-CommitRollback/Program.cs
// 提交、回滚与 using：没 Commit 就离开 using 会自动回滚；把 Commit 放在最后一行就是最简单的安全写法
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 清掉上次运行留下的单据，保证可以反复运行
conn.Execute(@"
    DELETE i FROM dbo.PriceRequestItem i
    JOIN dbo.PriceRequest r ON r.Id = i.RequestId
    WHERE r.RequestNo IN ('PR20261021', 'PR20261022');
    DELETE FROM dbo.PriceRequest WHERE RequestNo IN ('PR20261021', 'PR20261022');");

conn.Open();

// 场景 A：改了数据，但没调 Commit 就离开了 using
using (var tran = conn.BeginTransaction())
{
    conn.Execute("UPDATE dbo.PriceRequest SET Remark = N'场景A改的' WHERE Id = 3",
        transaction: tran);
    Console.WriteLine("A 事务里读到：" + ReadRemark(conn, tran));
    // 故意不写 tran.Commit()
}   // ← 走到这里，using 调用 tran.Dispose()，未提交的事务被回滚
Console.WriteLine("A 离开 using 后：" + ReadRemark(conn, null));

// 场景 B：正常走完 → 提交
CreateRequest(conn, "PR20261021", productId: 3);
Console.WriteLine("B 正常提交");

// 场景 C：中途出错 → 跳过 Commit → using 结束时自动回滚
try
{
    CreateRequest(conn, "PR20261022", productId: 99);   // 产品 99 不存在
}
catch (SqlException ex)
{
    Console.WriteLine($"C 出错（错误号 {ex.Number}），没走到 Commit");
}

var saved = conn.Query<string>(
    "SELECT RequestNo FROM dbo.PriceRequest WHERE RequestNo IN ('PR20261021', 'PR20261022') ORDER BY RequestNo");
Console.WriteLine("库里的新单据：" + string.Join("、", saved));

// 读 3 号单的备注。tran 为 null 时就是普通查询
static string ReadRemark(SqlConnection conn, SqlTransaction? tran) =>
    conn.ExecuteScalar<string>("SELECT Remark FROM dbo.PriceRequest WHERE Id = 3",
        transaction: tran)!;

// 新建"主表 + 1 条明细"。推荐的最简写法：using 开事务，最后一行 Commit
static void CreateRequest(SqlConnection conn, string requestNo, int productId)
{
    using var tran = conn.BeginTransaction();

    int id = conn.ExecuteScalar<int>(@"
        INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant)
        OUTPUT INSERTED.Id
        VALUES (@RequestNo, 1, N'张伟')",
        new { RequestNo = requestNo }, transaction: tran);

    conn.Execute(@"
        INSERT dbo.PriceRequestItem (RequestId, ProductId, Qty, RequestPrice)
        VALUES (@RequestId, @ProductId, 1, 25000)",
        new { RequestId = id, ProductId = productId }, transaction: tran);

    tran.Commit();   // 只有上面全部成功才会走到这里；抛异常就跳过它，离开方法时 using 自动回滚
}
