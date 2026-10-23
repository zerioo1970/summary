# 附录 A Dapper 常用 API 速查表

用途：忘了某个方法叫什么、返回什么、查不到时怎样，翻这一页；每一条都是本教程正文里实测过的行为，细节点"详见"跳回对应章节。

> 基线：`Dapper 2.1.89`。所有方法都是 `IDbConnection` 上的扩展方法，文件顶部必须有 `using Dapper;`，否则报 `CS1061 … does not contain a definition for 'Query'`（[第 2 章](ch02-first-query.md)）。`CommandType`、`DbType`、`ParameterDirection` 在 `System.Data` 里，ImplicitUsings 不包含它，要自己 `using System.Data;`（[第 8 章](ch08-stored-procedures.md)）。

## 0. 几乎每个方法都有的可选参数

| 参数 | 默认 | 作用 | 详见 |
|---|---|---|---|
| `param` | `null` | 参数对象：匿名对象、实体对象、`DynamicParameters` | [第 4 章](ch04-parameters.md) |
| `transaction` | `null` | 事务进行中**每条**命令都要传，读操作也一样 | [第 9 章](ch09-transactions.md) |
| `commandTimeout` | `null`（沿用 `SqlCommand` 的 30 秒） | 命令最多等几秒；`0` 表示永不超时 | [第 14 章](ch14-async-timeout-cancel.md) |
| `commandType` | `null`（Dapper 会猜） | 调存储过程时显式写 `CommandType.StoredProcedure` | [第 8 章](ch08-stored-procedures.md) |
| `buffered` | `true` | 只有 `Query` 系列有。`false` 为流式读取 | [第 3 章](ch03-how-it-works.md)、[第 15 章](ch15-performance.md) |

参数顺序是 `(sql, param, transaction, …)`。写成 `conn.Execute(sql, tran)` 时 `tran` 会被当成 `param`，所以 `transaction`、`commandTimeout`、`commandType` 一律用**命名参数**写（[第 9 章](ch09-transactions.md)实测）。

## 1. 查询

| API | 签名要点 | 返回 | 0 行 / 多行 | 最小示例 | 详见 |
|---|---|---|---|---|---|
| `Query<T>` | `(sql, param, transaction, buffered = true, commandTimeout, commandType)` | `IEnumerable<T>`；缓冲模式下实际是已读完的 `List<T>` | 0 行：**空集合，不是 null**；多行：全部 | `conn.Query<Customer>("SELECT Id, Name, City, Level FROM dbo.Customer")` | [第 2 章](ch02-first-query.md) |
| `Query<T>`，T 是简单类型 | T 为 `string`、`int`、`decimal` 等 | 只取**第 1 列**，不按列名匹配 | 同上 | `conn.Query<string>("SELECT City FROM dbo.Customer")` | [第 2 章](ch02-first-query.md) |
| `Query<T>(…, buffered: false)` | 流式读取 | 一个"还没开始读"的序列 | 遍历期间连接一直 `Open`；同一连接上不能再执行命令；只能遍历一次 | `foreach (var r in conn.Query<ReportRow>(sql, buffered: false))` | [第 3 章](ch03-how-it-works.md)、[第 15 章](ch15-performance.md) |
| `Query`（不带 T） | 返回 dynamic 行，用 `行.列名` 取值 | `IEnumerable<dynamic>` | 同 `Query<T>` | `foreach (var g in conn.Query("SELECT ProductId, COUNT(*) AS Cnt FROM dbo.PriceRequestItem GROUP BY ProductId"))` | 只在[第 15 章](ch15-performance.md) 15.4 节的核对代码里出现，正文没有展开 |

**映射规则速记**（[第 2 章](ch02-first-query.md)、[第 7 章](ch07-mapping-issues.md)）：

