// code/ch20/Step1-ThreeKinds/Program.cs
// 三类异常各来一个：连不上（连接类）、SQL 写错（SQL 类）、对象装不进去（映射类）。最后一个故意不 catch，看完整堆栈
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

// 下面 ①②③ 为了每个例子只占一行，直接 new SqlConnection 不写 using：连接没打开时 Dapper 自己开、自己关（第 2 章）
// ① 连接类：库名写错。用 SqlConnectionStringBuilder 只改库名，其余和正确的连接串一样
var wrongDb = new SqlConnectionStringBuilder(connStr) { InitialCatalog = "PriceDemoX" }.ConnectionString;
Try("① 连接类", () => new SqlConnection(wrongDb).Query<int>("SELECT 1"));

// ② SQL 类：列名写错（Applicnat）。连接没问题，是服务器拒绝了这条 SQL
Try("② SQL 类", () => new SqlConnection(connStr).Query<string>(
    "SELECT RequestNo FROM dbo.PriceRequest WHERE Applicnat = N'张伟'"));

// ③ 映射类：SQL 执行成功了，但 RequestNo（字符串）装不进 int
Try("③ 映射类", () => new SqlConnection(connStr).Query<int>(
    "SELECT RequestNo FROM dbo.PriceRequest"));

// ④ 不 catch：经过一层仓储再出错，看完整的报错和堆栈怎么读
Console.WriteLine("== ④ 不 catch，程序会在这里崩溃");
var repo = new RequestRepository(connStr);
Console.WriteLine(repo.FindNo(1));

// 只打印三样东西：异常类型、错误号（只有 SqlException 有）、消息。定位就靠这三样
static void Try(string title, Func<object> action)
{
    try
    {
        action();
        Console.WriteLine($"{title}：没有报错");
    }
    catch (Exception ex)
    {
        string number = ex is SqlException sql ? $"Number={sql.Number}" : "（不是 SqlException）";
        Console.WriteLine($"{title}：{ex.GetType().FullName}  {number}");
        Console.WriteLine($"    {ex.Message}");
    }
}

public class RequestRepository
{
    private readonly string _connStr;

    public RequestRepository(string connStr) => _connStr = connStr;

    public string FindNo(int id)
    {
        using var conn = new SqlConnection(_connStr);
        return conn.QuerySingle<string>(
            "SELECT RequestNo FROM dbo.PriceRequst WHERE Id = @Id",   // 表名少了一个 e
            new { Id = id });
    }
}
