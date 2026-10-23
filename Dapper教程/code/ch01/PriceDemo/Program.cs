// code/ch01/PriceDemo/Program.cs
using Microsoft.Data.SqlClient;   // 新包的命名空间，不是 System.Data.SqlClient
using PriceDemo;

// 连接字符串：按你的实例改 Server 这一段（详见本章 1.4 节）
// 第 16 章会把它移到配置文件，这里先写死，先求跑通
const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);   // using：离开作用域自动 Close，不必写 finally
conn.Open();

// ① 连通性检查：取一个单值
using (var cmd = new SqlCommand("SELECT @@VERSION", conn))
{
    var version = (string)cmd.ExecuteScalar()!;
    Console.WriteLine("已连接：" + version.Split('\n')[0]);
}

// ② 用 ADO.NET 手工把结果集变成 List<Customer>——第 2 章要用 Dapper 替换的就是这一段
var customers = new List<Customer>();
using (var cmd = new SqlCommand("SELECT Id, Name, City, Level FROM dbo.Customer ORDER BY Id", conn))
using (var reader = cmd.ExecuteReader())
{
    while (reader.Read())
    {
        customers.Add(new Customer
        {
            Id    = reader.GetInt32(reader.GetOrdinal("Id")),      // 每一列都要手写一次
            Name  = reader.GetString(reader.GetOrdinal("Name")),   // 类型取错（GetInt32 读字符串）运行时才报错
            City  = reader.GetString(reader.GetOrdinal("City")),
            Level = reader.GetString(reader.GetOrdinal("Level")),
        });
    }
}

foreach (var c in customers)
    Console.WriteLine($"{c.Id}  {c.Name}  {c.City}  {c.Level}");

Console.WriteLine($"共 {customers.Count} 个客户");