| 情况 | 行为 | 报错吗 |
|---|---|---|
| 列名和属性名只差大小写 | 照样对上 | 不报 |
| 结果集多出列 / 类里多出属性 | 多的列跳过；多的属性保持默认值 | **不报** |
| 列名 `request_no`、属性 `RequestNo` | 对不上；起别名或开 `MatchNamesWithUnderscores` | 不报 |
| 列值 NULL 进非可空属性（`int`、`decimal`） | 跳过赋值，保持默认值或初始值 | 不报 |
| 只取一列的 `Query<int>` / `QuerySingle<decimal>` 遇到 NULL | `Error parsing column 0 ((Unnamed Column)=n/a - Null object cannot be converted to a value type.)` | 报 |
| 类型不同但能转（TINYINT → `int`、`DECIMAL` → `double`） | 自动转换 | 不报 |
| 转不了（"上海" → `int`） | `DataException: Error parsing column N (列名=值 - 类型)` | 报 |
| record / 只有带参构造函数的类 | 按构造函数映射，**类型必须严格一致**（`byte` ≠ `int`） | 报 `A parameterless default constructor or one matching signature …` |

## 2. 单行与单值

| API | 签名要点 | 返回 | 0 行 / 多行 | 最小示例 | 详见 |
|---|---|---|---|---|---|
| `QueryFirst<T>` | 同 `Query<T>`，无 `buffered` | `T` | 0 行：`InvalidOperationException: Sequence contains no elements`；多行：第一行 | `conn.QueryFirst<RequestBrief>("SELECT TOP 1 Id, RequestNo, Applicant, Status FROM dbo.PriceRequest ORDER BY CreatedAt DESC")` | [第 5 章](ch05-single-row.md) |
| `QueryFirstOrDefault<T>` | 同上 | `T?` | 0 行：`default(T)`；多行：第一行 | `conn.QueryFirstOrDefault<RequestBrief>(sql, new { No = no })` | [第 5 章](ch05-single-row.md) |
| `QuerySingle<T>` | 同上 | `T` | 0 行：`Sequence contains no elements`；多行：`Sequence contains more than one element` | `conn.QuerySingle<int>("INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant) OUTPUT INSERTED.Id VALUES (@RequestNo, @CustomerId, @Applicant)", req)` | [第 5 章](ch05-single-row.md) |
| `QuerySingleOrDefault<T>` | 同上 | `T?` | 0 行：`default(T)`；**多行照样抛** `more than one element` | `conn.QuerySingleOrDefault<byte?>("SELECT Status FROM dbo.PriceRequest WHERE RequestNo = @No", new { No = no })` | [第 5 章](ch05-single-row.md) |
| `ExecuteScalar<T>` | `(sql, param, transaction, commandTimeout, commandType)` | 第一行第一列，用 `Convert.ChangeType` 转成 `T` | 0 行或值为 NULL：`default(T)`（`decimal` 得 0）；多行多列：只取第一格，**不报错** | `conn.ExecuteScalar<decimal?>("SELECT SUM(Qty * RequestPrice) FROM dbo.PriceRequestItem WHERE RequestId = @Id", new { Id = 99 })` | [第 5 章](ch05-single-row.md) |

| `default(T)` | 值 | 陷阱 |
|---|---|---|
| 类、`string` | `null` | 不判断就用会 `NullReferenceException`，编译时有 CS8602 警告 |
| `int`、`byte`、`decimal` | `0` | 查不到和"值就是 0"（草稿状态、金额 0）分不开 → **T 写成可空类型** |
| `int?`、`decimal?` | `null` | 推荐 |

`ExecuteScalar<T>` 转换失败时只报值、不报列名：`FormatException: The input string 'PR20261015' was not in a correct format.`；`decimal` 转 `int` 不报错但会舍入（[第 5 章](ch05-single-row.md)实测）。

## 3. 写入

