// code/ch16/Step2-Config/Program.cs
// 连接串从 appsettings.json 读出来，交给连接工厂；业务代码里不再出现连接串
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

// ① 读配置：从程序运行目录（bin\Debug\net10.0\）找 appsettings.json
IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)   // optional: false —— 找不到文件就立刻报错，不要默默跑下去
    .Build();

// ② GetConnectionString("PriceDemo") 就是读 "ConnectionStrings" 节点下的 "PriceDemo"
string? connStr = config.GetConnectionString("PriceDemo");

// ③ 连接工厂：全程序只在这里用到连接串
IDbConnectionFactory factory = new SqlConnectionFactory(connStr);

// ④ 业务代码：只跟工厂要连接
using (IDbConnection conn = factory.Create())
{
    int customers = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.Customer");
    int products = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.Product");
    Console.WriteLine($"客户 {customers} 个，产品 {products} 个");
}

// 连接串里的库名也能取出来看看（不要把整串打印到日志里：里面可能有密码）
var csb = new SqlConnectionStringBuilder(connStr);
Console.WriteLine($"配置文件位置：{Path.Combine(AppContext.BaseDirectory, "appsettings.json")}");
Console.WriteLine($"连接的库：{csb.InitialCatalog}");

// 只有一个方法的接口：调用方只知道"能要到一个连接"，不知道连的是哪台服务器
public interface IDbConnectionFactory
{
    IDbConnection Create();
}

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connStr;

    public SqlConnectionFactory(string? connStr)
    {
        // 配置写错时在启动阶段就报清楚，而不是等到第一次查询时报一句看不懂的错
        if (string.IsNullOrWhiteSpace(connStr))
            throw new InvalidOperationException("appsettings.json 里没有找到 ConnectionStrings:PriceDemo");
        _connStr = connStr;
    }

    // 每次都 new 一个新的：便宜（连接池兜底），而且每个调用方各用各的，互不干扰
    public IDbConnection Create() => new SqlConnection(_connStr);
}
