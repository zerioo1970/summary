// code/ch19/Step3-LoggedQuery/Program.cs
// 给查询包一层日志：LoggedQuery<T> 在控制台打印 SQL、参数值、行数、耗时；出错时也打印，然后照样抛出
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 用法和 Query<T> 一样，只是方法名换成 LoggedQuery
Console.WriteLine("== 例 1 匿名对象");
var r1 = conn.LoggedQuery<string>(@"
    SELECT RequestNo
    FROM dbo.PriceRequest
    WHERE Applicant = @Applicant", new { Applicant = "张伟" });

Console.WriteLine("== 例 2 IN 列表");
var r2 = conn.LoggedQuery<string>(
    "SELECT RequestNo FROM dbo.PriceRequest WHERE Id IN @Ids", new { Ids = new[] { 1, 3 } });

Console.WriteLine("== 例 3 DynamicParameters，含一个 NULL");
var dp = new DynamicParameters();
dp.Add("Applicant", "李娜");
dp.Add("Remark", null);
var r3 = conn.LoggedQuery<string>(
    "SELECT RequestNo FROM dbo.PriceRequest WHERE Applicant = @Applicant AND (@Remark IS NULL OR Remark = @Remark)", dp);

Console.WriteLine("== 例 4 SQL 写错了");
try
{
    conn.LoggedQuery<string>("SELECT RequestNo FROM dbo.PriceRequest WHERE Applicnat = @Applicant",
                             new { Applicant = "张伟" });
}
catch (SqlException ex)
{
    // 日志已经在 LoggedQuery 里写过了；这里是调用方自己的处理
    Console.WriteLine($"调用方收到异常：{ex.Number}");
}

public static class DapperLogExtensions
{
    // 和 Query<T> 一样挂在 IDbConnection 上（第 2 章的扩展方法），调用处只改方法名
    public static List<T> LoggedQuery<T>(this IDbConnection conn, string sql, object? param = null)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var rows = conn.Query<T>(sql, param).ToList();
            sw.Stop();
            Log($"[SQL] {OneLine(sql)}");
            Log($"[参数] {Describe(param)}");
            Log($"[结果] {rows.Count} 行，{sw.ElapsedMilliseconds} ms");
            return rows;
        }
        catch (Exception ex)
        {
            sw.Stop();
            Log($"[SQL] {OneLine(sql)}");
            Log($"[参数] {Describe(param)}");
            Log($"[失败] {sw.ElapsedMilliseconds} ms，{ex.GetType().Name}：{ex.Message}");
            throw;   // 只记录，不吞掉：调用方该怎么处理异常还怎么处理
        }
    }

    // 真实项目里换成 ILogger、NLog 等；这里先写控制台，看得最直接
    private static void Log(string line) => Console.WriteLine(line);

    // 多行 SQL 压成一行，日志里好搜索
    private static string OneLine(string sql) => Regex.Replace(sql.Trim(), @"\s+", " ");

    // 把参数对象变成 "@名=值" 列表。匿名对象 / 实体用反射读属性；DynamicParameters 用 ParameterNames + Get
    private static string Describe(object? param)
    {
        if (param is null) return "（无）";
        if (param is DynamicParameters dp)
            return string.Join(", ", dp.ParameterNames.Select(n => $"@{n}={Show(dp.Get<object>(n))}"));
        return string.Join(", ", param.GetType().GetProperties()
                                      .Select(p => $"@{p.Name}={Show(p.GetValue(param))}"));
    }

    private static string Show(object? v) => v switch
    {
        null => "NULL",
        string s => $"'{s}'",                                            // 字符串加引号，能看出首尾空格
        IEnumerable list => "[" + string.Join(",", list.Cast<object>()) + "]",   // IN 列表：显示每个元素
        _ => v.ToString() ?? ""
    };
}
