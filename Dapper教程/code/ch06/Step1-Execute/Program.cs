// code/ch06/Step1-Execute/Program.cs
// Execute 的四个例子：新建申请单、提交（看受影响行数）、一次改多行、删除撞外键
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 例 1：INSERT —— 新建一张草稿申请单。参数用匿名对象（第 4 章讲过）
// Status、CreatedAt 不写，用表上的默认值：0 草稿、当前时间
int inserted = conn.Execute(@"
    INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant, Remark)
    VALUES (@RequestNo, @CustomerId, @Applicant, @Remark)",
    new { RequestNo = "PR20261020", CustomerId = 3, Applicant = "王芳", Remark = "首次询价" });
Console.WriteLine($"例 1 新建：受影响 {inserted} 行");

// 例 2：UPDATE —— 提交审批。WHERE 里带上 Status = 0：只有草稿才能提交
const string submitSql =
    "UPDATE dbo.PriceRequest SET Status = 1 WHERE RequestNo = @RequestNo AND Status = 0";

int first  = conn.Execute(submitSql, new { RequestNo = "PR20261020" });   // 草稿 → 待审
int second = conn.Execute(submitSql, new { RequestNo = "PR20261020" });   // 已经是待审了
int wrong  = conn.Execute(submitSql, new { RequestNo = "PR99999999" });   // 单号不存在
Console.WriteLine($"例 2 提交：第 1 次 {first} 行，第 2 次 {second} 行，单号不存在 {wrong} 行");

// 受影响行数就是"条件有没有命中"的证据：0 行不会抛异常，要自己判断
if (second == 0)
    Console.WriteLine("       第 2 次没命中：单号不存在，或者已经不是草稿");

// 例 3：一条 UPDATE 改多行 —— 返回的是这条语句一共改了几行
int many = conn.Execute(
    "UPDATE dbo.PriceRequest SET Remark = ISNULL(Remark, N'') + N'（已复核）' WHERE Applicant = @Applicant",
    new { Applicant = "张伟" });
Console.WriteLine($"例 3 张伟的单子加备注：受影响 {many} 行");

// 例 4：DELETE —— 1 号单下面有明细和审批记录，外键不让删
try
{
    conn.Execute("DELETE dbo.PriceRequest WHERE Id = @Id", new { Id = 1 });
}
catch (SqlException ex)                        // 数据库报的错，Dapper 原样往外抛
{
    Console.WriteLine($"例 4 删除 1 号单失败：Number = {ex.Number}");
    Console.WriteLine(ex.Message);
}

// 例 5：DELETE —— 例 1 新建的单子没有明细，可以删
int deleted = conn.Execute("DELETE dbo.PriceRequest WHERE RequestNo = @RequestNo",
    new { RequestNo = "PR20261020" });
Console.WriteLine($"例 5 删除新单：受影响 {deleted} 行");
