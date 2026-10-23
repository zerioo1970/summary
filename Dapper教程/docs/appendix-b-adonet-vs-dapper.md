# 附录 B ADO.NET ↔ Dapper 写法对照

用途：改造老项目时，把一段熟悉的 ADO.NET 代码对照翻译成 Dapper 写法；每项左右两段做的是同一件事。

## 怎么用这些片段

- 每段都是**方法体片段**，放进第 2 章那样的控制台项目（顶级语句）里就能运行，包与 csproj 同[第 2 章](ch02-first-query.md) 2.1 节。一次放一段；想同时放几段，就给每段各包一对 `{ }`，变量名才不会冲突（验证时就是这样做的）。
- 所有片段共用下面的文件头和两个类。文件头放在 `Program.cs` 最上面，类放在文件末尾。
- 片段会改数据，想让结果和初始数据一致，每次运行前重跑 `code/sql/00-reset-database.sql`。第 7、12 项另需执行文中注明的脚本。
- 全部 26 段都在沙箱里实测编译、运行通过（沙箱把连接串换成了 SQL 身份验证，其余一字不差）。

```csharp
// 片段共用的文件头
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";
```

```csharp
// 片段共用的类（放在 Program.cs 末尾）
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
    public string Level { get; set; } = "";
}

public class RequestBrief
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
    public byte Status { get; set; }   // 列是 TINYINT
}
```

## 目录

| # | 对照项 | # | 对照项 |
|---|---|---|---|
| 1 | 查多行 | 8 | 事务 |
| 2 | 查一行（查不到返回 null，多行报错） | 9 | 多结果集（当前页 + 总数） |
| 3 | 取单值（可能是 NULL） | 10 | 命令超时 |
| 4 | 增删改 + 受影响行数 | 11 | 取消（CancellationToken） |
| 5 | 插入后取自增 Id | 12 | 表值参数（TVP） |
| 6 | 参数（含 IN 列表） | 13 | 连接管理（打开与关闭） |
| 7 | 存储过程：Output 参数与 ReturnValue |  |  |

---

## 1. 查多行

**ADO.NET**

```csharp
var list = new List<Customer>();
using var conn = new SqlConnection(connStr);
conn.Open();
using var cmd = new SqlCommand("SELECT Id, Name, City, Level FROM dbo.Customer ORDER BY Id", conn);
using var reader = cmd.ExecuteReader();
while (reader.Read())
{
    list.Add(new Customer
    {
        Id    = reader.GetInt32(reader.GetOrdinal("Id")),      // 每列手写一次列名和类型
        Name  = reader.GetString(reader.GetOrdinal("Name")),
        City  = reader.GetString(reader.GetOrdinal("City")),
        Level = reader.GetString(reader.GetOrdinal("Level")),
    });
}
Console.WriteLine($"ADO.NET：{list.Count} 个客户");
```

**Dapper**

```csharp
using var conn = new SqlConnection(connStr);
// 不用 Open；列名按名字（不分大小写）对上属性
List<Customer> list = conn.Query<Customer>(
    "SELECT Id, Name, City, Level FROM dbo.Customer ORDER BY Id").ToList();
Console.WriteLine($"Dapper：{list.Count} 个客户");
```

**差在哪**：映射和开关连接由 Dapper 做；但列名写错时 ADO.NET 报 `IndexOutOfRangeException`，Dapper **不报错**、属性留默认值。详见[第 1 章](ch01-setup.md)、[第 2 章](ch02-first-query.md)。

---

## 2. 查一行（查不到返回 null，多行报错）

**ADO.NET**

```csharp
RequestBrief? r = null;
using var conn = new SqlConnection(connStr);
conn.Open();
using var cmd = new SqlCommand(
    "SELECT Id, RequestNo, Applicant, Status FROM dbo.PriceRequest WHERE RequestNo = @No", conn);
cmd.Parameters.Add("@No", SqlDbType.VarChar, 20).Value = "PR20261008";
using var reader = cmd.ExecuteReader();
if (reader.Read())
{
    r = new RequestBrief
    {
        Id = reader.GetInt32(0),
        RequestNo = reader.GetString(1),
        Applicant = reader.GetString(2),
        Status = reader.GetByte(3),
    };
    if (reader.Read()) throw new InvalidOperationException("单号不唯一");   // 自己检查"多行"
}
Console.WriteLine(r is null ? "ADO.NET：查不到" : $"ADO.NET：{r.RequestNo} {r.Applicant}");
```

