// code/ch15/Step2-DbString/Program.cs
// 同一条按编码查产品的 SQL，用三种方式传字符串参数，再去计划缓存里看服务器收到的参数类型
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

// 只查 Id、Code：Code 上有唯一索引（建表时的 UNIQUE），这两列都在索引里，执行计划最简单
const string sql = "SELECT Id, Code FROM dbo.Product WHERE Code = @Code";

using var conn = new SqlConnection(connStr);
conn.Open();

// 方式 1：Dapper 普通 string 参数，查两个不同长度的编码
foreach (var code in new[] { "US-100", "PB-01" })
{
    var p = conn.QuerySingle<Product>(sql, new { Code = code });
    Console.WriteLine($"方式 1 string    ：{p.Id} {p.Code}");
}

// 方式 2：Dapper + DbString，明确告诉它"这是 varchar(20)"
foreach (var code in new[] { "US-100", "PB-01" })
{
    var p = conn.QuerySingle<Product>(sql, new
    {
        Code = new DbString { Value = code, IsAnsi = true, Length = 20 }  // IsAnsi=true → varchar；Length 与列定义一致
    });
    Console.WriteLine($"方式 2 DbString  ：{p.Id} {p.Code}");
}

// 方式 3：对照组——ADO.NET 的 AddWithValue（你在 WebForm 里最常写的那种）
foreach (var code in new[] { "US-100", "PB-01" })
{
    using var cmd = new SqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@Code", code);   // 没指定类型和长度，全靠驱动猜
    using var reader = cmd.ExecuteReader();
    reader.Read();
    Console.WriteLine($"方式 3 AddWithValue：{reader.GetInt32(0)} {reader.GetString(1)}");
}

// 去计划缓存里看：服务器收到的参数声明是什么、执行计划第一个运算符是什么、有没有隐式转换
var plans = conn.Query<PlanInfo>(@"
SELECT cp.usecounts AS UseCounts,
       LEFT(st.text, CHARINDEX(')SELECT', st.text)) AS ParamDecl,   -- 只取开头的参数声明部分
       qp.query_plan.value('declare namespace p=""http://schemas.microsoft.com/sqlserver/2004/07/showplan"";
                            (//p:RelOp/@PhysicalOp)[1]', 'nvarchar(60)') AS FirstOp,
       CASE WHEN CAST(qp.query_plan AS nvarchar(max)) LIKE '%CONVERT_IMPLICIT%' THEN N'有' ELSE N'无' END AS ImplicitConvert
FROM sys.dm_exec_cached_plans cp
CROSS APPLY sys.dm_exec_sql_text(cp.plan_handle) st
CROSS APPLY sys.dm_exec_query_plan(cp.plan_handle) qp
WHERE st.text LIKE '%FROM dbo.Product WHERE Code = @Code%'
  AND st.text NOT LIKE '%dm_exec%'           -- 排除这条查询自己
ORDER BY ParamDecl");

Console.WriteLine();
Console.WriteLine("计划缓存：使用次数 | 参数声明 | 第一个运算符 | 隐式转换");
foreach (var x in plans)
    Console.WriteLine($"{x.UseCounts} | {x.ParamDecl} | {x.FirstOp} | {x.ImplicitConvert}");

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
}

public class PlanInfo
{
    public int UseCounts { get; set; }
    public string ParamDecl { get; set; } = "";
    public string FirstOp { get; set; } = "";
    public string ImplicitConvert { get; set; } = "";
}
