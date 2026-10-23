// code/ch16/Exercise/Program.cs
// 第 16 章练习参考答案：ProductRepository（按编码查 + 改目录价），连接串来自 appsettings.json
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var products = new ProductRepository(new SqlConnectionFactory(config.GetConnectionString("PriceDemo")));

var p = products.GetByCode("PB-01")!;
Console.WriteLine($"{p.Code} {p.Name} {p.ListPrice:N2}");

int n1 = products.UpdateListPrice("PB-01", 25500m);
Console.WriteLine($"改价 PB-01：影响 {n1} 行 → {products.GetByCode("PB-01")!.ListPrice:N2}");

int n2 = products.UpdateListPrice("XX-999", 1m);
Console.WriteLine($"改价 XX-999：影响 {n2} 行");

Console.WriteLine($"查 XX-999：{products.GetByCode("XX-999")?.Name ?? "null"}");

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

public class ProductRepository
{
    private readonly IDbConnectionFactory _factory;

    public ProductRepository(IDbConnectionFactory factory) => _factory = factory;

    // Code 列是 VARCHAR(20)：用第 15 章的 DbString，参数类型和列一致，能走索引查找
    private static DbString CodeParam(string code) =>
        new DbString { Value = code, IsAnsi = true, Length = 20 };

    public Product? GetByCode(string code)
    {
        using var conn = _factory.Create();
        return conn.QuerySingleOrDefault<Product>(
            "SELECT Id, Code, Name, ListPrice FROM dbo.Product WHERE Code = @Code",
            new { Code = CodeParam(code) });
    }

    // 返回受影响行数：0 表示编码不存在（第 6 章）
    public int UpdateListPrice(string code, decimal price)
    {
        using var conn = _factory.Create();
        return conn.Execute(
            "UPDATE dbo.Product SET ListPrice = @Price WHERE Code = @Code",
            new { Code = CodeParam(code), Price = price });
    }
}

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal ListPrice { get; set; }
}
