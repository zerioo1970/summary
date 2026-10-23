// code/ch17/Step1-Handler/Program.cs
// 自定义类型处理器：JSON 文本列 Spec 读出来直接是 ProductSpec 对象，写的时候自动转回 JSON
// 运行前先执行 code/sql/ch17-json-column.sql
using System.Data;
using System.Text.Encodings.Web;
using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

// 注册：告诉 Dapper "遇到 ProductSpec 类型，就交给这个处理器"。放在程序最开头（17.3 节讲为什么）
SqlMapper.AddTypeHandler(new ProductSpecHandler());

using var conn = new SqlConnection(connStr);

// 读：Spec 列里的 JSON 文本 → ProductSpec 对象
Console.WriteLine("== 读");
foreach (var p in conn.Query<Product>("SELECT Id, Code, Name, Spec FROM dbo.Product ORDER BY Id"))
{
    if (p.Spec is null)
        Console.WriteLine($"{p.Code,-8}（没有录规格）");
    else
        Console.WriteLine($"{p.Code,-8}{p.Spec.Category}  {p.Spec.FrequencyMHz} MHz  {p.Spec.WeightKg} kg");
}

// 写：ProductSpec 对象直接当参数，处理器负责把它变成 JSON 文本
Console.WriteLine("== 写");
var spec = new ProductSpec { FrequencyMHz = 3.5m, WeightKg = 0.25m, Category = "探头" };
int rows = conn.Execute("UPDATE dbo.Product SET Spec = @Spec WHERE Code = @Code",
                        new { Spec = spec, Code = "PB-02" });
Console.WriteLine($"更新 {rows} 行");

// 绕过处理器：用 ExecuteScalar<string> 直接看库里存的原始文本
string? raw = conn.ExecuteScalar<string>("SELECT Spec FROM dbo.Product WHERE Code = 'PB-02'");
Console.WriteLine($"库里存的原文：{raw}");

// 再经过处理器读回来
var back = conn.QuerySingle<Product>("SELECT Id, Code, Name, Spec FROM dbo.Product WHERE Code = 'PB-02'");
Console.WriteLine($"读回来：{back.Spec!.Category}  {back.Spec.FrequencyMHz} MHz  {back.Spec.WeightKg} kg");

// 写 null：属性是 null 时，库里应该是 NULL，而不是字符串 "null"
conn.Execute("UPDATE dbo.Product SET Spec = @Spec WHERE Code = @Code",
             new { Spec = (ProductSpec?)null, Code = "PB-02" });
bool isNull = conn.ExecuteScalar<int>("SELECT CASE WHEN Spec IS NULL THEN 1 ELSE 0 END FROM dbo.Product WHERE Code = 'PB-02'") == 1;
Console.WriteLine($"写入 null 后，库里是 NULL 吗？{isNull}");

// 处理器：一个类型 = 一个处理器。只需要实现两个方法：写（SetValue）和读（Parse）
public class ProductSpecHandler : SqlMapper.TypeHandler<ProductSpec>
{
    // 不转义中文：默认设置会把"探头"存成 \u63A2\u5934（见踩坑警告）
    private static readonly JsonSerializerOptions Options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    // 写：C# 对象 → 参数。Dapper 在生成 SqlParameter 时调用
    // 值是 null 时 Dapper 自己传 DBNull，不会调用这里（实测）；下面的 null 判断只是保险
    public override void SetValue(IDbDataParameter parameter, ProductSpec? value)
    {
        parameter.DbType = DbType.String;   // nvarchar
        parameter.Size = -1;                // -1 表示 MAX，和列定义 NVARCHAR(MAX) 一致
        parameter.Value = value is null ? DBNull.Value : JsonSerializer.Serialize(value, Options);
    }

    // 读：数据库里的值 → C# 对象。每读到一行非 NULL 的 Spec 就调用一次；NULL 不调用，属性直接是 null（实测）
    public override ProductSpec? Parse(object value) =>
        JsonSerializer.Deserialize<ProductSpec>((string)value, Options);
}

// JSON 里的字段名和这里的属性名一致
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
    public ProductSpec? Spec { get; set; }   // 列是 JSON 文本，属性是对象——中间靠处理器转换
}
