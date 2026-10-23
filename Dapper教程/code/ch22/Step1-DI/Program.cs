// code/ch22/Step1-DI/Program.cs
// 依赖注入：启动时注册连接工厂和仓储，接口参数里直接"要"一个仓储
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// ① 连接串从 appsettings.json 的 ConnectionStrings:PriceDemo 读（第 16 章）
string connStr = builder.Configuration.GetConnectionString("PriceDemo")
    ?? throw new InvalidOperationException("appsettings.json 里没有 ConnectionStrings:PriceDemo");

// ② 注册服务：告诉容器"有人要 X 的时候，给他什么、多久换一个"
builder.Services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connStr)); // 全程一个
builder.Services.AddScoped<CustomerRepository>();                                    // 每个请求一个

var app = builder.Build();

// 参数 CustomerRepository repo：容器发现它已注册，就自动创建一个传进来
app.MapGet("/customers", (CustomerRepository repo) => repo.GetAll());

app.Run();

// ===================== 以下和第 16 章基本相同：连接串的空值检查挪到了上面，构造函数里多了计数输出 =====================

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

public class CustomerRepository
{
    private static int _count;                         // 只为演示：数一数容器一共创建了几个实例
    private readonly IDbConnectionFactory _factory;

    // 构造函数要一个 IDbConnectionFactory：容器会把上面注册的那一个单例传进来
    public CustomerRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
        Console.WriteLine($"CustomerRepository 第 {++_count} 个实例，工厂 HashCode = {factory.GetHashCode()}");
    }

    public List<Customer> GetAll()
    {
        using var conn = _factory.Create();
        return conn.Query<Customer>(
            "SELECT Id, Name, City, Level FROM dbo.Customer ORDER BY Id").ToList();
    }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
    public string Level { get; set; } = "";
}