**Dapper**

```csharp
using var conn = new SqlConnection(connStr);
// 0 行返回 null；多行抛 Sequence contains more than one element
RequestBrief? r = conn.QuerySingleOrDefault<RequestBrief>(
    "SELECT Id, RequestNo, Applicant, Status FROM dbo.PriceRequest WHERE RequestNo = @No",
    new { No = "PR20261008" });
Console.WriteLine(r is null ? "Dapper：查不到" : $"Dapper：{r.RequestNo} {r.Applicant}");
```

**差在哪**：0 行 / 多行的判断变成选方法（First / Single / OrDefault）。另一处差别：Dapper 把 `string` 参数发成 `nvarchar(4000)`，左边手写的是 `varchar(20)`，要一致就用 `DbString`。详见[第 5 章](ch05-single-row.md)、[第 15 章](ch15-performance.md)。

---

## 3. 取单值（可能是 NULL）

**ADO.NET**

```csharp
using var conn = new SqlConnection(connStr);
conn.Open();
using var cmd = new SqlCommand(
    "SELECT SUM(Qty * RequestPrice) FROM dbo.PriceRequestItem WHERE RequestId = @Id", conn);
cmd.Parameters.Add("@Id", SqlDbType.Int).Value = 99;
object? result = cmd.ExecuteScalar();
decimal? total = result is null || result == DBNull.Value ? null : (decimal)result;   // 自己判断 DBNull
Console.WriteLine($"ADO.NET：{total?.ToString("N2") ?? "没有明细"}");
```

**Dapper**

```csharp
using var conn = new SqlConnection(connStr);
// T 写成 decimal?：NULL 直接得到 null
decimal? total = conn.ExecuteScalar<decimal?>(
    "SELECT SUM(Qty * RequestPrice) FROM dbo.PriceRequestItem WHERE RequestId = @Id",
    new { Id = 99 });
Console.WriteLine($"Dapper：{total?.ToString("N2") ?? "没有明细"}");
```

**差在哪**：ADO.NET 用 `(decimal)` 强转 `DBNull` 会抛 `InvalidCastException`；Dapper 用 `decimal` 接 NULL **不报错、返回 0**，所以要写 `decimal?`。详见[第 5 章](ch05-single-row.md)。

---

## 4. 增删改 + 受影响行数

**ADO.NET**

```csharp
using var conn = new SqlConnection(connStr);
conn.Open();
using var cmd = new SqlCommand(
    "UPDATE dbo.PriceRequest SET Status = 1 WHERE RequestNo = @No AND Status = 0", conn);
cmd.Parameters.Add("@No", SqlDbType.VarChar, 20).Value = "PR20261015";
int rows = cmd.ExecuteNonQuery();
Console.WriteLine(rows == 0 ? "ADO.NET：没命中" : $"ADO.NET：改了 {rows} 行");
```

**Dapper**

```csharp
using var conn = new SqlConnection(connStr);
int rows = conn.Execute(
    "UPDATE dbo.PriceRequest SET Status = 1 WHERE RequestNo = @No AND Status = 0",
    new { No = "PR20261015" });
Console.WriteLine(rows == 0 ? "Dapper：没命中" : $"Dapper：改了 {rows} 行");
```

**差在哪**：返回值含义完全相同：0 行不抛异常，要自己判断；SQL 里有 `SET NOCOUNT ON` 时两边都返回 -1。详见[第 6 章](ch06-insert-update-delete.md)。

---

## 5. 插入后取自增 Id

**ADO.NET**

```csharp
using var conn = new SqlConnection(connStr);
conn.Open();
using var cmd = new SqlCommand(@"
    INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant)
    OUTPUT INSERTED.Id
    VALUES (@No, @CustomerId, @Applicant)", conn);
cmd.Parameters.Add("@No", SqlDbType.VarChar, 20).Value = "PR20261101";
cmd.Parameters.Add("@CustomerId", SqlDbType.Int).Value = 2;
cmd.Parameters.Add("@Applicant", SqlDbType.NVarChar, 20).Value = "李娜";
int newId = (int)cmd.ExecuteScalar()!;
Console.WriteLine($"ADO.NET：新 Id = {newId}");
```

