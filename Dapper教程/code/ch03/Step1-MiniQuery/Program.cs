// code/ch03/Step1-MiniQuery/Program.cs
// 手写一个"迷你 Dapper"：用反射按列名给属性赋值，和真 Dapper 的结果对照
using System.Data;
using System.Reflection;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

const string sql = "SELECT Id, Code, Name, ListPrice FROM dbo.Product ORDER BY Id";

Console.WriteLine("== 迷你版 MiniQuery<Product>");
foreach (var p in conn.MiniQuery<Product>(sql))
    Console.WriteLine($"{p.Id} {p.Code,-8}{p.Name,-10}{p.ListPrice,12:N2}");
Console.WriteLine($"查完后连接：{conn.State}");

Console.WriteLine("== 真 Dapper Query<Product>");
foreach (var p in conn.Query<Product>(sql))
    Console.WriteLine($"{p.Id} {p.Code,-8}{p.Name,-10}{p.ListPrice,12:N2}");
Console.WriteLine($"查完后连接：{conn.State}");

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal ListPrice { get; set; }
}

public static class MiniDapper
{
    // 和 Dapper 一样写成 IDbConnection 的扩展方法；where T : new() 表示 T 必须能 new 出来
    public static List<T> MiniQuery<T>(this IDbConnection conn, string sql) where T : new()
    {
        bool wasClosed = conn.State == ConnectionState.Closed;
        if (wasClosed) conn.Open();                        // 第 1 步：连接关着就打开
        try
        {
            using var cmd = conn.CreateCommand();          // 第 2 步：建命令
            cmd.CommandText = sql;
            using var reader = cmd.ExecuteReader();        // 第 3 步：执行，拿到 DataReader

            // 第 4 步：每条查询只做一次——第 i 列对应哪个属性（不分大小写），对不上的记 null
            var props = new PropertyInfo?[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
                props[i] = typeof(T).GetProperty(reader.GetName(i),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            var list = new List<T>();
            while (reader.Read())                          // 第 5 步：每读一行，new 一个对象
            {
                var obj = new T();
                for (int i = 0; i < props.Length; i++)
                {
                    var prop = props[i];
                    if (prop is null || !prop.CanWrite || reader.IsDBNull(i)) continue;  // 对不上就跳过
                    prop.SetValue(obj, reader.GetValue(i));   // 反射赋值：每次调用都要做检查，慢
                }
                list.Add(obj);
            }
            return list;                                   // 第 6 步：全部读完才返回（缓冲）
        }
        finally
        {
            if (wasClosed) conn.Close();                   // 第 7 步：自己开的自己关
        }
    }
}
