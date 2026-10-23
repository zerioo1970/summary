// code/ch23/Exercise/Program.cs
// 第 23 章练习参考答案：在 PriceManager 的菜单上加 6 审批、7 客户汇总（其余部分与 PriceManager 相同）
using Microsoft.Data.SqlClient;
using PriceManager;

const int PageSize = 2;                                  // 每页 2 条：示例库只有 3 张单，方便看到翻页

var repo = new PriceRequestRepository(SqlConnectionFactory.FromAppSettings());

while (true)
{
    Console.WriteLine();
    Console.WriteLine("===== 价格申请单管理 =====");
    Console.WriteLine("1 分页列表  2 查看单据  3 新建申请单  4 提交审批  5 按单号查询");
    Console.WriteLine("6 审批  7 客户汇总  0 退出");
    string? choice = Ask("请选择：");
    if (choice is null or "0") break;                    // null：输入流结束（管道输入读完了）

    try
    {
        switch (choice)
        {
            case "1": ShowPage(); break;
            case "2": ShowDetail(); break;
            case "3": CreateRequest(); break;
            case "4": SubmitRequest(); break;
            case "5": FindByNo(); break;
            case "6": ApproveRequest(); break;        // 练习 1 新增
            case "7": ShowCustomerSummary(); break;   // 练习 2 新增
            default: Console.WriteLine("没有这个选项"); break;
        }
    }
    catch (SqlException ex)                              // 兜底：数据库报错不让程序退出，打印错误号方便查（第 20 章）
    {
        Console.WriteLine($"数据库错误 {ex.Number}：{ex.Message}");
    }
}
Console.WriteLine("再见");

// ---------- 菜单 1 ----------
void ShowPage()
{
    string? applicant = Ask("申请人（直接回车 = 全部）：");
    int page = Math.Max(1, AskInt("页码：") ?? 1);              // 不填或填 0 都按第 1 页
    var result = repo.GetPage(applicant, page, PageSize);

    int pageCount = (result.Total + PageSize - 1) / PageSize;   // 总页数向上取整
    Console.WriteLine($"第 {page}/{pageCount} 页，共 {result.Total} 条");
    foreach (var r in result.Items)
        Console.WriteLine($"  {r.Id,-3}{r.RequestNo,-12}{r.CustomerName,-8}{r.Applicant,-4}" +
                          $"{r.Status.ToText(),-4}{r.CreatedAt:yyyy-MM-dd}{r.Amount ?? 0,14:N2}");
    if (result.Items.Count == 0) Console.WriteLine("  （这一页没有数据）");
}

// ---------- 菜单 2 ----------
void ShowDetail()
{
    int? id = AskInt("申请单 Id：");
    if (id is null) return;
    var d = repo.GetDetail(id.Value);
    if (d is null) { Console.WriteLine($"找不到 Id = {id} 的申请单"); return; }

    Console.WriteLine($"单号 {d.RequestNo}  客户 {d.CustomerName}  申请人 {d.Applicant}  状态 {d.Status.ToText()}");
    Console.WriteLine($"创建 {d.CreatedAt:yyyy-MM-dd HH:mm}  备注 {d.Remark ?? "（无）"}");
    Console.WriteLine("  明细：");
    foreach (var i in d.Items)
        Console.WriteLine($"    {i.ProductCode,-8}{i.ProductName,-8}{i.Qty,3} × {i.RequestPrice,12:N2} = {i.Amount,12:N2}");
    Console.WriteLine($"    合计 {d.Items.Sum(i => i.Amount):N2}");
    Console.WriteLine("  审批记录：");
    foreach (var l in d.Logs)
        Console.WriteLine($"    {l.ActedAt:yyyy-MM-dd HH:mm}  {l.Approver}  {l.Action}  {l.Comment}");
    if (d.Logs.Count == 0) Console.WriteLine("    （无）");
}