| API | 签名要点 | 返回 | 0 行 / 多行 | 最小示例 | 详见 |
|---|---|---|---|---|---|
| `Execute` | `(sql, param, transaction, commandTimeout, commandType)` | `int`：受影响行数 | 0 行：返回 0，**不抛异常**；SQL 或过程里有 `SET NOCOUNT ON`：返回 -1 | `conn.Execute("UPDATE dbo.PriceRequest SET Status = 1 WHERE RequestNo = @No AND Status = 0", new { No = no })` | [第 6 章](ch06-insert-update-delete.md) |
| `Execute`，param 是集合 | 传 `List<T>` / 数组 | 每次受影响行数的**总和** | 对每个元素执行一次，**不是批量**；不在事务里时中途失败会留下前面的数据 | `conn.Execute("INSERT dbo.PriceRequestItem (RequestId, ProductId, Qty, RequestPrice) VALUES (@RequestId, @ProductId, @Qty, @RequestPrice)", items)` | [第 6 章](ch06-insert-update-delete.md) |
| `OUTPUT INSERTED.Id` + `ExecuteScalar<int>` | SQL 写法，不是 API | 新 Id | 用 `Execute` 接只得到行数 1 | `int id = conn.ExecuteScalar<int>("INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant) OUTPUT INSERTED.Id VALUES (@RequestNo, @CustomerId, @Applicant)", req)` | [第 6 章](ch06-insert-update-delete.md) |
| `OUTPUT INSERTED.*` + `QuerySingle<T>` / `Query<int>` | 一行拿多列 / 多行拿多个 Id | 对象 / Id 列表 | 表上有触发器时报 334，改用 `OUTPUT … INTO @表变量` | `conn.Query<int>("INSERT dbo.PriceRequestItem (RequestId, ProductId, Qty, RequestPrice) OUTPUT INSERTED.Id VALUES (@Id, 3, 1, 24000), (@Id, 4, 1, 22000)", new { Id = id })` | [第 6 章](ch06-insert-update-delete.md) |

只有 `Execute` 支持"传集合逐条执行"；把集合传给 `Query` 系列报 `An enumerable sequence of parameters (arrays, lists, etc) is not allowed in this context`（[第 6 章](ch06-insert-update-delete.md)）。

## 4. 参数

| API / 写法 | 签名要点 | 结果 | 注意 | 最小示例 | 详见 |
|---|---|---|---|---|---|
| 匿名对象 / 实体对象 | 属性名对 SQL 里的 `@名字`，不带 `@` | 每个用到的属性生成一个参数 | 普通 SQL 只发 SQL 里出现过的 `@名字`；**存储过程会把每个属性都发过去**（多了报 8144） | `new { Applicant = "张伟", Status = 2 }` | [第 4 章](ch04-parameters.md)、[第 8 章](ch08-stored-procedures.md) |
| 集合参数 `IN @Ids` | 参数值是数组 / `List<T>` | 展开成 `IN (@Ids1,@Ids2)` | **不写括号**；空列表改写成 `IN (SELECT @Ids WHERE 1 = 0)`，`NOT IN` 空列表返回全部；元素多受 SQL Server 2100 个参数上限限制 | `new { Ids = new[] { 1, 3 } }` | [第 4 章](ch04-parameters.md)、[第 11 章](ch11-one-to-many.md) |
| 类型推断 | 按 C# 类型 | `string` → `nvarchar(4000)`、`int` → `int`、`long` → `bigint`、`bool` → `bit`、枚举 → 底层数字、`DateOnly` → `date` | 同一列在不同地方传 `int` 和 `long`，服务器上是两个计划 | — | [第 4 章](ch04-parameters.md)、[第 7 章](ch07-mapping-issues.md)、[第 8 章](ch08-stored-procedures.md) |
| `new DynamicParameters()` / `new DynamicParameters(匿名对象)` | 参数袋 | 可当 `param` 传给任何方法 | 构造时传入的对象，**执行后**才拆成参数 | `var p = new DynamicParameters(new { RequestNo = no });` | [第 8 章](ch08-stored-procedures.md)、[第 19 章](ch19-see-the-sql.md) |
| `DynamicParameters.Add` | `(name, value = null, dbType = null, direction = null, size = null)` | — | 名字带不带 `@` 都行；Output 不给值，给 `dbType` 和 `direction`；忘写 `direction` 拿到的永远是 NULL | `p.Add("@NewStatus", dbType: DbType.Byte, direction: ParameterDirection.Output);` | [第 8 章](ch08-stored-procedures.md)、[第 13 章](ch13-paging-dynamic-where.md) |
| `DynamicParameters.Get<T>` | `(name)` | 参数值 | **执行后**才有值；执行前取得 null；NULL 用不可空 T 取报 `Attempting to cast a DBNull to a non nullable type!`；名字拼错报 `KeyNotFoundException` | `byte? s = p.Get<byte?>("@NewStatus");` | [第 8 章](ch08-stored-procedures.md)、[第 19 章](ch19-see-the-sql.md) |
| `DynamicParameters.ParameterNames` | 只读属性 | 已加入的参数名（**不带 @**） | 构造时传入的对象，执行前这里是空的 | `string.Join(", ", p.ParameterNames)` | [第 13 章](ch13-paging-dynamic-where.md)、[第 19 章](ch19-see-the-sql.md) |
| `commandType: CommandType.StoredProcedure` | 第一个字符串是过程名 | 按过程调用 | 不写时 Dapper 会猜：文本没有空格就当过程名，有空格（如 `EXEC 过程名`）就报 201 | `conn.Execute("dbo.Update_PriceRequestSubmit_Proc", p, commandType: CommandType.StoredProcedure)` | [第 8 章](ch08-stored-procedures.md) |

