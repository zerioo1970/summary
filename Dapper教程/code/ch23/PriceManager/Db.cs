// code/ch23/PriceManager/Db.cs
// 连接工厂：连接串只在这里读一次，其余代码只管"要一个连接"（第 16 章）
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace PriceManager;

public interface IDbConnectionFactory
{
    IDbConnection Create();
}

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connStr;

    public SqlConnectionFactory(string connStr) => _connStr = connStr;

    // 每次都 new 一个：SqlConnection 很轻，真正的物理连接由连接池复用
    public IDbConnection Create() => new SqlConnection(_connStr);

    // 从输出目录下的 appsettings.json 读 ConnectionStrings:PriceDemo
    public static SqlConnectionFactory FromAppSettings()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        string? connStr = config.GetConnectionString("PriceDemo");
        if (string.IsNullOrWhiteSpace(connStr))
            throw new InvalidOperationException("appsettings.json 里没有找到 ConnectionStrings:PriceDemo");
        return new SqlConnectionFactory(connStr);
    }
}
