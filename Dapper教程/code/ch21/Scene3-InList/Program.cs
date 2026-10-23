// code/ch21/Scene3-InList/Program.cs
// 场景 3：IN 查询报错——三种常见写错的方式，和唯一正确的写法
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 页面上勾选了第 1、3 号单据，要查出它们的单号
int[] ids = { 1, 3 };
string idsText = "1,3";                        // 前端传来的往往是这样一个逗号分隔的字符串

Try("错法 A：IN (@Ids) 多写了括号", () =>
    conn.Query<string>("SELECT RequestNo FROM dbo.PriceRequest WHERE Id IN (@Ids)",
        new { Ids = ids }));

Try("错法 B：把逗号字符串当列表传", () =>
    conn.Query<string>("SELECT RequestNo FROM dbo.PriceRequest WHERE Id IN @Ids",
        new { Ids = idsText }));

Try("错法 C：字符串 + 括号", () =>
    conn.Query<string>("SELECT RequestNo FROM dbo.PriceRequest WHERE Id IN (@Ids)",
        new { Ids = idsText }));

// 正确：参数是集合，SQL 里 IN 后面不写括号（第 4 章）
Try("正确：IN @Ids + int[]", () =>
    conn.Query<string>("SELECT RequestNo FROM dbo.PriceRequest WHERE Id IN @Ids",
        new { Ids = ids }));

// 字符串先在 C# 里拆成 int 数组，再按正确写法传
int[] parsed = idsText.Split(',').Select(int.Parse).ToArray();
Try("修复错法 B/C：先拆成 int[]", () =>
    conn.Query<string>("SELECT RequestNo FROM dbo.PriceRequest WHERE Id IN @Ids",
        new { Ids = parsed }));

// 每种写法都包在 try 里，一次运行就能看全所有报错
static void Try(string title, Func<IEnumerable<string>> query)
{
    try
    {
        Console.WriteLine($"{title} → {string.Join("、", query())}");
    }
    catch (SqlException ex)
    {
        Console.WriteLine($"{title} → SqlException {ex.Number}：{ex.Message}");
    }
}