// ---------- 菜单 3 ----------
void CreateRequest()
{
    var r = new NewRequest
    {
        RequestNo = Ask("单号：") ?? "",
        CustomerId = AskInt("客户 Id：") ?? 0,
        Applicant = Ask("申请人：") ?? "",
    };
    string? remark = Ask("备注（可空）：");
    r.Remark = string.IsNullOrWhiteSpace(remark) ? null : remark;   // 空字符串存成 NULL

    while (true)
    {
        string? line = Ask("明细（产品Id 数量 单价，直接回车结束）：");
        if (string.IsNullOrWhiteSpace(line)) break;
        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 3 && int.TryParse(parts[0], out int pid)
            && int.TryParse(parts[1], out int qty) && decimal.TryParse(parts[2], out decimal price))
            r.Items.Add(new NewItem { ProductId = pid, Qty = qty, RequestPrice = price });
        else
            Console.WriteLine("  格式不对，这一行没有加入。例：1 2 165000");
    }
    if (r.Items.Count == 0) { Console.WriteLine("至少要有一条明细，已取消"); return; }

    try
    {
        int id = repo.Create(r);
        Console.WriteLine($"新建成功：Id = {id}，单号 {r.RequestNo}，明细 {r.Items.Count} 条");
    }
    catch (SqlException ex) when (ex.Number == 547)      // 外键：客户或产品不存在
    {
        Console.WriteLine("客户或产品不存在，整张单据没有保存（已回滚）");
    }
    catch (SqlException ex) when (ex.Number == 2627)     // 唯一键：单号重复
    {
        Console.WriteLine($"单号 {r.RequestNo} 已存在，请换一个");
    }
}

// ---------- 菜单 4 ----------
void SubmitRequest()
{
    string no = Ask("单号：") ?? "";
    string op = Ask("操作人：") ?? "";
    var result = repo.Submit(no, op);

    if (result.Success)
        Console.WriteLine($"提交成功，当前状态：{result.CurrentStatus!.Value.ToText()}");
    else if (result.CurrentStatus is null)
        Console.WriteLine($"提交失败：单号 {no} 不存在");
    else
        Console.WriteLine($"提交失败：当前状态是「{result.CurrentStatus.Value.ToText()}」，只有草稿能提交");
}

// ---------- 菜单 5 ----------
void FindByNo()
{
    string no = Ask("单号：") ?? "";
    var r = repo.GetByNo(no);
    Console.WriteLine(r is null
        ? $"找不到单号 {no}"
        : $"Id {r.Id}  {r.RequestNo}  {r.CustomerName}  {r.Applicant}  {r.Status.ToText()}  金额 {r.Amount ?? 0:N2}");
}

// ---------- 菜单 6（练习 1） ----------
void ApproveRequest()
{
    string no = Ask("单号：") ?? "";
    string approver = Ask("审批人：") ?? "";
    string? decision = Ask("结果（1 通过 / 2 驳回）：");
    if (decision is not ("1" or "2")) { Console.WriteLine("只能输入 1 或 2"); return; }
    string? comment = Ask("意见（可空）：");

    bool ok = repo.Approve(no, approver, decision == "1", string.IsNullOrWhiteSpace(comment) ? null : comment);
    Console.WriteLine(ok ? "审批完成" : $"审批失败：单号 {no} 不存在，或者不是「待审」状态");
}

// ---------- 菜单 7（练习 2） ----------
void ShowCustomerSummary()
{
    foreach (var c in repo.GetCustomerSummary())
        Console.WriteLine($"  {c.CustomerName,-8}{c.RequestCount,3} 张{c.Amount ?? 0,16:N2}");
}

// ---------- 读输入 ----------
// 输入来自管道（printf … | dotnet run）时，键入的内容不会显示在屏幕上，这里补打一遍，输出才看得懂
static string? Ask(string prompt)
{
    Console.Write(prompt);
    string? line = Console.ReadLine();
    if (Console.IsInputRedirected) Console.WriteLine(line);
    return line?.Trim();
}

static int? AskInt(string prompt)
{
    string? s = Ask(prompt);
    if (int.TryParse(s, out int n)) return n;
    if (!string.IsNullOrEmpty(s)) Console.WriteLine("  请输入数字");
    return null;
}
