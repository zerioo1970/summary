// code/ch19/Step1-Debugger/Program.cs
// 在调试器里能看到哪些参数：匿名对象、DynamicParameters（执行前 / 执行后）、IN 列表
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

const string sql =
    "SELECT RequestNo FROM dbo.PriceRequest WHERE Applicant = @Applicant AND Status >= @MinStatus ORDER BY Id";

// 例 1：匿名对象。断点停在 Query 这一行时，「局部变量」窗口里展开 p1，就能看到两个属性的值
var p1 = new { Applicant = "张伟", MinStatus = 0 };
var r1 = conn.Query<string>(sql, p1).ToList();                       // ← 断点 1
Console.WriteLine($"例 1 匿名对象 {p1} → {string.Join("、", r1)}");

// 例 2：DynamicParameters 用 Add 加参数。还没执行，名字和值就已经在里面了
var p2 = new DynamicParameters();
p2.Add("Applicant", "张伟");
p2.Add("MinStatus", 1);
Console.WriteLine($"例 2 执行前 ParameterNames = [{string.Join(", ", p2.ParameterNames)}]");
foreach (var name in p2.ParameterNames)
    Console.WriteLine($"     @{name} = {p2.Get<object>(name)}");      // 「监视」窗口里也可以输入 p2.ParameterNames
var r2 = conn.Query<string>(sql, p2).ToList();                       // ← 断点 2
Console.WriteLine($"     结果 → {string.Join("、", r2)}");

// 例 3：把匿名对象交给 DynamicParameters 的构造函数。执行前 ParameterNames 是空的！
var p3 = new DynamicParameters(new { Applicant = "李娜", MinStatus = 0 });
Console.WriteLine($"例 3 执行前 ParameterNames = [{string.Join(", ", p3.ParameterNames)}]");
var r3 = conn.Query<string>(sql, p3).ToList();                       // ← 断点 3
Console.WriteLine($"     执行后 ParameterNames = [{string.Join(", ", p3.ParameterNames)}]  结果 → {string.Join("、", r3)}");

// 例 4：IN 列表。C# 这边看到的只是一个数组；它在发给服务器之前被展开成几个参数——这一步调试器看不到
var p4 = new { Ids = new[] { 1, 3 } };
var r4 = conn.Query<string>("SELECT RequestNo FROM dbo.PriceRequest WHERE Id IN @Ids ORDER BY Id", p4).ToList();   // ← 断点 4
Console.WriteLine($"例 4 Ids 在 C# 里是 {p4.Ids.GetType().Name}，长度 {p4.Ids.Length} → {string.Join("、", r4)}");
