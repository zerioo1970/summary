// code/ch23/PriceManager/PriceRequestRepository.cs
// 申请单的全部 SQL 都在这里。菜单只调方法，不碰 SQL、不碰连接
using System.Data;
using System.Text;
using Dapper;

namespace PriceManager;

// partial：允许把这个类的方法分写在几个文件里。第 23 章练习会在另一个文件里给它加方法
public partial class PriceRequestRepository
{
    private readonly IDbConnectionFactory _factory;

    public PriceRequestRepository(IDbConnectionFactory factory) => _factory = factory;

    // 菜单 1：分页列表。申请人可选（第 13 章动态条件），总数和当前页一次取回（第 12 章）
    public PageResult<RequestListItem> GetPage(string? applicant, int pageIndex, int pageSize)
    {
        var where = new StringBuilder(" WHERE 1 = 1");
        var p = new DynamicParameters();
        if (!string.IsNullOrWhiteSpace(applicant))
        {
            where.Append(" AND r.Applicant = @Applicant");     // 只拼固定片段，值走参数
            p.Add("Applicant", applicant);
        }
        p.Add("Skip", (pageIndex - 1) * pageSize);
        p.Add("Take", pageSize);

        string sql = $@"
            SELECT COUNT(*) FROM dbo.PriceRequest r{where};

            SELECT r.Id, r.RequestNo, c.Name AS CustomerName, r.Applicant, r.Status, r.CreatedAt,
                   (SELECT SUM(i.Qty * i.RequestPrice)
                    FROM dbo.PriceRequestItem i WHERE i.RequestId = r.Id) AS Amount
            FROM dbo.PriceRequest r
            JOIN dbo.Customer c ON c.Id = r.CustomerId{where}
            ORDER BY r.Id
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;";

        using var conn = _factory.Create();
        using var grid = conn.QueryMultiple(sql, p);
        return new PageResult<RequestListItem>
        {
            Total = grid.ReadSingle<int>(),                    // 读的顺序必须和 SQL 里的顺序一致
            Items = grid.Read<RequestListItem>().ToList(),
        };
    }

    // 菜单 2：单据详情 = 主表 + 明细 + 审批记录，三个结果集一次往返（第 12 章）
    public RequestDetail? GetDetail(int id)
    {
        using var conn = _factory.Create();
        using var grid = conn.QueryMultiple(@"
            SELECT r.Id, r.RequestNo, c.Name AS CustomerName, r.Applicant, r.Status, r.CreatedAt, r.Remark
            FROM dbo.PriceRequest r
            JOIN dbo.Customer c ON c.Id = r.CustomerId
            WHERE r.Id = @Id;

            SELECT p.Code AS ProductCode, p.Name AS ProductName, i.Qty, i.RequestPrice
            FROM dbo.PriceRequestItem i
            JOIN dbo.Product p ON p.Id = i.ProductId
            WHERE i.RequestId = @Id
            ORDER BY i.Id;

            SELECT Approver, Action, Comment, ActedAt
            FROM dbo.ApprovalLog
            WHERE RequestId = @Id
            ORDER BY ActedAt, Id;", new { Id = id });

        var head = grid.ReadSingleOrDefault<RequestDetail>();  // 0 行 → null（第 5 章）
        if (head is null) return null;
        head.Items = grid.Read<RequestItemView>().ToList();
        head.Logs = grid.Read<ApprovalLogView>().ToList();
        return head;
    }

    // 菜单 3：新建申请单。主表 + 明细在一个事务里（第 9 章），OUTPUT 取新 Id（第 6 章）
    public int Create(NewRequest r)
    {
        using var conn = _factory.Create();
        conn.Open();                                           // 开事务前必须先打开连接
        using var tx = conn.BeginTransaction();                // 没 Commit 就离开 using → 自动回滚

        int id = conn.QuerySingle<int>(@"
            INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant, Remark)
            OUTPUT INSERTED.Id
            VALUES (@RequestNo, @CustomerId, @Applicant, @Remark)",
            new { r.RequestNo, r.CustomerId, r.Applicant, r.Remark }, tx);

        conn.Execute(@"
            INSERT dbo.PriceRequestItem (RequestId, ProductId, Qty, RequestPrice)
            VALUES (@RequestId, @ProductId, @Qty, @RequestPrice)",
            r.Items.Select(i => new { RequestId = id, i.ProductId, i.Qty, i.RequestPrice }), tx);

        tx.Commit();
        return id;
    }

    // 菜单 4：提交审批。调存储过程，读 RETURN 值和 OUTPUT 参数（第 8 章）
    public SubmitResult Submit(string requestNo, string operatorName)
    {
        var p = new DynamicParameters();
        p.Add("RequestNo", requestNo, DbType.AnsiString, size: 20);   // 过程参数是 VARCHAR(20)
        p.Add("Operator", operatorName);
        p.Add("NewStatus", dbType: DbType.Byte, direction: ParameterDirection.Output);
        p.Add("Ret", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

        using var conn = _factory.Create();
        conn.Execute("dbo.Update_PriceRequestSubmit_Proc", p, commandType: CommandType.StoredProcedure);

        // Output / ReturnValue 要在执行之后才能取
        return new SubmitResult
        {
            Success = p.Get<int>("Ret") == 0,
            CurrentStatus = (RequestStatus?)p.Get<byte?>("NewStatus"),
        };
    }

    // 菜单 5：按单号查一张。单号有唯一约束，最多一行（第 5 章）
    public RequestListItem? GetByNo(string requestNo)
    {
        using var conn = _factory.Create();
        return conn.QuerySingleOrDefault<RequestListItem>(@"
            SELECT r.Id, r.RequestNo, c.Name AS CustomerName, r.Applicant, r.Status, r.CreatedAt,
                   (SELECT SUM(i.Qty * i.RequestPrice)
                    FROM dbo.PriceRequestItem i WHERE i.RequestId = r.Id) AS Amount
            FROM dbo.PriceRequest r
            JOIN dbo.Customer c ON c.Id = r.CustomerId
            WHERE r.RequestNo = @RequestNo",
            new { RequestNo = new DbString { Value = requestNo, IsAnsi = true, Length = 20 } }); // 第 15 章
    }
}
