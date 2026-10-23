// code/ch17/Step2-RegisterTiming/Program.cs
// 注册时机：先查询、后注册处理器，会发生什么？同一条 SQL 和新的 SQL 表现不一样
// 运行前先执行 code/sql/ch17-json-column.sql
using System.Data;
using System.Text.Encodings.Web;
using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

const string sqlA = "SELECT Id, Code, Name, Spec FROM dbo.Product WHERE Code = 'US-100'";
const string sqlB = "SELECT Id, Code, Name, Spec FROM dbo.Product WHERE Code = 'US-200'";   // 只是 SQL 文本不同

// ① 还没注册就查
Try("① 未注册，查 sqlA", () => conn.QuerySingle<Product>(sqlA).Spec?.Category);

// ② 还没注册就写
Try("② 未注册，写 Spec", () => conn.Execute(
        "UPDATE dbo.Product SET Spec = @Spec WHERE Code = 'PB-02'",
        new { Spec = new ProductSpec { Category = "探头" } }).ToString());

// 现在才注册
SqlMapper.AddTypeHandler(new ProductSpecHandler());
Console.WriteLine($"—— 现在注册处理器，HasTypeHandler = {SqlMapper.HasTypeHandler(typeof(ProductSpec))}");

// ③ 注册后，再查同一条 sqlA
Try("③ 已注册，再查 sqlA", () => conn.QuerySingle<Product>(sqlA).Spec?.Category);

// ④ 注册后，查一条从没执行过的 sqlB
Try("④ 已注册，查 sqlB", () => conn.QuerySingle<Product>(sqlB).Spec?.Category);

// ⑤ 注册后，再写一次
Try("⑤ 已注册，写 Spec", () => conn.Execute(
        "UPDATE dbo.Product SET Spec = @Spec WHERE Code = 'PB-02'",
        new { Spec = new ProductSpec { Category = "探头" } }).ToString());

// ⑥ 清空 Dapper 的查询缓存（第 3 章讲的那个缓存），再查 sqlA
SqlMapper.PurgeQueryCache();
Try("⑥ PurgeQueryCache 后，查 sqlA", () => conn.QuerySingle<Product>(sqlA).Spec?.Category);

// 执行一段代码：成功打印结果，失败打印异常类型和消息的第一行
static void Try(string label, Func<string?> action)
{
    try
    {
        Console.WriteLine($"{label}：成功，结果 = {action()}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{label}：{ex.GetType().Name}: {ex.Message.Split('\n')[0]}");
        if (ex.InnerException is not null)
            Console.WriteLine($"      内层：{ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
    }
}

public class ProductSpecHandler : SqlMapper.TypeHandler<ProductSpec>
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public override void SetValue(IDbDataParameter parameter, ProductSpec? value)
    {
        parameter.DbType = DbType.String;
        parameter.Size = -1;
        parameter.Value = value is null ? DBNull.Value : JsonSerializer.Serialize(value, Options);
    }

    public override ProductSpec? Parse(object value) =>
        JsonSerializer.Deserialize<ProductSpec>((string)value, Options);
}

public class ProductSpec
{
    public decimal FrequencyMHz { get; set; }
    public decimal WeightKg { get; set; }
    public string Category { get; set; } = "";
}

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public ProductSpec? Spec { get; set; }
}