## 5. 多结果集与多重映射

| API | 签名要点 | 返回 | 0 行 / 多行 | 最小示例 | 详见 |
|---|---|---|---|---|---|
| `QueryMultiple` | `(sql, param, transaction, commandTimeout, commandType)`，几条 SELECT 写在一个字符串里 | `GridReader`（写 `using`） | 一次往返；参数所有语句共用 | `using var grid = conn.QueryMultiple(pageSql + countSql, new { Skip = 0, Take = 2 });` | [第 12 章](ch12-query-multiple.md) |
| `GridReader.Read<T>` | 读**下一个**结果集 | `IEnumerable<T>` | 0 行：空集合。第几次调用就读第几个，**不看 T**：读反了不报错、属性全是默认值 | `var rows = grid.Read<RequestRow>().ToList();` | [第 12 章](ch12-query-multiple.md) |
| `ReadFirst<T>` / `ReadFirstOrDefault<T>` / `ReadSingle<T>` / `ReadSingleOrDefault<T>` | 语义同第 2 节的四个方法 | `T` / `T?` | 同 `QueryFirst` 等；读多读一次报 `ObjectDisposedException: The reader has been disposed …` | `int total = grid.ReadSingle<int>();` | [第 12 章](ch12-query-multiple.md) |
| `GridReader.IsConsumed` | 只读属性 | `bool` | 读完最后一个结果集变 `true`，Dapper 自己开的连接随之关闭 | `Console.WriteLine(grid.IsConsumed);` | [第 12 章](ch12-query-multiple.md) |
| `GridReader.Read<A,B,R>(map, splitOn)` | 多结果集里的多重映射 | `IEnumerable<R>` | — | 只在[第 12 章](ch12-query-multiple.md)的对照表里列出，没有单独演示 | [第 12 章](ch12-query-multiple.md) |
| `Query<A,B,R>` | `(sql, map, param, transaction, buffered, splitOn = "Id", commandTimeout, commandType)`；泛型最多 7 个对象 | `IEnumerable<R>` | 每行调用一次 `map`，每行 new 新对象（一对多会重复主对象） | `conn.Query<PriceRequest, Customer, PriceRequest>(sql, (r, c) => { r.Customer = c; return r; }, splitOn: "Id")` | [第 10 章](ch10-multi-mapping-one-to-one.md) |
| `splitOn` | 第二个（及以后）对象的**第一列**列名，逗号分隔 | — | **从右往左**找；第 0 列不算；切分列为 NULL 时整个对象是 null；只写一个名字会用于每个切分点 | `splitOn: "Id,Id"`、`splitOn: "Name"` | [第 10 章](ch10-multi-mapping-one-to-one.md) |

