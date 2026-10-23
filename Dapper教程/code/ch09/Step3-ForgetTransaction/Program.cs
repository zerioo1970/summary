// code/ch09/Step3-ForgetTransaction/Program.cs
// 事务进行中，忘传 transaction 会怎样：写、读、取单值三种情况各试一次，再看正确写法
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);
conn.Open();
using var tran = conn.BeginTransaction();

// 情况 1：写操作忘传 transaction
try
{
    conn.Execute("UPDATE dbo.PriceRequest SET Remark = N'改了备注' WHERE Id = 3");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine("情况 1 Execute 忘传：" + ex.Message);
}

// 情况 2：读操作也一样——事务进行中，这个连接上的每一条命令都必须带上事务
try
{
    conn.Query<string>("SELECT RequestNo FROM dbo.PriceRequest WHERE Id = 3");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine("情况 2 Query 忘传：" + ex.Message);
}

// 情况 3：取单值
try
{
    conn.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.PriceRequest");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine("情况 3 ExecuteScalar 忘传：" + ex.Message);
}

// 正确写法：每条命令都写 transaction: tran（用命名参数，不容易传错位置）
int rows = conn.Execute(
    "UPDATE dbo.PriceRequest SET Remark = N'改了备注' WHERE Id = 3", transaction: tran);
string remark = conn.ExecuteScalar<string>(
    "SELECT Remark FROM dbo.PriceRequest WHERE Id = 3", transaction: tran)!;
Console.WriteLine($"正确写法：改了 {rows} 行，事务里读到的备注 = {remark}");

tran.Rollback();   // 演示用，不保存修改
Console.WriteLine("回滚后的备注 = " + conn.ExecuteScalar<string>(
    "SELECT Remark FROM dbo.PriceRequest WHERE Id = 3"));