**Dapper**

```csharp
using var conn = new SqlConnection(connStr);
// OUTPUT 返回的是结果集，要用"读"的方法接；用 Execute 只会拿到受影响行数 1
int newId = conn.ExecuteScalar<int>(@"
    INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant)
    OUTPUT INSERTED.Id
    VALUES (@No, @CustomerId, @Applicant)",
    new { No = "PR20261102", CustomerId = 2, Applicant = "李娜" });
Console.WriteLine($"Dapper：新 Id = {newId}");
```

**差在哪**：SQL 一样，只是不用强转。两段用了不同单号，因为 `RequestNo` 有唯一约束。表上有触发器时 `OUTPUT` 要加 `INTO`（错误 334）。详见[第 6 章](ch06-insert-update-delete.md)。

---

## 6. 参数（含 IN 列表）

**ADO.NET**

```csharp
int[] ids = { 1, 3 };
var nos = new List<string>();
using var conn = new SqlConnection(connStr);
conn.Open();
using var cmd = new SqlCommand { Connection = conn };
var names = new List<string>();
for (int i = 0; i < ids.Length; i++)
{
    names.Add("@p" + i);                                       // 只拼占位符，值仍然走参数
    cmd.Parameters.Add("@p" + i, SqlDbType.Int).Value = ids[i];
}
cmd.Parameters.Add("@Applicant", SqlDbType.NVarChar, 20).Value = "张伟";
cmd.CommandText = "SELECT RequestNo FROM dbo.PriceRequest WHERE Applicant = @Applicant"
                + " AND Id IN (" + string.Join(",", names) + ") ORDER BY Id";   // ids 为空时是 IN ()，语法错误
using var reader = cmd.ExecuteReader();
while (reader.Read()) nos.Add(reader.GetString(0));
Console.WriteLine("ADO.NET：" + string.Join("、", nos));
```

**Dapper**

```csharp
int[] ids = { 1, 3 };
using var conn = new SqlConnection(connStr);
// IN @Ids 不写括号：Dapper 展开成 IN (@Ids1,@Ids2)，括号由它加
List<string> nos = conn.Query<string>(
    "SELECT RequestNo FROM dbo.PriceRequest WHERE Applicant = @Applicant AND Id IN @Ids ORDER BY Id",
    new { Applicant = "张伟", Ids = ids }).ToList();
Console.WriteLine("Dapper：" + string.Join("、", nos));
```

**差在哪**：属性名就是参数名（不带 `@`）。写成 `IN (@Ids)` 报 `Incorrect syntax near ','`；空列表被改写成永假子查询，`NOT IN` 空列表会返回全部行。详见[第 4 章](ch04-parameters.md)。

---

## 7. 存储过程：Output 参数与 ReturnValue

**ADO.NET**

```csharp
using var conn = new SqlConnection(connStr);
conn.Open();
using var cmd = new SqlCommand("dbo.Update_PriceRequestSubmit_Proc", conn);
cmd.CommandType = CommandType.StoredProcedure;
cmd.Parameters.Add("@RequestNo", SqlDbType.VarChar, 20).Value = "PR20261015";
cmd.Parameters.Add("@Operator", SqlDbType.NVarChar, 20).Value = "张伟";
var pStatus = cmd.Parameters.Add("@NewStatus", SqlDbType.TinyInt);
pStatus.Direction = ParameterDirection.Output;
var pReturn = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
pReturn.Direction = ParameterDirection.ReturnValue;
cmd.ExecuteNonQuery();
int ret = (int)pReturn.Value;
byte? status = pStatus.Value == DBNull.Value ? null : (byte)pStatus.Value;
Console.WriteLine($"ADO.NET：返回值 {ret}，新状态 {status?.ToString() ?? "NULL"}");
```

**Dapper**

