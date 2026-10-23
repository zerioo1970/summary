// code/ch21/Scene8-SchemaChange/Program.cs
// 场景 8：改了表以后旧查询静默出错——列 Remark 被改名为 Note
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 模拟 DBA 改表：Remark → Note。程序最后会改回去，不影响后面的章节
conn.Execute("EXEC sp_rename 'dbo.PriceRequest.Remark', 'Note', 'COLUMN'");
try
{
    Console.WriteLine("== 故障现象：SELECT * 的旧查询，不报错，备注全没了");
    foreach (var r in conn.Query<PriceRequest>("SELECT * FROM dbo.PriceRequest ORDER BY Id"))
        Console.WriteLine($"  {r.RequestNo}  备注：{r.Remark ?? "(null)"}");

    Console.WriteLine("== 对照：明确写出列名的查询，直接报错");
    try
    {
        conn.Query<PriceRequest>("SELECT Id, RequestNo, Remark FROM dbo.PriceRequest").ToList();
    }
    catch (SqlException ex)
    {
        Console.WriteLine($"  SqlException {ex.Number}：{ex.Message}");
    }

    Console.WriteLine("== 定位：查这张表现在有哪些列");
    var cols = conn.Query<string>(@"
        SELECT name FROM sys.columns
        WHERE object_id = OBJECT_ID('dbo.PriceRequest') ORDER BY column_id");
    Console.WriteLine("  " + string.Join(", ", cols));

    Console.WriteLine("== 修复：写明列名，新列名用别名对上旧属性");
    foreach (var r in conn.Query<PriceRequest>(@"
        SELECT Id, RequestNo, Note AS Remark FROM dbo.PriceRequest ORDER BY Id"))
        Console.WriteLine($"  {r.RequestNo}  备注：{r.Remark ?? "(null)"}");
}
finally
{
    // 改回原名，保证示例库和其他章节一致
    conn.Execute("EXEC sp_rename 'dbo.PriceRequest.Note', 'Remark', 'COLUMN'");
}

public class PriceRequest
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string? Remark { get; set; }
}
