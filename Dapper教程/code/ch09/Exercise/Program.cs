// code/ch09/Exercise/Program.cs
// 第 9 章练习参考答案：练习 1（用事务提交审批）+ 练习 2（找出忘传 transaction 的那一行）
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 把 3 号单恢复成草稿、删掉它的审批记录，保证可以反复运行
conn.Execute(@"
    UPDATE dbo.PriceRequest SET Status = 0 WHERE Id = 3;
    DELETE FROM dbo.ApprovalLog WHERE RequestId = 3;");
conn.Open();

// ===== 练习 1 =====
Submit(conn, "第 1 次（审批人为空）", null);
Submit(conn, "第 2 次（审批人张伟）", "张伟");

// ===== 练习 2 =====
// 原题错在第一句查询没传 tran：事务进行中，这个连接上的读操作也必须带上事务
using (var tran = conn.BeginTransaction())
{
    byte status = conn.ExecuteScalar<byte>(
        "SELECT Status FROM dbo.PriceRequest WHERE Id = @Id", new { Id = 3 }, tran);

    if (status <= 1)
    {
        conn.Execute("UPDATE dbo.PriceRequestItem SET RequestPrice = @Price WHERE Id = @ItemId",
            new { Price = 24000m, ItemId = 4 }, tran);
        conn.Execute("UPDATE dbo.PriceRequest SET Remark = @Remark WHERE Id = @Id",
            new { Remark = "探头改价", Id = 3 }, tran);
    }
    tran.Commit();
}

var price = conn.ExecuteScalar<decimal>("SELECT RequestPrice FROM dbo.PriceRequestItem WHERE Id = 4");
var remark = conn.ExecuteScalar<string>("SELECT Remark FROM dbo.PriceRequest WHERE Id = 3");
Console.WriteLine($"改价完成：明细 4 申请价 = {price:N2}，备注 = {remark}");

// 提交审批：改状态 + 写审批记录，两件事要么都成功，要么都不做
static void Submit(SqlConnection conn, string title, string? approver)
{
    using (var tran = conn.BeginTransaction())
    {
        try
        {
            conn.Execute("UPDATE dbo.PriceRequest SET Status = 1 WHERE Id = 3 AND Status = 0",
                transaction: tran);
            conn.Execute(@"
                INSERT dbo.ApprovalLog (RequestId, Approver, Action)
                VALUES (3, @Approver, N'提交')",
                new { Approver = approver }, transaction: tran);   // approver 为 null → 违反 NOT NULL
            tran.Commit();
            Console.Write($"{title}：已提交");
        }
        catch (SqlException ex)
        {
            tran.Rollback();
            Console.Write($"{title}：出错 {ex.Number}，已回滚");
        }
    }

    // 事务已结束，再查结果不用传 tran
    byte status = conn.ExecuteScalar<byte>("SELECT Status FROM dbo.PriceRequest WHERE Id = 3");
    int logs = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.ApprovalLog WHERE RequestId = 3");
    Console.WriteLine($" → 状态 {status}，审批记录 {logs} 条");
}