```csharp
using var conn = new SqlConnection(connStr);
var p = new DynamicParameters(new { RequestNo = "PR20261015", Operator = "张伟" });   // 输入参数
p.Add("@NewStatus", dbType: DbType.Byte, direction: ParameterDirection.Output);
p.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
conn.Execute("dbo.Update_PriceRequestSubmit_Proc", p, commandType: CommandType.StoredProcedure);
int ret = p.Get<int>("@ReturnValue");           // 执行之后才能 Get
byte? status = p.Get<byte?>("@NewStatus");       // 可能是 NULL，用 byte?
Console.WriteLine($"Dapper：返回值 {ret}，新状态 {status?.ToString() ?? "NULL"}");
```

**差在哪**：一个 Output 参数从两行变一行；`DbType` 代替 `SqlDbType`。忘写 `direction` 拿到的永远是 NULL；`commandType` 一律显式写。在初始数据上先后执行两段：左边返回 0，右边返回 1（单据已是待审）。运行前先执行 `code/sql/ch08-procs.sql`。详见[第 8 章](ch08-stored-procedures.md)。

---

## 8. 事务

**ADO.NET**

```csharp
using var conn = new SqlConnection(connStr);
conn.Open();
using var tran = conn.BeginTransaction();
try
{
    using var cmd1 = new SqlCommand(
        "UPDATE dbo.PriceRequest SET Status = 1 WHERE Id = 3 AND Status = 0", conn, tran);
    cmd1.ExecuteNonQuery();
    using var cmd2 = new SqlCommand(
        "INSERT dbo.ApprovalLog (RequestId, Approver, Action) VALUES (3, @Approver, N'提交')", conn, tran);
    cmd2.Parameters.Add("@Approver", SqlDbType.NVarChar, 20).Value = "张伟";
    cmd2.ExecuteNonQuery();
    tran.Commit();
    Console.WriteLine("ADO.NET：已提交");
}
catch (SqlException ex)
{
    tran.Rollback();
    Console.WriteLine($"ADO.NET：出错 {ex.Number}，已回滚");
}
```

**Dapper**

```csharp
using var conn = new SqlConnection(connStr);
conn.Open();                                     // BeginTransaction 前必须手动 Open
using var tran = conn.BeginTransaction();
try
{
    conn.Execute("UPDATE dbo.PriceRequest SET Status = 1 WHERE Id = 3 AND Status = 0",
        transaction: tran);                      // 每条命令都要传，读操作也一样
    conn.Execute("INSERT dbo.ApprovalLog (RequestId, Approver, Action) VALUES (3, @Approver, N'提交')",
        new { Approver = "张伟" }, transaction: tran);
    tran.Commit();
    Console.WriteLine("Dapper：已提交");
}
catch (SqlException ex)
{
    tran.Rollback();
    Console.WriteLine($"Dapper：出错 {ex.Number}，已回滚");
}
```

**差在哪**：事务对象完全是 ADO.NET 的；`cmd.Transaction = tran` 变成参数 `transaction: tran`。漏传一条就报 `…requires the command to have a transaction…`；用命名参数，别按位置传。详见[第 9 章](ch09-transactions.md)。

---

## 9. 多结果集（当前页 + 总数）

**ADO.NET**

```csharp
var rows = new List<RequestBrief>();
int total;
using var conn = new SqlConnection(connStr);
conn.Open();
using var cmd = new SqlCommand(@"
    SELECT Id, RequestNo, Applicant, Status FROM dbo.PriceRequest
    ORDER BY Id OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
    SELECT COUNT(*) FROM dbo.PriceRequest;", conn);
cmd.Parameters.Add("@Skip", SqlDbType.Int).Value = 0;
cmd.Parameters.Add("@Take", SqlDbType.Int).Value = 2;
using (var reader = cmd.ExecuteReader())
{
    while (reader.Read())
        rows.Add(new RequestBrief
        {
            Id = reader.GetInt32(0),
            RequestNo = reader.GetString(1),
            Applicant = reader.GetString(2),
            Status = reader.GetByte(3),
        });
    reader.NextResult();                         // 手动切到第 2 个结果集
    reader.Read();
    total = reader.GetInt32(0);
}
Console.WriteLine($"ADO.NET：本页 {rows.Count} 条，共 {total} 条");
```

**Dapper**