一对多组装不是 API，是写法：JOIN + `Dictionary<int, 主对象>` 去重，或两次查询 + `IN @ids` + `ToLookup` 分组（[第 11 章](ch11-one-to-many.md)）。

## 6. 异步、超时与取消

| API | 签名要点 | 返回 | 0 行 / 多行 | 最小示例 | 详见 |
|---|---|---|---|---|---|
| `QueryAsync<T>` | 同 `Query<T>` | `Task<IEnumerable<T>>` | 同同步版 | `var list = (await conn.QueryAsync<Product>(sql)).ToList();` | [第 14 章](ch14-async-timeout-cancel.md) |
| `QueryFirstAsync<T>` / `QueryFirstOrDefaultAsync<T>` / `QuerySingleAsync<T>` / `QuerySingleOrDefaultAsync<T>` | 同同步版 | `Task<T>` | 同同步版 | `var r = await conn.QuerySingleAsync<RequestBrief>(sql, new { Id = 2 });` | [第 14 章](ch14-async-timeout-cancel.md) |
| `ExecuteScalarAsync<T>` | 同 `ExecuteScalar<T>` | `Task<T>` | 同同步版 | `int n = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM dbo.PriceRequest WHERE Status = 1");` | [第 14 章](ch14-async-timeout-cancel.md) |
| `ExecuteAsync` | 同 `Execute` | `Task<int>` | 同同步版；漏写 `await` 只有 CS4014 警告 | `int rows = await conn.ExecuteAsync(sql, obj);` | [第 14 章](ch14-async-timeout-cancel.md) |
| `QueryMultipleAsync` + `ReadAsync<T>` / `ReadSingleOrDefaultAsync<T>` | 同同步版 | `Task<GridReader>` / `Task<IEnumerable<T>>` / `Task<T?>` | 同同步版 | `using var grid = await conn.QueryMultipleAsync(sql, new { Id = id });` | [第 14 章](ch14-async-timeout-cancel.md)、[第 22 章](ch22-minimal-api.md) |
| `CommandDefinition` | 构造参数 `(commandText, parameters, transaction, commandTimeout, commandType, flags, cancellationToken)`，按名字传 | 交给 `QueryAsync<T>(CommandDefinition)`、`ExecuteAsync`、`ExecuteScalarAsync<T>` 等 | 普通的 `QueryAsync(sql, param)` **没有令牌参数** | `new CommandDefinition(sql, new { Applicant = "张伟" }, commandTimeout: 10, cancellationToken: ct)` | [第 14 章](ch14-async-timeout-cancel.md)、[第 22 章](ch22-minimal-api.md) |

| 情况 | 实测结果 | 详见 |
|---|---|---|
| 超时 | `SqlException`，`Number = -2`，`Execution Timeout Expired.` | [第 14 章](ch14-async-timeout-cancel.md) |
| 执行**中**被取消 | `SqlException`，消息末行 `Operation cancelled by user.` | [第 14 章](ch14-async-timeout-cancel.md) |
| 发出**前**已取消 | `TaskCanceledException` | [第 14 章](ch14-async-timeout-cancel.md) |
| 两个 Task 共用一个连接 | `The connection does not support MultipleActiveResultSets.` | [第 14 章](ch14-async-timeout-cancel.md) |

判断"是不是我取消的"用 `catch (Exception) when (token.IsCancellationRequested)`，不要猜异常类型。

## 7. 性能

