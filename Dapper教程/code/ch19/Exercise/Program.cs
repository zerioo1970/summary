// code/ch19/Exercise/Program.cs
// 第 19 章练习参考答案：仿照 LoggedQuery 写 LoggedExecute，并用扩展事件核对服务器收到的参数类型
// 运行前先执行 code/sql/ch19-xe-start.sql；运行后执行 code/sql/ch19-xe-read.sql
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

// 应用程序名和 Step2 一样，扩展事件会话才抓得到
var builder = new SqlConnectionStringBuilder(connStr) { ApplicationName = "Ch19Demo" };
using var conn = new SqlConnection(builder.ConnectionString);

const string sql = "UPDATE dbo.PriceRequest SET Remark = @Remark WHERE Id = @Id";

conn.LoggedExecute(sql, new { Remark = "已电话确认", Id = 2 });    // 命中 1 行
conn.LoggedExecute(sql, new { Remark = "已电话确认", Id = 99 });   // 没有这张单：0 行，不报错，但日志里一眼能看出来

public static class DapperLogExtensions
{
    public static int LoggedExecute(this IDbConnection conn, string sql, object? param = null)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            int rows = conn.Execute(sql, param);
            sw.Stop();
            Log($"[SQL] {OneLine(sql)}");
            Log($"[参数] {Describe(param)}");
            Log($"[结果] 受影响 {rows} 行，{sw.ElapsedMilliseconds} ms");   // 增删改最关心受影响行数（第 6 章）
            return rows;
        }
        catch (Exception ex)
        {
            sw.Stop();
            Log($"[SQL] {OneLine(sql)}");
            Log($"[参数] {Describe(param)}");
            Log($"[失败] {sw.ElapsedMilliseconds} ms，{ex.GetType().Name}：{ex.Message}");
            throw;
        }
    }

    private static void Log(string line) => Console.WriteLine(line);

    private static string OneLine(string sql) => Regex.Replace(sql.Trim(), @"\s+", " ");

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
        string s => $"'{s}'",
        _ => v.ToString() ?? ""
    };
}