```csharp
using var conn = new SqlConnection(connStr);
using var grid = conn.QueryMultiple(@"
    SELECT Id, RequestNo, Applicant, Status FROM dbo.PriceRequest
    ORDER BY Id OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
    SELECT COUNT(*) FROM dbo.PriceRequest;", new { Skip = 0, Take = 2 });
List<RequestBrief> rows = grid.Read<RequestBrief>().ToList();   // 第 1 个结果集
int total = grid.ReadSingle<int>();                             // 第 2 个结果集
Console.WriteLine($"Dapper：本页 {rows.Count} 条，共 {total} 条");
```

**差在哪**：`NextResult()` 变成"下一次 Read 自动切换"。Read 的顺序只认第几次、不认类型：读反了**不报错**，属性全是默认值；多读一次报 `ObjectDisposedException`。详见[第 12 章](ch12-query-multiple.md)。

---

## 10. 命令超时

**ADO.NET**

```csharp
using var conn = new SqlConnection(connStr);
conn.Open();
using var cmd = new SqlCommand("WAITFOR DELAY '00:00:05'; SELECT COUNT(*) FROM dbo.PriceRequest;", conn);
cmd.CommandTimeout = 2;                          // 不写就是 SqlCommand 默认的 30 秒
try
{
    cmd.ExecuteScalar();
}
catch (SqlException ex) when (ex.Number == -2)
{
    Console.WriteLine("ADO.NET 超时：" + ex.Message);
}
```

**Dapper**

```csharp
using var conn = new SqlConnection(connStr);
try
{
    conn.ExecuteScalar<int>("WAITFOR DELAY '00:00:05'; SELECT COUNT(*) FROM dbo.PriceRequest;",
        commandTimeout: 2);                      // 不传就沿用 SqlCommand 的 30 秒
}
catch (SqlException ex) when (ex.Number == -2)
{
    Console.WriteLine("Dapper 超时：" + ex.Message);
}
```

**差在哪**：30 秒是 ADO.NET 的默认值，Dapper 不改它；全局改用连接串 `Command Timeout=` 或 `SqlMapper.Settings.CommandTimeout`。`0` 表示永不超时。详见[第 14 章](ch14-async-timeout-cancel.md)。

---

## 11. 取消（CancellationToken）

**ADO.NET**

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
await using var conn = new SqlConnection(connStr);
await conn.OpenAsync();
using var cmd = new SqlCommand("WAITFOR DELAY '00:00:05'; SELECT 1;", conn);
try
{
    await cmd.ExecuteScalarAsync(cts.Token);     // 令牌直接传给 ExecuteXxxAsync
}
catch (Exception ex) when (cts.IsCancellationRequested)
{
    Console.WriteLine("ADO.NET 已取消：" + ex.GetType().Name);
}
```

**Dapper**

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
await using var conn = new SqlConnection(connStr);
// 普通的 QueryAsync(sql, param) 没有令牌参数，要先装进 CommandDefinition
var command = new CommandDefinition("WAITFOR DELAY '00:00:05'; SELECT 1;", cancellationToken: cts.Token);
try
{
    await conn.ExecuteScalarAsync<int>(command);
}
catch (Exception ex) when (cts.IsCancellationRequested)
{
    Console.WriteLine("Dapper 已取消：" + ex.GetType().Name);
}
```

**差在哪**：Dapper 多一步 `CommandDefinition`。执行中取消抛 `SqlException`，发出前已取消抛 `TaskCanceledException`，所以用 `when (cts.IsCancellationRequested)` 判断。详见[第 14 章](ch14-async-timeout-cancel.md)。

---

## 12. 表值参数（TVP）

**ADO.NET**

```csharp
var table = new DataTable();
table.Columns.Add("RequestId", typeof(int));     // 列顺序必须和 CREATE TYPE 一致
table.Columns.Add("ProductId", typeof(int));
table.Columns.Add("Qty", typeof(int));
table.Columns.Add("RequestPrice", typeof(decimal));
table.Rows.Add(3, 3, 1, 24000m);
table.Rows.Add(3, 4, 2, 22000m);

using var conn = new SqlConnection(connStr);
conn.Open();
using var cmd = new SqlCommand(@"
    INSERT dbo.PriceRequestItem (RequestId, ProductId, Qty, RequestPrice)
    SELECT RequestId, ProductId, Qty, RequestPrice FROM @Items", conn);
var p = cmd.Parameters.AddWithValue("@Items", table);
p.SqlDbType = SqlDbType.Structured;
p.TypeName = "dbo.PriceRequestItemType";
int rows = cmd.ExecuteNonQuery();
Console.WriteLine($"ADO.NET：插入 {rows} 行");
```

