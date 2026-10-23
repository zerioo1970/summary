// code/ch09/Exercise2-Bug/Program.cs
// 练习 2 的原题：运行就抛 InvalidOperationException。SQL 不许改，找出原因并修好。
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);
conn.Open();
using var tran = conn.BeginTransaction();

// 还没审完（状态 0 草稿 / 1 待审）的单据才允许改价
byte status = conn.ExecuteScalar<byte>(
    "SELECT Status FROM dbo.PriceRequest WHERE Id = @Id", new { Id = 3 });

if (status <= 1)
{
    conn.Execute("UPDATE dbo.PriceRequestItem SET RequestPrice = @Price WHERE Id = @ItemId",
        new { Price = 24000m, ItemId = 4 }, tran);
    conn.Execute("UPDATE dbo.PriceRequest SET Remark = @Remark WHERE Id = @Id",
        new { Remark = "探头改价", Id = 3 }, tran);
}
tran.Commit();

var price = conn.ExecuteScalar<decimal>("SELECT RequestPrice FROM dbo.PriceRequestItem WHERE Id = 4");
var remark = conn.ExecuteScalar<string>("SELECT Remark FROM dbo.PriceRequest WHERE Id = 3");
Console.WriteLine($"改价完成：明细 4 申请价 = {price:N2}，备注 = {remark}");
