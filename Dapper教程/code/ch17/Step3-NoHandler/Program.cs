// code/ch17/Step3-NoHandler/Program.cs
// 什么时候不该写处理器：① SQL 自己能取出来的，用 JSON_VALUE + 别名；② 给 string 这种内置类型注册处理器，读写表现不一致
// 运行前先执行 code/sql/ch17-json-column.sql
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 反例 1：只想要规格里的一两个字段 —— 不写处理器。JSON_VALUE 在 SQL 里取出字段，AS 成属性名即可
Console.WriteLine("== 反例 1：JSON_VALUE + 别名，不需要处理器");
var rows = conn.Query<ProductWeight>(@"
    SELECT Code,
           JSON_VALUE(Spec, '$.Category')                         AS Category,
           CAST(JSON_VALUE(Spec, '$.WeightKg') AS DECIMAL(9,2))   AS WeightKg
    FROM dbo.Product
    ORDER BY Id");
foreach (var r in rows)
    Console.WriteLine($"{r.Code,-8}{r.Category ?? "-",-4}{r.WeightKg?.ToString() ?? "NULL"}");

// 还能在 WHERE 里按规格筛选——处理器只在 C# 这边工作，帮不了 WHERE
var light = conn.Query<string>(@"
    SELECT Code FROM dbo.Product
    WHERE CAST(JSON_VALUE(Spec, '$.WeightKg') AS DECIMAL(9,2)) < 1
    ORDER BY Id");
Console.WriteLine("重量 < 1 kg：" + string.Join("、", light));

// 反例 2：想"顺手"让所有字符串自动去掉首尾空格，给 string 注册一个处理器
Console.WriteLine("== 反例 2：给 string 注册处理器");
SqlMapper.AddTypeHandler(new TrimStringHandler());

// 读：结果里的字符串列
var t = conn.QuerySingle<CodeOnly>("SELECT '  US-100  ' AS Code");
Console.WriteLine($"读：Code = [{t.Code}]，Parse 被调用 {TrimStringHandler.ParseCalls} 次");

// 写：字符串参数前面带了空格，如果 SetValue 生效，空格会被去掉，就能查到 US-100
int? id = conn.QuerySingleOrDefault<int?>(
    "SELECT Id FROM dbo.Product WHERE Code = @Code", new { Code = "  US-100" });
Console.WriteLine($"写：查到的 Id = {id?.ToString() ?? "null"}，SetValue 被调用 {TrimStringHandler.SetValueCalls} 次");

public class TrimStringHandler : SqlMapper.TypeHandler<string>
{
    public static int ParseCalls;
    public static int SetValueCalls;

    public override void SetValue(IDbDataParameter parameter, string? value)
    {
        SetValueCalls++;                    // 数一数到底有没有被调用
        parameter.Value = (object?)value?.Trim() ?? DBNull.Value;
    }

    public override string? Parse(object value)
    {
        ParseCalls++;
        return ((string)value).Trim();
    }
}

public class ProductWeight
{
    public string Code { get; set; } = "";
    public string? Category { get; set; }
    public decimal? WeightKg { get; set; }   // PB-02 没有规格，JSON_VALUE 返回 NULL
}

public class CodeOnly
{
    public string Code { get; set; } = "";
}
