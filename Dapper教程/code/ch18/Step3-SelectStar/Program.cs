// code/ch18/Step3-SelectStar/Program.cs
// SELECT * 的三个隐患：加列后多拿数据、JOIN 后同名列互相覆盖、列改名后静默变成 0
// 运行前先执行 code/sql/ch18-add-discount.sql。程序会临时改一次列名，结束前一定改回来
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 隐患 1：表加了 Discount 列，SELECT * 自动多拿一列；OldItem 里没有这个属性，Dapper 静默丢掉
var columns = conn.Query<string>(
    "SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PriceRequestItem') ORDER BY column_id");
Console.WriteLine($"== 隐患 1：SELECT * 现在返回 {columns.Count()} 列：{string.Join(", ", columns)}");
var a = conn.QuerySingle<OldItem>("SELECT * FROM dbo.PriceRequestItem WHERE Id = 1");
Console.WriteLine($"   OldItem 照样能用：明细 {a.Id}  {a.Qty} × {a.RequestPrice:N2}（Discount 被悄悄丢掉，没有任何提示）");

// 隐患 2：两张表都有 Id 列。SELECT * 返回两个叫 Id 的列，Dapper 按顺序赋值，后一个覆盖前一个
var b = conn.QuerySingle<OldItem>(@"
    SELECT * FROM dbo.PriceRequestItem i
    JOIN dbo.Product p ON p.Id = i.ProductId
    WHERE i.Id = 2");
Console.WriteLine($"== 隐患 2：查的是明细 2，拿到的 Id = {b.Id}，ProductId = {b.ProductId}");

// 隐患 3：列改名。同一个需求写成两种 SQL，看改名前后各自的表现
const string starSql = "SELECT * FROM dbo.PriceRequestItem WHERE RequestId = 1 ORDER BY Id";
const string listSql = "SELECT Id, RequestId, ProductId, Qty, RequestPrice FROM dbo.PriceRequestItem WHERE RequestId = 1 ORDER BY Id";

Console.WriteLine("== 隐患 3：改名前");
Print("SELECT *  ", conn.Query<OldItem>(starSql));
Print("显式列清单", conn.Query<OldItem>(listSql));

// 模拟 DBA 把 RequestPrice 改名为 UnitPrice（只为演示，finally 里一定改回来）
conn.Execute("EXEC sp_rename 'dbo.PriceRequestItem.RequestPrice', 'UnitPrice', 'COLUMN'");
try
{
    Console.WriteLine("== 隐患 3：RequestPrice 改名为 UnitPrice 之后");
    Print("SELECT *  ", conn.Query<OldItem>(starSql));        // 不报错，价格全变 0
    try
    {
        Print("显式列清单", conn.Query<OldItem>(listSql));    // 立刻报错，告诉你哪一列不存在
    }
    catch (SqlException ex)
    {
        Console.WriteLine($"显式列清单 → SqlException {ex.Number}：{ex.Message}");
    }
}
finally
{
    conn.Execute("EXEC sp_rename 'dbo.PriceRequestItem.UnitPrice', 'RequestPrice', 'COLUMN'");
    Console.WriteLine("（已把列名改回 RequestPrice）");
}

static void Print(string title, IEnumerable<OldItem> items) =>
    Console.WriteLine($"{title} → " + string.Join("；", items.Select(i => $"明细 {i.Id} 单价 {i.RequestPrice:N2}")));

// 加折扣之前写的类：没有 Discount 属性
public class OldItem
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public int ProductId { get; set; }
    public int Qty { get; set; }
    public decimal RequestPrice { get; set; }
}
