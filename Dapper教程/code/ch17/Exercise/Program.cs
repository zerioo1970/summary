// code/ch17/Exercise/Program.cs
// 第 17 章练习参考答案：给"产品编码"值对象 ProductCode 写处理器，读写都用它，并确认参数是 varchar(20)
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

SqlMapper.AddTypeHandler(new ProductCodeHandler());   // 第一次查询之前注册

using var conn = new SqlConnection(connStr);

// 参数是 ProductCode，结果里的 Code 属性也是 ProductCode
var p = conn.QuerySingle<ProductItem>(
    "SELECT Code, Name FROM dbo.Product WHERE Code = @Code",
    new { Code = new ProductCode("PB-01") });
Console.WriteLine($"{p.Code} {p.Name}  Code 属性的类型 = {p.Code.GetType().Name}");

// 去计划缓存看服务器收到的参数声明（第 15 章的查法）
string decl = conn.QuerySingle<string>(@"
    SELECT LEFT(st.text, CHARINDEX(')SELECT', st.text))
    FROM sys.dm_exec_cached_plans cp
    CROSS APPLY sys.dm_exec_sql_text(cp.plan_handle) st
    WHERE st.text LIKE '%SELECT Code, Name FROM dbo.Product WHERE Code = @Code%'
      AND st.text NOT LIKE '%dm_exec%'");
Console.WriteLine($"参数声明：{decl}");

// 值对象：只包一个字符串，但类型上和"随便一个 string"区分开
public record ProductCode(string Value)
{
    public override string ToString() => Value;
}

public class ProductCodeHandler : SqlMapper.TypeHandler<ProductCode>
{
    // 写：和第 15 章 DbString { IsAnsi = true, Length = 20 } 的效果一样
    public override void SetValue(IDbDataParameter parameter, ProductCode? value)
    {
        parameter.DbType = DbType.AnsiString;   // varchar
        parameter.Size = 20;                    // 和列定义 VARCHAR(20) 一致
        parameter.Value = value is null ? DBNull.Value : value.Value;
    }

    // 读：varchar 读出来是 string，包一层
    public override ProductCode? Parse(object value) => new ProductCode((string)value);
}

public class ProductItem
{
    public ProductCode Code { get; set; } = new("");
    public string Name { get; set; } = "";
}
