// code/ch23/Exercise/PriceRequestRepository.Exercise.cs
// 第 23 章练习参考答案：给仓储加两个方法（审批、按客户汇总）。partial 让它们和原文件合成同一个类
using System.Data;
using Dapper;

namespace PriceManager;

public partial class PriceRequestRepository
{
    // 练习 1：审批。只有"待审"能审批；改状态 + 写审批记录在同一个事务里
    public bool Approve(string requestNo, string approver, bool pass, string? comment)
    {
        var p = new
        {
            RequestNo = requestNo,
            Approver = approver,
            Comment = comment,
            NewStatus = pass ? RequestStatus.Approved : RequestStatus.Rejected,  // 枚举按底层的 byte 值传
            Pending = RequestStatus.Pending,
            Action = pass ? "通过" : "驳回",
        };

        using var conn = _factory.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();

        // 条件里带上 Status = 待审：受影响 0 行，就是单号不存在或者状态不对（第 6 章）
        int rows = conn.Execute(@"
            UPDATE dbo.PriceRequest SET Status = @NewStatus
            WHERE RequestNo = @RequestNo AND Status = @Pending", p, tx);
        if (rows == 0) return false;                   // 不 Commit，离开 using 自动回滚

        conn.Execute(@"
            INSERT dbo.ApprovalLog (RequestId, Approver, Action, Comment)
            SELECT Id, @Approver, @Action, @Comment FROM dbo.PriceRequest WHERE RequestNo = @RequestNo",
            p, tx);

        tx.Commit();
        return true;
    }

    // 练习 2：按客户汇总。用子查询而不是 JOIN 后 COUNT，避免第 21 章场景 5 的重复计数
    public List<CustomerSummary> GetCustomerSummary()
    {
        using var conn = _factory.Create();
        return conn.Query<CustomerSummary>(@"
            SELECT c.Name AS CustomerName,
                   (SELECT COUNT(*) FROM dbo.PriceRequest r WHERE r.CustomerId = c.Id) AS RequestCount,
                   (SELECT SUM(i.Qty * i.RequestPrice)
                    FROM dbo.PriceRequestItem i
                    JOIN dbo.PriceRequest r ON r.Id = i.RequestId
                    WHERE r.CustomerId = c.Id) AS Amount
            FROM dbo.Customer c
            ORDER BY c.Id").ToList();
    }
}

public class CustomerSummary
{
    public string CustomerName { get; set; } = "";
    public int RequestCount { get; set; }
    public decimal? Amount { get; set; }               // 没有单据的客户，SUM 是 NULL
}
