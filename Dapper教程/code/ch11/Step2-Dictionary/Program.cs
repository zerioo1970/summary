// code/ch11/Step2-Dictionary/Program.cs
// 用字典去重：同一张单只保留第一次出现的那个对象，后面行里的明细都加到它身上
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 例 1：全部明细
List<PriceRequest> all = LoadRequests(conn, @"
    SELECT r.Id, r.RequestNo, r.Applicant,
           i.Id, i.ProductId, i.Qty, i.RequestPrice
    FROM dbo.PriceRequest r
    LEFT JOIN dbo.PriceRequestItem i ON i.RequestId = r.Id
    ORDER BY r.Id, i.Id");
Print("例 1 全部明细", all);

// 例 2：只看探头（产品 3、4）。2 号单没有探头 → LEFT JOIN 后明细那几列全是 NULL
List<PriceRequest> probes = LoadRequests(conn, @"
    SELECT r.Id, r.RequestNo, r.Applicant,
           i.Id, i.ProductId, i.Qty, i.RequestPrice
    FROM dbo.PriceRequest r
    LEFT JOIN dbo.PriceRequestItem i ON i.RequestId = r.Id AND i.ProductId IN (3, 4)
    ORDER BY r.Id, i.Id");
Print("例 2 只看探头", probes);

static List<PriceRequest> LoadRequests(SqlConnection conn, string sql)
{
    var byId = new Dictionary<int, PriceRequest>();   // 去重用：单据 Id → 第一次出现的那个对象
    var result = new List<PriceRequest>();            // 保持 SQL 里 ORDER BY 的顺序

    conn.Query<PriceRequest, RequestItem, PriceRequest>(
        sql,
        (request, item) =>
        {
            // 这张单第一次出现：登记进字典；以后再出现，就用字典里那一个，Dapper 新建的 request 直接丢掉
            if (!byId.TryGetValue(request.Id, out var head))
            {
                head = request;
                byId.Add(head.Id, head);
                result.Add(head);
            }

            // LEFT JOIN 没有明细时，明细那一段的切分列 i.Id 是 NULL，Dapper 传进来的 item 就是 null
            if (item is not null)
                head.Items.Add(item);

            return head;
        });                     // Query 的返回值不用了：它每行一个元素，还是 5 个（有重复引用）

    return result;
}

static void Print(string title, List<PriceRequest> list)
{
    Console.WriteLine($"{title}：{list.Count} 张单");
    foreach (var r in list)
        Console.WriteLine($"  {r.RequestNo}  {r.Applicant}  明细 {r.Items.Count} 条  " +
                          $"合计 {r.Items.Sum(x => x.Amount),12:N2}");
}

public class PriceRequest
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
    public List<RequestItem> Items { get; set; } = new();
}

public class RequestItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Qty { get; set; }
    public decimal RequestPrice { get; set; }
    public decimal Amount => Qty * RequestPrice;
}
