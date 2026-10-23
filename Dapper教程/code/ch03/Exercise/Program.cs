// code/ch03/Exercise/Program.cs
// 第 3 章练习参考答案：练习 1（MiniQuery 报告跳过的列）+ 练习 2（预测缓存条数）
using System.Data;
using System.Reflection;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 练习 1：结果集里多了 Memo 列，Product 里没有这个属性
Console.WriteLine("== 练习 1");
var one = conn.MiniQuery<Product>(
    "SELECT Id, Code, Name, ListPrice, N'测试' AS Memo FROM dbo.Product WHERE Id = 1");
Console.WriteLine($"{one[0].Code} {one[0].ListPrice:N2}");

// 练习 2：每一步之后打印 Dapper 的缓存条数
Console.WriteLine("== 练习 2");
const string sqlA = "SELECT Id, Code, Name, ListPrice FROM dbo.Product";
for (int i = 0; i < 3; i++) conn.Query<Product>(sqlA);        // 同 SQL 同类型跑 3 次：仍是 1 条
Console.WriteLine($"第 1 步：{SqlMapper.GetCachedSQLCount()}");
conn.Query<ProductBrief>(sqlA);                                // 同 SQL 换类型：+1
Console.WriteLine($"第 2 步：{SqlMapper.GetCachedSQLCount()}");
conn.Query<Product>(sqlA + " ");                               // 末尾多一个空格：文本不同，+1
Console.WriteLine($"第 3 步：{SqlMapper.GetCachedSQLCount()}");
for (int id = 1; id <= 3; id++)
    conn.Query<Product>(sqlA + " WHERE Id = " + id);           // 拼接 3 个不同的 Id：+3
Console.WriteLine($"第 4 步：{SqlMapper.GetCachedSQLCount()}");
for (int id = 1; id <= 3; id++)
    conn.Query<Product>(sqlA + " WHERE Id = @Id", new { Id = id });   // 参数写法，文本不变：+1
Console.WriteLine($"第 5 步：{SqlMapper.GetCachedSQLCount()}");

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal ListPrice { get; set; }
}

public class ProductBrief
{
    public string Code { get; set; } = "";
    public decimal ListPrice { get; set; }
}

public static class MiniDapper
{
    public static List<T> MiniQuery<T>(this IDbConnection conn, string sql) where T : new()
    {
        bool wasClosed = conn.State == ConnectionState.Closed;
        if (wasClosed) conn.Open();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            using var reader = cmd.ExecuteReader();

            var props = new PropertyInfo?[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
            {
                props[i] = typeof(T).GetProperty(reader.GetName(i),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                // 新增：找列对应属性只做一次，所以在这里报告，不会每行重复打印
                if (props[i] is null) Console.WriteLine($"跳过列：{reader.GetName(i)}");
            }

            var list = new List<T>();
            while (reader.Read())
            {
                var obj = new T();
                for (int i = 0; i < props.Length; i++)
                {
                    var prop = props[i];
                    if (prop is null || !prop.CanWrite || reader.IsDBNull(i)) continue;
                    prop.SetValue(obj, reader.GetValue(i));
                }
                list.Add(obj);
            }
            return list;
        }
        finally
        {
            if (wasClosed) conn.Close();
        }
    }
}