| API | 签名要点 | 作用 | 注意 | 最小示例 | 详见 |
|---|---|---|---|---|---|
| `buffered: false` | `Query` 系列的参数 | 读一行交一行，内存和行数无关（沙箱 20 万行：+0.0 MB 对 +19 MB，数值因机器而异） | 后面接 `.ToList()`、`.OrderBy()` 等于白开 | `conn.Query<ReportRow>(sql, buffered: false)` | [第 15 章](ch15-performance.md) |
| `DbString` | `new DbString { Value, IsAnsi, IsFixedLength, Length }` | 指定字符串参数类型：`IsAnsi = true` → `varchar`，`Length = 20` → `(20)` | varchar 列用它避免 `CONVERT_IMPLICIT`；`Length` 比值短会**静默截断**；`IsAnsi` 的值含中文会变 `?` | `new { Code = new DbString { Value = code, IsAnsi = true, Length = 20 } }` | [第 15 章](ch15-performance.md)、[第 21 章](ch21-troubleshooting.md) |
| `AsTableValuedParameter` | `DataTable.AsTableValuedParameter("架构.类型名")` | 一个参数装一整张表，一次往返（沙箱 1000 行：约 3000 ms 对 13–31 ms，数值因机器而异） | 先 `CREATE TYPE`；`DataTable` 列按**位置**对应，顺序错了不报错、数据错位 | `new { Items = table.AsTableValuedParameter("dbo.PriceRequestItemType") }` | [第 15 章](ch15-performance.md) |
| `SqlMapper.GetCachedSQLCount()` | 静态方法 | 当前映射缓存条数 | 拼接 SQL 会让它一直涨，约每 500 条清理一次只用过一次的 | `Console.WriteLine(SqlMapper.GetCachedSQLCount());` | [第 3 章](ch03-how-it-works.md) |

## 8. 扩展与全局设置

| API | 签名要点 | 作用 | 注意 | 最小示例 | 详见 |
|---|---|---|---|---|---|
| `SqlMapper.TypeHandler<T>` | 继承后重写 `T Parse(object value)` 和 `void SetValue(IDbDataParameter parameter, T value)` | 自定义"列值 ↔ 对象"转换 | NULL 不调用 `Parse`，null 值不调用 `SetValue`（实测）；给 `string` 这类内置类型写，读生效、写不生效 | `public class ProductSpecHandler : SqlMapper.TypeHandler<ProductSpec>` | [第 17 章](ch17-type-handlers.md) |
| `SqlMapper.AddTypeHandler` | `(handler)` | 注册处理器（全局） | 必须在**第一次**用到该类型的查询之前；晚了同样列组合的查询仍用旧缓存 | `SqlMapper.AddTypeHandler(new ProductSpecHandler());` | [第 17 章](ch17-type-handlers.md) |
| `SqlMapper.HasTypeHandler` | `(Type)` | 查是否已注册 | 返回 `true` 不代表已经生效（缓存问题） | `SqlMapper.HasTypeHandler(typeof(ProductSpec))` | [第 17 章](ch17-type-handlers.md) |
| `SqlMapper.PurgeQueryCache()` | 静态方法 | 清空**所有**映射缓存 | 只用来救急，正式做法是早注册 | `SqlMapper.PurgeQueryCache();` | [第 17 章](ch17-type-handlers.md) |
| `DefaultTypeMap.MatchNamesWithUnderscores` | 静态属性，默认 `false` | `true` 时匹配忽略下划线：`request_no` → `RequestNo` | 全局；一个类**第一次映射时**的值会被缓存，之后改开关对它无效 → 放程序第一行 | `DefaultTypeMap.MatchNamesWithUnderscores = true;` | [第 7 章](ch07-mapping-issues.md) |
| `SqlMapper.Settings.CommandTimeout` | 静态属性，默认 `null` | 所有 Dapper 命令的默认超时秒数 | 单次传的 `commandTimeout:` 覆盖它 | `SqlMapper.Settings.CommandTimeout = 60;` | [第 14 章](ch14-async-timeout-cancel.md) |
| `SqlMapper.Settings.ApplyNullValues` | 静态属性，默认 `false` | `true` 时 NULL 也赋值：`string`、`int?` 属性会被设成 null（覆盖初始值） | 非可空 `int` 属性仍保持初始值；一般不用动 | `SqlMapper.Settings.ApplyNullValues = true;` | [第 7 章](ch07-mapping-issues.md) |

[返回目录](../README.md)