**Dapper**

```csharp
var table = new DataTable();
table.Columns.Add("RequestId", typeof(int));     // 列顺序必须和 CREATE TYPE 一致
table.Columns.Add("ProductId", typeof(int));
table.Columns.Add("Qty", typeof(int));
table.Columns.Add("RequestPrice", typeof(decimal));
table.Rows.Add(3, 3, 1, 24000m);
table.Rows.Add(3, 4, 2, 22000m);

using var conn = new SqlConnection(connStr);
int rows = conn.Execute(@"
    INSERT dbo.PriceRequestItem (RequestId, ProductId, Qty, RequestPrice)
    SELECT RequestId, ProductId, Qty, RequestPrice FROM @Items",
    new { Items = table.AsTableValuedParameter("dbo.PriceRequestItemType") });
Console.WriteLine($"Dapper：插入 {rows} 行");
```

**差在哪**：三行参数设置变成一个 `AsTableValuedParameter("架构.类型名")`；`DataTable` 的准备完全一样。列顺序错了**不报错、数据错位**；类型名错报 351。运行前先执行 `code/sql/ch15-tvp.sql`。详见[第 15 章](ch15-performance.md)。

---

## 13. 连接管理（打开与关闭）

**ADO.NET**

```csharp
using var conn = new SqlConnection(connStr);   // using：离开作用域自动关闭，物理连接还回连接池
conn.Open();                                   // 必须先 Open
using var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Customer", conn);
int count = (int)cmd.ExecuteScalar()!;
Console.WriteLine($"ADO.NET：{count} 个客户，查完连接 {conn.State}");
```

**Dapper**

```csharp
using var conn = new SqlConnection(connStr);   // 仍然要 using：Dapper 只关它自己打开的连接
int count = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.Customer");   // 关着就自动打开、用完关上
Console.WriteLine($"Dapper：{count} 个客户，查完连接 {conn.State}");
```

**差在哪**：连接池是 SqlClient 的，两边一样；Dapper 只是"关着就开、自己开的自己关"。左边输出 `Open`、右边 `Closed`。事务、`buffered: false`、`QueryMultiple` 期间连接一直被占着。正式项目把连接串收进工厂：见[第 16 章](ch16-connection-and-repository.md)；原理见[第 3 章](ch03-how-it-works.md)。

---

## 一张表看完

| 对照项 | ADO.NET 要你写的 | Dapper 替你做的 | Dapper 不替你做的 |
|---|---|---|---|
| 查多行 / 一行 | `ExecuteReader` + `while` + 每列 `GetXxx` | 按列名映射、开关连接 | 核对列名（写错不报错） |
| 单值 | 强转 + 判断 `DBNull` | 转成 `T`，NULL 变 `default(T)` | 选可空类型 |
| 增删改 | `ExecuteNonQuery` | 同样返回行数 | 判断 0 行、`NOCOUNT` 的 -1 |
| 参数 | 每个参数一行 `Parameters.Add` | 按属性生成参数、展开 `IN` 列表 | varchar 列用 `DbString` |
| 存储过程 | `Add` + `Direction` + 读 `Value` | `DynamicParameters.Add` / `Get` | 执行后再 `Get` |
| 事务 | `cmd.Transaction = tran` | `transaction: tran` 参数 | 每条命令都传、手动 `Open` |
| 多结果集 | `NextResult()` | `QueryMultiple` + `Read` | 保证 Read 顺序 |
| 超时 / 取消 | `CommandTimeout`、`ExecuteXxxAsync(token)` | `commandTimeout:`、`CommandDefinition` | 令牌一路传到底 |
| 表值参数 | `Structured` + `TypeName` | `AsTableValuedParameter` | `DataTable` 列顺序 |

[返回目录](../README.md)
