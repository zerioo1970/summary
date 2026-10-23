// code/ch22/Step2-Endpoints/Program.cs
// 三个接口：客户列表、申请单详情（含明细，查不到 404）、新建申请单（事务）。仓储方法全部异步
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

string connStr = builder.Configuration.GetConnectionString("PriceDemo")
    ?? throw new InvalidOperationException("appsettings.json 里没有 ConnectionStrings:PriceDemo");
builder.Services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connStr));
builder.Services.AddScoped<CustomerRepository>();
builder.Services.AddScoped<PriceRequestRepository>();

var app = builder.Build();

// GET /customers：直接返回列表，框架把它序列化成 JSON 数组，状态码 200
app.MapGet("/customers", async (CustomerRepository repo) => await repo.GetAllAsync());

// GET /requests/1：{id:int} 限定只匹配整数，id 自动从路由里取
app.MapGet("/requests/{id:int}", async (int id, PriceRequestRepository repo) =>
{
    var detail = await repo.GetDetailAsync(id);
    return detail is null
        ? Results.NotFound(new { message = $"申请单 {id} 不存在" })   // 404
        : Results.Ok(detail);                                          // 200
});

// POST /requests：请求体的 JSON 自动反序列化成 NewRequest
app.MapPost("/requests", async (NewRequest body, PriceRequestRepository repo) =>
{
    try
    {
        int id = await repo.CreateAsync(body);
        return Results.Created($"/requests/{id}", new { id });        // 201 + Location 头
    }
    catch (SqlException ex) when (ex.Number == 547)                    // 外键：客户或产品不存在（第 20 章）
    {
        return Results.BadRequest(new { message = "客户或产品不存在，整张单据未保存" });
    }
    catch (SqlException ex) when (ex.Number == 2627)                   // 唯一键：单号重复
    {
        return Results.Conflict(new { message = $"单号 {body.RequestNo} 已存在" });
    }
});

app.Run();

// ===================== 连接工厂（同第 16 章） =====================

public interface IDbConnectionFactory
{
    IDbConnection Create();
}

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connStr;
    public SqlConnectionFactory(string connStr) => _connStr = connStr;
    public IDbConnection Create() => new SqlConnection(_connStr);
}

// ===================== 仓储：全部改成 Async 方法（第 14 章） =====================

public class CustomerRepository
{
    private readonly IDbConnectionFactory _factory;
    public CustomerRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<List<Customer>> GetAllAsync()
    {
        using var conn = _factory.Create();
        var list = await conn.QueryAsync<Customer>(
            "SELECT Id, Name, City, Level FROM dbo.Customer ORDER BY Id");
        return list.ToList();
    }
}

public class PriceRequestRepository
{
    private readonly IDbConnectionFactory _factory;
    public PriceRequestRepository(IDbConnectionFactory factory) => _factory = factory;

    // 主表 + 明细两个结果集，一次往返（第 12 章）
    public async Task<RequestDetail?> GetDetailAsync(int id)
    {
        using var conn = _factory.Create();
        using var grid = await conn.QueryMultipleAsync(@"
            SELECT r.Id, r.RequestNo, c.Name AS CustomerName, r.Applicant, r.Status, r.CreatedAt, r.Remark
            FROM dbo.PriceRequest r
            JOIN dbo.Customer c ON c.Id = r.CustomerId
            WHERE r.Id = @Id;

            SELECT i.ProductId, p.Name AS ProductName, i.Qty, i.RequestPrice
            FROM dbo.PriceRequestItem i
            JOIN dbo.Product p ON p.Id = i.ProductId
            WHERE i.RequestId = @Id
            ORDER BY i.Id;", new { Id = id });

        var head = await grid.ReadSingleOrDefaultAsync<RequestDetail>();   // 0 行 → null（第 5 章）
        if (head is null) return null;
        head.Items = (await grid.ReadAsync<RequestItemView>()).ToList();
        return head;
    }

    // 主表 + 明细放在一个事务里（第 9 章），返回新单据的 Id（第 6 章 OUTPUT INSERTED.Id）
    public async Task<int> CreateAsync(NewRequest r)
    {
        using var conn = _factory.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();         // 没 Commit 就离开 using → 自动回滚

        int id = await conn.QuerySingleAsync<int>(@"
            INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant, Remark)
            OUTPUT INSERTED.Id
            VALUES (@RequestNo, @CustomerId, @Applicant, @Remark)",
            new { r.RequestNo, r.CustomerId, r.Applicant, r.Remark }, tx);

        // 传集合：逐条执行，每条都在同一个事务里
        await conn.ExecuteAsync(@"
            INSERT dbo.PriceRequestItem (RequestId, ProductId, Qty, RequestPrice)
            VALUES (@RequestId, @ProductId, @Qty, @RequestPrice)",
            r.Items.Select(i => new { RequestId = id, i.ProductId, i.Qty, i.RequestPrice }), tx);

        tx.Commit();
        return id;
    }
}

// ===================== 数据类 =====================

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
    public string Level { get; set; } = "";
}

public class RequestDetail
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Applicant { get; set; } = "";
    public byte Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Remark { get; set; }
    public List<RequestItemView> Items { get; set; } = new();
}

public class RequestItemView
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int Qty { get; set; }
    public decimal RequestPrice { get; set; }
}

// POST 请求体：JSON 的属性名不分大小写对上这里（requestNo → RequestNo）
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
