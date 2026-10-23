// code/ch23/PriceManager/Models.cs
// 数据类：每个类对应一种"查询结果的形状"，而不是一张表
namespace PriceManager;

// Status 列是 TINYINT，枚举底层类型写 byte 才能直接映射（第 7 章）
public enum RequestStatus : byte
{
    Draft = 0,      // 草稿
    Pending = 1,    // 待审
    Approved = 2,   // 通过
    Rejected = 3,   // 驳回
}

public static class RequestStatusText
{
    // 界面上显示中文；数据库和代码里用枚举，避免到处写魔法数字 0/1/2/3
    public static string ToText(this RequestStatus s) => s switch
    {
        RequestStatus.Draft => "草稿",
        RequestStatus.Pending => "待审",
        RequestStatus.Approved => "通过",
        RequestStatus.Rejected => "驳回",
        _ => $"未知({(byte)s})",
    };
}

// 列表页的一行：主表 + 客户名称 + 金额合计
public class RequestListItem
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Applicant { get; set; } = "";
    public RequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal? Amount { get; set; }          // 没有明细时 SUM 是 NULL，所以用可空类型（第 7 章）
}

// 一页数据 + 总条数
public class PageResult<T>
{
    public int Total { get; set; }
    public List<T> Items { get; set; } = new();
}

// 单据详情：主表 + 明细 + 审批记录
public class RequestDetail
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Applicant { get; set; } = "";
    public RequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Remark { get; set; }
    public List<RequestItemView> Items { get; set; } = new();
    public List<ApprovalLogView> Logs { get; set; } = new();
}

public class RequestItemView
{
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public int Qty { get; set; }
    public decimal RequestPrice { get; set; }
    public decimal Amount => Qty * RequestPrice;  // 只读计算属性，Dapper 不会往里写（第 2 章练习）
}

public class ApprovalLogView
{
    public string Approver { get; set; } = "";
    public string Action { get; set; } = "";
    public string? Comment { get; set; }
    public DateTime ActedAt { get; set; }
}

// 新建申请单时，从菜单收集到的输入
public class NewRequest
{
    public string RequestNo { get; set; } = "";
    public int CustomerId { get; set; }
    public string Applicant { get; set; } = "";
    public string? Remark { get; set; }
    public List<NewItem> Items { get; set; } = new();
}

public class NewItem
{
    public int ProductId { get; set; }
    public int Qty { get; set; }
    public decimal RequestPrice { get; set; }
}

// 提交审批的结果：存储过程的 RETURN 值 + OUTPUT 参数（第 8 章）
public class SubmitResult
{
    public bool Success { get; set; }
    public RequestStatus? CurrentStatus { get; set; }   // 单号不存在时为 null
}
