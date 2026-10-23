// code/ch16/Step3-Repository/Program.cs
// Repository 封装：客户、申请单各一个仓储类；业务代码只调方法，不碰连接串、不碰 SQL
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

// 程序启动时组装一次：工厂 → 仓储。之后到处用仓储
IDbConnectionFactory factory = new SqlConnectionFactory(config.GetConnectionString("PriceDemo"));
var customers = new CustomerRepository(factory);
var requests = new PriceRequestRepository(factory);

// 1. 查全部客户
Console.WriteLine("== 1 全部客户");
foreach (var c in customers.GetAll())
    Console.WriteLine($"{c.Id}  {c.Name}  {c.City}  {c.Level}");

// 2. 按 Id 查：查得到 / 查不到
Console.WriteLine("== 2 按 Id 查");
Console.WriteLine($"Id=2：{customers.GetById(2)?.Name}");
Console.WriteLine($"Id=99：{(customers.GetById(99) is null ? "null（查不到）" : "有")}");

// 3. 新增客户，再给他建一张申请单
Console.WriteLine("== 3 新增");
int newCustomerId = customers.Insert(new Customer { Name = "西部康复医院", City = "成都", Level = "B" });
Console.WriteLine($"新客户 Id = {newCustomerId}");

int newRequestId = requests.Insert(new PriceRequest
{
    RequestNo = "PR20261020",
    CustomerId = newCustomerId,
    Applicant = "王芳",
    Remark = "首次报价"
});
var r = requests.GetById(newRequestId)!;
Console.WriteLine($"新申请单 Id = {r.Id}  {r.RequestNo}  客户 {r.CustomerId}  {r.Applicant}  状态 {r.Status}  {r.Remark}");

// 4. 全部申请单
Console.WriteLine("== 4 全部申请单");
foreach (var x in requests.GetAll())
    Console.WriteLine($"{x.Id}  {x.RequestNo}  客户 {x.CustomerId}  {x.Applicant}  状态 {x.Status}  {x.Remark ?? "(无备注)"}");

// ===================== 以下是可复用的部分 =====================

public interface IDbConnectionFactory
{
    IDbConnection Create();
}

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connStr;

    public SqlConnectionFactory(string? connStr)
    {
        if (string.IsNullOrWhiteSpace(connStr))
            throw new InvalidOperationException("appsettings.json 里没有找到 ConnectionStrings:PriceDemo");
        _connStr = connStr;
    }

    public IDbConnection Create() => new SqlConnection(_connStr);
}

// 一张表一个仓储：这张表的 SQL 全在这里，改表结构时只需要找这一个类
public class CustomerRepository
{
    private readonly IDbConnectionFactory _factory;

    public CustomerRepository(IDbConnectionFactory factory) => _factory = factory;

    public List<Customer> GetAll()
    {
        using var conn = _factory.Create();          // 每个方法自己要连接、用完立刻还（Dispose → 回到连接池）
        return conn.Query<Customer>(
            "SELECT Id, Name, City, Level FROM dbo.Customer ORDER BY Id").ToList();
    }

    public Customer? GetById(int id)
    {
        using var conn = _factory.Create();
        return conn.QuerySingleOrDefault<Customer>(   // 第 5 章：最多一行，查不到返回 null
            "SELECT Id, Name, City, Level FROM dbo.Customer WHERE Id = @Id", new { Id = id });
    }

    public int Insert(Customer c)
    {
        using var conn = _factory.Create();
        return conn.QuerySingle<int>(@"
            INSERT dbo.Customer (Name, City, Level)
            OUTPUT INSERTED.Id                        -- 第 6 章：直接拿回自增 Id
            VALUES (@Name, @City, @Level)", c);       // 实体对象直接当参数：属性名对上 @参数名
    }
}

public class PriceRequestRepository
{
    private readonly IDbConnectionFactory _factory;

    public PriceRequestRepository(IDbConnectionFactory factory) => _factory = factory;

    // 列清单只写一次，三个查询共用，避免某个方法漏查一列
    private const string Columns = "Id, RequestNo, CustomerId, Applicant, Status, CreatedAt, Remark";

    public List<PriceRequest> GetAll()
    {
        using var conn = _factory.Create();
        return conn.Query<PriceRequest>(
            $"SELECT {Columns} FROM dbo.PriceRequest ORDER BY Id").ToList();
    }

    public PriceRequest? GetById(int id)
    {
        using var conn = _factory.Create();
        return conn.QuerySingleOrDefault<PriceRequest>(
            $"SELECT {Columns} FROM dbo.PriceRequest WHERE Id = @Id", new { Id = id });
    }

    public int Insert(PriceRequest r)
    {
        using var conn = _factory.Create();
        // Status、CreatedAt 不传：用表上的默认值（0 草稿、当前时间）
        return conn.QuerySingle<int>(@"
            INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant, Remark)
            OUTPUT INSERTED.Id
            VALUES (@RequestNo, @CustomerId, @Applicant, @Remark)", r);
    }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
    public string Level { get; set; } = "";
}

public class PriceRequest
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public int CustomerId { get; set; }
    public string Applicant { get; set; } = "";
    public byte Status { get; set; }                 // 列是 TINYINT
    public DateTime CreatedAt { get; set; }
    public string? Remark { get; set; }              // 列允许 NULL（第 7 章）
}
