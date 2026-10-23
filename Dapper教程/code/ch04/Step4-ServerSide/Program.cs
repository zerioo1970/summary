// code/ch04/Step4-ServerSide/Program.cs
// 到 SQL Server 的计划缓存里看：参数化和拼接，服务器实际收到的语句有什么不同
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 每条 SQL 开头加一个 /*ch04*/ 注释当记号，方便等会儿在计划缓存里只找出这几条
// A：参数化，执行 3 次
for (int id = 1; id <= 3; id++)
    conn.Query<RequestBrief>("/*ch04*/ SELECT RequestNo, Applicant FROM dbo.PriceRequest WHERE Id = @Id",
        new { Id = id });

// B：拼接，执行 3 次（3 个不同的值）
for (int id = 1; id <= 3; id++)
    conn.Query<RequestBrief>("/*ch04*/ SELECT RequestNo, Applicant FROM dbo.PriceRequest WHERE Id = " + id);

// C：字符串参数 + IN 列表
conn.Query<RequestBrief>("/*ch04*/ SELECT RequestNo, Applicant FROM dbo.PriceRequest WHERE RequestNo = @No",
    new { No = "PR20261001" });
conn.Query<RequestBrief>("/*ch04*/ SELECT RequestNo, Applicant FROM dbo.PriceRequest WHERE Id IN @Ids",
    new { Ids = new[] { 1, 2 } });

// 查计划缓存：objtype 是计划类型，usecounts 是被复用的次数，text 是服务器收到的语句
var plans = conn.Query<PlanRow>(@"
    SELECT cp.objtype AS ObjType, cp.usecounts AS UseCounts, st.text AS SqlText
    FROM sys.dm_exec_cached_plans AS cp
    CROSS APPLY sys.dm_exec_sql_text(cp.plan_handle) AS st
    WHERE st.text LIKE '%/*ch04*/%'
      AND st.text NOT LIKE '%dm_exec_cached_plans%'   -- 排除这条查询自己
    ORDER BY cp.objtype, st.text");

foreach (var p in plans)
    Console.WriteLine($"{p.ObjType,-9}{p.UseCounts,3}  {p.SqlText}");

public class RequestBrief
{
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
}

public class PlanRow
{
    public string ObjType { get; set; } = "";
    public int UseCounts { get; set; }
    public string SqlText { get; set; } = "";
}
