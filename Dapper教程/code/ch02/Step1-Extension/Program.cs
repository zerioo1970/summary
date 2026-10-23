// code/ch02/Step1-Extension/Program.cs
// 自己写一个扩展方法，看懂 conn.Query 为什么能"长"在 SqlConnection 上
using System.Data;
using Microsoft.Data.SqlClient;
using PriceDemo.MyExtensions;      // 去掉这一行，下面的 conn.CountRows 就会报 CS1061

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 看起来像 SqlConnection 自带的方法，实际调用的是下面静态类里的静态方法
Console.WriteLine($"客户表：{conn.CountRows("dbo.Customer")} 行");

// 编译器把上一行翻译成的样子——两种写法完全等价
Console.WriteLine($"产品表：{ConnectionExtensions.CountRows(conn, "dbo.Product")} 行");

namespace PriceDemo.MyExtensions
{
    public static class ConnectionExtensions       // ① 必须是 static class
    {
        // ② 第一个参数前加 this：表示"把这个方法挂到 IDbConnection 上"
        //    写 IDbConnection 而不是 SqlConnection：任何数据库的连接都能用（Dapper 也是这么做的）
        public static int CountRows(this IDbConnection conn, string table)
        {
            bool wasClosed = conn.State == ConnectionState.Closed;
            if (wasClosed) conn.Open();            // 连接关着就替调用方打开——Dapper 同样这样做
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT COUNT(*) FROM {table}";   // 仅演示用；表名拼接的风险第 4 章讲
                return (int)cmd.ExecuteScalar()!;
            }
            finally
            {
                if (wasClosed) conn.Close();       // 谁打开谁关闭：调用方自己开的连接不替它关
            }
        }
    }
}
