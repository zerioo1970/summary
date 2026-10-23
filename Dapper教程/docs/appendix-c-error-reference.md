# 附录 C 报错速查

用途：拿着一条报错，先在这里按关键字找到原因和改法，再跳回讲过它的那一章；收录的都是正文各章实测过的报错原文。

## 拿到报错先做什么（同第 20 章）

| 步 | 做什么 | 看哪里 |
|---|---|---|
| 1 | **看异常类型，分三类** | `SqlException` 且错误号是 18456、4060、-2 这类 → 连接类；`SqlException` 其他错误号 → SQL 类；**不是** `SqlException`（`DataException`、`InvalidOperationException`、`ArgumentException`、`NotSupportedException`）→ 映射类，SQL 已经执行成功了 |
| 2 | **是 `SqlException` 就看错误号** | `ex.Number`，或堆栈末尾的 `Error Number:208,State:1,Class:16`。错误号和服务器语言无关，代码里判断也用它，不用消息文字 |
| 3 | **在堆栈里找第一行自己的代码** | 从上往下跳过 `Microsoft.Data.SqlClient.…`、`Dapper.SqlMapper.…`、`System.Linq.…` 这些框架行，第一个出现你自己类名的行就是出事的地方；看到 ` ---> ` 再读内层异常（`InnerException`），外层说"在哪"，内层说"为什么" |

详见[第 20 章](ch20-read-errors.md)。**没有报错、只是结果不对**的情况见本页最后一节和[第 21 章](ch21-troubleshooting.md)。

阅读说明：

- 英文是沙箱（英文版 SQL Server、英文系统）实测的原文。中文只收正文里出现过的版本；标 ⚠️ 的是正文注明"未实机验证 / 未核对"的中文措辞。
- 约束名后半段（如 `FK__PriceRequ__Produ__13F1F5EB`）每次建库都不同；`'…'` 里的列名、表名、值换成你自己的。
- 每组按在全书出现的章数从多到少排列。

---

## 1. 连接类（含命令超时、取消）

| 报错关键字 | 原因 | 改法 | 详见 |
|---|---|---|---|
| `Execution Timeout Expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.`，`Number = -2`<br/>中文：`执行超时已过期。完成操作之前已超时或服务器未响应。` | 命令执行时间超过 `commandTimeout`（不传就是 `SqlCommand` 默认的 30 秒）。连接其实已经建好，这是**命令超时**，不是连接超时。"偶尔超时"多半是在等锁 | 先查慢的原因：超时发生时查 `sys.dm_exec_requests` 的 `blocking_session_id`、看执行计划；确实要更久就给这一条单独传 `commandTimeout`；不要设 `0`（永不超时） | [第 14 章](ch14-async-timeout-cancel.md)、[第 9 章](ch09-transactions.md)、[第 20 章](ch20-read-errors.md)、[第 21 章](ch21-troubleshooting.md) 场景 4 |
| `Cannot open database "PriceDemo" requested by the login. The login failed.`，下一行 `Login failed for user '…'.`，`Number = 4060`<br/>中文：`无法打开登录所请求的数据库 "PriceDemo"。登录失败。` | 登录成功了，但库不存在、库名拼错，或账号没有这个库的权限 | 执行 `code/sql/00-reset-database.sql` 建库；核对连接串里的 `Database=` | [第 1 章](ch01-setup.md)、[第 20 章](ch20-read-errors.md) |
| `A severe error occurred on the current command.  The results, if any, should be discarded.`，下一行 `Operation cancelled by user.`（`SqlException`）<br/>中文：`当前命令发生服务器错误。结果(如果有)应被放弃。` / `用户已取消操作。` | 命令**执行过程中**，传进 `CommandDefinition` 的令牌发出了取消信号（用户点取消、HTTP 客户端断开） | 属于预期结果；用 `catch (Exception) when (ct.IsCancellationRequested)` 接住，不要只写 `catch (OperationCanceledException)` | [第 14 章](ch14-async-timeout-cancel.md)、[第 22 章](ch22-minimal-api.md) |
| `已成功与服务器建立连接，但是在登录前的握手期间发生错误。 (provider: TCP 提供程序, error: 35 - 捕获内部异常)`，内层写着 `UntrustedRoot`（第 1 章只贴了中文） | `Microsoft.Data.SqlClient` 4.0 起默认 `Encrypt=True`，服务器用的是自签名证书 | 开发环境连接串加 `TrustServerCertificate=True;`；生产环境给 SQL Server 装受信任的证书 | [第 1 章](ch01-setup.md) |
| `Login failed for user 'nobody'.`，`Number = 18456`<br/>中文：`用户 'nobody' 登录失败。` | 用户名 / 密码错；Windows 账号没有登录权限 | 核对连接串里的身份验证部分 | [第 20 章](ch20-read-errors.md) |
| `System.Threading.Tasks.TaskCanceledException` | 令牌在调用**之前**就已经取消，命令根本没发出去 | 同"执行中取消"那一条，用 `when (ct.IsCancellationRequested)` 统一判断 | [第 14 章](ch14-async-timeout-cancel.md) |

---

## 2. SQL 类（按错误号）

| 错误号 | 报错关键字 | 原因 | 改法 | 详见 |
|---|---|---|---|---|
| 547 | `The DELETE statement conflicted with the REFERENCE constraint "FK__…". The conflict occurred in database "PriceDemo", table "dbo.PriceRequestItem", column 'RequestId'.`<br/>`The INSERT statement conflicted with the FOREIGN KEY constraint "FK__…". The conflict occurred in database "PriceDemo", table "dbo.Product", column 'Id'.`<br/>中文（INSERT 版）：`INSERT 语句与 FOREIGN KEY 约束"FK__…"冲突。该冲突发生于数据库"PriceDemo"，表"dbo.Product", column 'Id'。` ⚠️ | DELETE：子表还有行引用它；INSERT：引用的父表行不存在（如产品 99） | DELETE 先删子表（明细、审批记录）或改成"作废"状态；INSERT 先校验数据。多条写操作放进事务，否则会留下半张单子；Web 接口里 `catch (SqlException ex) when (ex.Number == 547)` 回 400 | [第 6 章](ch06-insert-update-delete.md)、[第 9 章](ch09-transactions.md)、[第 20 章](ch20-read-errors.md)、[第 21 章](ch21-troubleshooting.md) 场景 2、[第 22 章](ch22-minimal-api.md)、[第 23 章](ch23-project.md) |
| 137 | `Must declare the scalar variable "@Status".`<br/>中文：`必须声明标量变量 "@Status"。` ⚠️ | SQL 里有 `@Status`，参数对象里没有同名属性（拼错、漏写、整个参数忘了传）；或者给 `Execute` 传了 `new[] { 1, 2 }` 这种简单值集合；或者 SQL 片段前少了空格，拼成了 `@ApplicantAND` | 核对每个 `@名字` 都有同名属性；按 Id 批量删用 `IN @Ids`；动态拼接的片段一律以空格开头，出错时打印 `sql.ToString()` | [第 4 章](ch04-parameters.md)、[第 6 章](ch06-insert-update-delete.md)、[第 13 章](ch13-paging-dynamic-where.md)、[第 20 章](ch20-read-errors.md) |
| 2627 | `Violation of UNIQUE KEY constraint 'UQ__PriceReq__…'. Cannot insert duplicate key in object 'dbo.PriceRequest'. The duplicate key value is (PR20261101).` | 单号、编码重复（`UNIQUE` 约束） | 翻译成友好提示：`catch (SqlException ex) when (ex.Number == 2627 \|\| ex.Number == 2601)`（2601 是唯一索引，正文未实测）；**不要**用 `ex.Message.Contains(…)` 判断 | [第 6 章](ch06-insert-update-delete.md)、[第 20 章](ch20-read-errors.md)、[第 22 章](ch22-minimal-api.md)、[第 23 章](ch23-project.md) |
| 207 | `Invalid column name 'RequestPrice'.`<br/>中文：`列名 'RequestPrice' 无效。` | 列名拼错；表改过列名而 SQL 还写着旧名 | 按[第 18 章](ch18-changing-queries.md)三步走改 SQL；新列名用 `AS 旧属性名` 映射回去。显式列清单会立刻报 207，`SELECT *` 则静默变默认值 | [第 18 章](ch18-changing-queries.md)、[第 19 章](ch19-see-the-sql.md)、[第 20 章](ch20-read-errors.md)、[第 21 章](ch21-troubleshooting.md) 场景 8 |
| 102 | `Incorrect syntax near ','.`<br/>中文：`',' 附近有语法错误。` | `IN (@Ids)` 多写了括号，Dapper 展开后成了 `IN ((@Ids1,@Ids2))` | 写成 `IN @Ids`，不加括号 | [第 4 章](ch04-parameters.md)、[第 11 章](ch11-one-to-many.md)、[第 21 章](ch21-troubleshooting.md) 场景 3 |
| 102 | `Incorrect syntax near '@Ids'.`<br/>中文：`'@Ids' 附近有语法错误。` | 把逗号字符串 `"1,3"` 当列表传给 `IN @Ids`：字符串不是集合，Dapper 不展开 | 先在 C# 里拆成 `int[]` 再传 | [第 21 章](ch21-troubleshooting.md) 场景 3 |
| 102 | `Incorrect syntax near '@Skip'.`，下一行 `Invalid usage of the option NEXT in the FETCH statement.` | 用了 `OFFSET … FETCH`，却没有 `ORDER BY` | 补上 `ORDER BY`，最后带一个唯一列（如主键） | [第 13 章](ch13-paging-dynamic-where.md) |
| （正文未给号） | `Incorrect syntax near '伟'.`，下一行 `Unclosed quotation mark after the character string ''.`<br/>中文：`'伟' 附近有语法错误。` / `字符串 '' 后的引号不完整。` | 值拼进了 SQL 文本，值里的单引号把字符串提前闭合 | 改成参数化；不要用"把 `'` 换成 `''`"补救 | [第 4 章](ch04-parameters.md) |
| 208 | `Invalid object name 'dbo.Custmer'.`<br/>中文：`对象名 'dbo.Custmer' 无效。` | 表 / 视图名拼错；连错了库；架构名不对；没执行建对象的脚本（如第 7 章的视图） | 核对对象名和 `Database=`；执行对应章节的 SQL 脚本 | [第 7 章](ch07-mapping-issues.md)、[第 20 章](ch20-read-errors.md) |
| 2812 | `Could not find stored procedure 'dbo.Update_PriceRequestSubmit_Proc'.`<br/>中文：`找不到存储过程 'dbo.…'。` ⚠️ | 过程名写错；没执行建过程的脚本；连到了别的库 | 执行 `ch08-procs.sql` / `ch23-project.sql`；核对过程名 | [第 8 章](ch08-stored-procedures.md)、[第 23 章](ch23-project.md) |
| 245 | `Conversion failed when converting the nvarchar value '1,3' to data type int.`<br/>中文：`在将 nvarchar 值 '1,3' 转换成数据类型 int 时失败。` | 字符串和 `int` 比较：`IN (@Ids)` 传了字符串 `"1,3"`；或 `JSON_VALUE(…) < 1` 没先 `CAST` | 列表传 `int[]`；`JSON_VALUE` 先 `CAST(… AS DECIMAL(9,2))` 再比较 | [第 17 章](ch17-type-handlers.md)、[第 21 章](ch21-troubleshooting.md) 场景 3 |
| 1008 | `The SELECT item identified by the ORDER BY number 1 contains a variable as part of the expression identifying a column position. Variables are only allowed when ordering by an expression referencing a column name.` | 写了 `ORDER BY @Sort`：参数只能代替值，不能代替列名 | 用白名单字典选出写死的排序片段；`ORDER BY (SELECT @Sort)` 不报错但排序静默失效 | [第 4 章](ch04-parameters.md)、[第 13 章](ch13-paging-dynamic-where.md) |
| 10742 | `The offset specified in a OFFSET clause may not be negative.` | 页码是 0 或负数，算出了负的 OFFSET | C# 里先校正：`pageIndex = Math.Max(pageIndex, 1)` | [第 13 章](ch13-paging-dynamic-where.md)、[第 23 章](ch23-project.md) |
| 201 | `Procedure or function 'Select_PriceRequestByNo_Proc' expects parameter '@RequestNo', which was not supplied.`<br/>中文：`过程或函数 '…' 需要参数 '@RequestNo'，但未提供该参数。` ⚠️ | 没写 `commandType` 且过程名带空格 / 写成 `EXEC 过程名`；或参数属性名和过程参数名不一致 | 显式写 `commandType: CommandType.StoredProcedure`；属性名和过程参数名一致 | [第 8 章](ch08-stored-procedures.md) |
| 8144 | `Procedure or function Select_PriceRequestByNo_Proc has too many arguments specified.`<br/>中文：`为过程或函数 … 指定了过多的参数。` ⚠️ | 调存储过程时参数对象的**每个属性**都会传过去，多了过程没有的 | 用匿名对象只挑过程要的参数，不要直接传实体 | [第 8 章](ch08-stored-procedures.md) |
| 2628 | `String or binary data would be truncated in table 'PriceDemo.dbo.Customer', column 'City'. Truncated value: '…'.` | 值比列长。SQL Server 2019 起是 2628；2017 及以前是 8152 且不报列名（正文未实测 8152） | 先在 C# 里校验长度；判断时 2628、8152 都写上 | [第 20 章](ch20-read-errors.md) |
| 515 | `Cannot insert the value NULL into column 'Approver', table 'PriceDemo.dbo.ApprovalLog'; column does not allow nulls. INSERT fails.` | 给 `NOT NULL` 列传了 null（C# 的 null 被当作 `DBNull` 发出） | 调用前校验必填项 | [第 9 章](ch09-transactions.md) |
| 334 | `The target table 'dbo.PriceRequest' of the DML statement cannot have any enabled triggers if the statement contains an OUTPUT clause without INTO clause.` | 表上有触发器，`OUTPUT INSERTED.Id` 没带 `INTO` | 改成 `OUTPUT INSERTED.Id INTO @t` 再 `SELECT Id FROM @t`，或用 `SCOPE_IDENTITY()` | [第 6 章](ch06-insert-update-delete.md) |
| 156 | `Incorrect syntax near the keyword 'ORDER'.` / `… 'AND'.`<br/>中文：`关键字 'ORDER' 附近有语法错误。` / `关键字 'AND' 附近有语法错误。` | 动态 WHERE 去掉了 `1 = 1`，没有条件时成了 `WHERE ORDER BY`，有条件时成了 `WHERE AND …` | 用 `WHERE 1 = 1`，每个条件片段以 ` AND ` 开头 | [第 13 章](ch13-paging-dynamic-where.md) |
| 10744 | `The number of rows provided for a FETCH clause must be greater then zero.`（`then` 是 SQL Server 原文的拼写） | 每页条数是 0 | C# 里限制每页条数范围，比如 1–100 | [第 13 章](ch13-paging-dynamic-where.md) |
| 351 | `Column, parameter, or variable @Items. : Cannot find data type PriceRequestItemTyp.` | 表值参数的类型名拼错、没写架构名，或 `CREATE TYPE` 脚本没在这个库执行 | 先执行 `ch15-tvp.sql`；类型名写成 `dbo.类型名` | [第 15 章](ch15-performance.md) |

---

## 3. 映射与 Dapper 类

这一类**不是** `SqlException`，没有错误号；Dapper 和 .NET 运行时的消息在中文系统下也是英文（第 5、10、12、20 章实测）。

| 报错关键字 | 原因 | 改法 | 详见 |
|---|---|---|---|
| `System.Data.DataException: Error parsing column 0 (City=上海 - String)`，内层 `FormatException: The input string '上海' was not in a correct format.` | 第 N 列（从 0 数）的值转不成属性类型。括号里是 SQL 里的列名、出错那一行的值、数据库给的类型 | 按列名找到属性，改成匹配的类型 | [第 2 章](ch02-first-query.md)、[第 20 章](ch20-read-errors.md) |
| `Error parsing column 0 ((Unnamed Column)=n/a - Null object cannot be converted to a value type.)`，内层 `InvalidCastException` | 只取一列的 `Query<int>` / `QuerySingle<decimal>` 遇到 NULL；`(Unnamed Column)` 表示 SQL 没写别名 | 泛型参数改成 `decimal?`；或 SQL 里 `ISNULL(…, 0)` | [第 7 章](ch07-mapping-issues.md) |
| `Error parsing column 3 (Spec={…} - String)`，内层 `Invalid cast from 'System.String' to 'ProductSpec'.` | 自定义类型没有注册类型处理器；或注册晚了（之前已查过同样列组合，旧映射已缓存） | 程序启动第一行 `SqlMapper.AddTypeHandler(…)` | [第 17 章](ch17-type-handlers.md) |
| `InvalidOperationException: Sequence contains no elements` | `QueryFirst` / `QuerySingle` / `ReadSingle` 查到 0 行；`DbString.Length` 写短了把值截断后也会查不到 | 查不到属正常就换 `…OrDefault` 并判断 null；`Length` 照抄列定义 | [第 5 章](ch05-single-row.md)、[第 15 章](ch15-performance.md)、[第 20 章](ch20-read-errors.md) |
| `InvalidOperationException: Sequence contains more than one element` | `QuerySingle` / `QuerySingleOrDefault` 查到多行（`OrDefault` 只管 0 行）；`QueryMultiple` 里先用 `ReadSingle` 读到了多行的结果集 | 查数据是否重复、SQL 是否少条件；只要第一行用 `QueryFirst` + `ORDER BY`；核对 Read 顺序 | [第 5 章](ch05-single-row.md)、[第 12 章](ch12-query-multiple.md)、[第 20 章](ch20-read-errors.md) |
| `System.NullReferenceException: Object reference not set to an instance of an object.` | 常见四种：`…OrDefault` 返回 null 没判断（编译时有 CS8602）；多重映射函数里忘了 `r.Customer = c`；LEFT JOIN 的 null 子对象被加进了列表；`DynamicParameters.Get<byte>` 取早了（堆栈里有 `DynamicParameters.Get`） | 先判断 `is null`；映射函数里每个对象都挂上；组装时 `if (item is not null)`；执行完、结果集读完再 `Get` | [第 5 章](ch05-single-row.md)、[第 8 章](ch08-stored-procedures.md)、[第 10 章](ch10-multi-mapping-one-to-one.md)、[第 11 章](ch11-one-to-many.md) |
| `ArgumentException: Multi-map error: splitOn column 'Id' was not found - please ensure your splitOn parameter is set and in the correct order (Parameter 'splitOn')` | 第二个对象的第一列不叫 `Id` 又没写 `splitOn`；唯一的 `Id` 在第 0 列（不算）；`splitOn` 拼错（引号里就是你写的名字） | 写 `splitOn: "第二个对象第一列的列名"`，拿引号里的名字去 SELECT 列表里对照 | [第 10 章](ch10-multi-mapping-one-to-one.md)、[第 20 章](ch20-read-errors.md) |
| `NotSupportedException: The member Spec of type ProductSpec cannot be used as a parameter value` | 参数对象的某个属性是类，Dapper 不知道怎么变成一个 SQL 参数 | 传它的某个属性（如 `c.Id`）；确实要整体传就写类型处理器 | [第 17 章](ch17-type-handlers.md)、[第 20 章](ch20-read-errors.md) |
| `KeyNotFoundException: The given key 'Ret' was not present in the dictionary.` | `DynamicParameters.Get` 的名字拼错（报错里不带 `@`）；或者在执行前取构造函数传入的参数 | 核对拼写，带不带 `@` 都行；执行后再取 | [第 8 章](ch08-stored-procedures.md)、[第 19 章](ch19-see-the-sql.md) |
| `ObjectDisposedException: The reader has been disposed; this can happen after all data has been consumed`，`Object name: 'Dapper.SqlMapper+GridReader'.` | `Read` 次数比 SELECT 多；或离开 `using` 后还在读 | 数一数 SELECT 和 Read 是否一样多；要用的数据先读进 `List` | [第 12 章](ch12-query-multiple.md) |
| `ApplicationException: Attempting to cast a DBNull to a non nullable type! Note that out/return parameters will not have updated values until the data stream completes …` | Output 参数值是 NULL，却用 `Get<byte>` 这种不可空类型取；或结果集还没读完就取 | 改用 `Get<byte?>`；执行完、读完再取 | [第 8 章](ch08-stored-procedures.md) |
| `InvalidOperationException: An enumerable sequence of parameters (arrays, lists, etc) is not allowed in this context` | 把集合当参数传给了 `Query` / `QuerySingle` 等读方法 | 只有 `Execute` 支持集合参数；查多个值用 `IN @Ids` | [第 6 章](ch06-insert-update-delete.md) |
| `InvalidOperationException: A parameterless default constructor or one matching signature (System.Int32 Id, System.String RequestNo, System.Byte Status) is required for RequestRecord materialization` | record / 只有带参构造函数的类，参数类型和列类型不完全一致（`int` 对 TINYINT 也不行） | 照报错括号里的签名改参数类型；或改用带 `{ get; set; }` 的普通类 | [第 7 章](ch07-mapping-issues.md) |
| `FormatException: The input string 'PR20261015' was not in a correct format.`（堆栈里有 `Dapper.SqlMapper.ExecuteScalar`） | `ExecuteScalar<int>` 拿到的第一行第一列是字符串；这条报错**不带列名** | 看 SQL 的第一列是什么，改 `T` | [第 5 章](ch05-single-row.md) |
| `DivideByZeroException: Attempted to divide by zero.`（堆栈里只有你自己的代码） | `splitOn: "Id,Code"` 名字顺序和段的顺序不一致，某个对象的属性全是 0，后面的计算才出错 | 名字从左到右对应第 2、3… 段的第一列，按"从右往左"规则走一遍 | [第 10 章](ch10-multi-mapping-one-to-one.md) |

**对照：ADO.NET 手写映射时的同类报错**（改造老代码时会见到）

| 报错关键字 | 原因 | 详见 |
|---|---|---|
| `System.IndexOutOfRangeException: Lvl` | `GetOrdinal("Lvl")` 列名写错（Dapper 下同样的错误不报错、只给默认值） | [第 1 章](ch01-setup.md)、[第 20 章](ch20-read-errors.md) |
| `InvalidCastException: Unable to cast object of type 'System.Int32' to type 'System.Int64'.` | `GetXxx` 的类型和列类型不一致，报错不说是哪一列 | [第 1 章](ch01-setup.md)、[第 20 章](ch20-read-errors.md) |
| `InvalidOperationException: Invalid attempt to read when no data is present.` | 没判断 `Read()` 的返回值就读 | [第 20 章](ch20-read-errors.md) |

---

## 4. 事务与连接状态类

| 报错关键字 | 原因 | 改法 | 详见 |
|---|---|---|---|
| `InvalidOperationException: There is already an open DataReader associated with this Connection which must be closed first.`<br/>中文：`已有打开的与此 Connection 相关联的 DataReader，必须首先将它关闭。` | 同一连接上还有没读完的 DataReader：`buffered: false` 的 `foreach` 里又查；`GridReader` 没读完、没释放就执行别的命令 | 外层改回缓冲；先读完或离开 `using` 再查；或另开一个连接 | [第 3 章](ch03-how-it-works.md)、[第 12 章](ch12-query-multiple.md) |
| `ExecuteNonQuery requires the command to have a transaction when the connection assigned to the command is in a pending local transaction.  The Transaction property of the command has not been initialized.`（开头也可能是 `ExecuteReader` / `ExecuteScalar`）<br/>中文：`如果分配给命令的连接位于本地挂起事务中，ExecuteNonQuery 要求命令拥有事务。命令的 Transaction 属性尚未初始化。` | 事务进行中，某条命令没传 `transaction: tran`（读操作也算）；或把 `tran` 按位置传到了 `param` 上 | 按开头的方法名找：`ExecuteReader` 对应 `Query` 系列、`ExecuteNonQuery` 对应 `Execute`；一律用命名参数 `transaction: tran` | [第 9 章](ch09-transactions.md)、[第 23 章](ch23-project.md) |
| `InvalidOperationException: Invalid operation. The connection is closed.`<br/>中文：`无效操作。连接被关闭。` | 没 `Open()` 就 `BeginTransaction()`（Dapper 的自动开连接只对 Dapper 方法有效） | 开事务前先 `conn.Open()` | [第 9 章](ch09-transactions.md) |
| `The transaction is either not associated with the current connection or has been completed.`<br/>中文：`事务要么与当前连接无关联，要么已完成。` | 把 A 连接开的事务传给了 B 连接上的命令 | 一个事务里的所有命令用同一个连接对象 | [第 9 章](ch09-transactions.md) |
| `This SqlTransaction has completed; it is no longer usable.`<br/>中文：`此 SqlTransaction 已完成；它再也无法使用。` | Commit 之后又 Commit 或 Rollback，常见于 `finally` 里无条件 `Rollback()` | 回滚写在 `catch` 里；或用 `using` + 最后一行 `Commit()` | [第 9 章](ch09-transactions.md) |
| `InvalidOperationException: The connection does not support MultipleActiveResultSets.`<br/>中文：`此连接不支持 MultipleActiveResultSets。` | 两个并发的 Task 共用一个连接 | 每个并发任务各用一个连接 | [第 14 章](ch14-async-timeout-cancel.md) |
| `InvalidOperationException: Timeout expired.  The timeout period elapsed prior to obtaining a connection from the pool.  This may have occurred because all pooled connections were in use and max pool size was reached.`<br/>中文：`超时时间已到。超时时间已到，但是尚未从池中获取连接。出现这种情况可能是因为所有池连接均在使用，并且达到了最大池大小。` | 有连接打开后没关（少了 `using`），池里默认 100 条被占满，等满 `Connect Timeout`（默认 15 秒） | 找出没有 `using` 的连接；不要靠调大 `Max Pool Size` 掩盖 | [第 16 章](ch16-connection-and-repository.md) |
| `ExecuteScalar requires an open and available Connection. The connection's current state is connecting.` / `Internal DbConnection Error: 4` / `The connection was not closed. The connection's current state is connecting.`（并发时随机出现） | 全程序共享一个静态 `SqlConnection`，多个线程同时用；`SqlConnection` 不是线程安全的 | 每次查询从工厂 `Create()` 一个新连接，`using` 释放 | [第 16 章](ch16-connection-and-repository.md) |

---

## 5. 配置与项目类（NuGet、csproj、appsettings、xUnit）

| 报错关键字 | 原因 | 改法 | 详见 |
|---|---|---|---|
| `System.IO.FileNotFoundException: The configuration file 'appsettings.json' was not found and is not optional. The expected physical path was '…/bin/Debug/net10.0/appsettings.json'.`（中文系统下也是英文） | 控制台项目里 `appsettings.json` 没有复制到输出目录 | 文件属性「复制到输出目录」选「如果较新则复制」（csproj 里的 `<None Update="appsettings.json">`）；Web 项目自动复制 | [第 16 章](ch16-connection-and-repository.md)、[第 23 章](ch23-project.md) |
| 警告 `NU1903` / `NU1902`：`System.Security.Cryptography.Pkcs 9.0.18` 有已知漏洞 | SqlClient 间接引用了旧版 Pkcs | 显式安装 `System.Security.Cryptography.Pkcs` 10.0.13 | [第 1 章](ch01-setup.md)、[第 2 章](ch02-first-query.md) |
| `error CS1061: 'SqlConnection' does not contain a definition for 'Query' and no accessible extension method 'Query' … (are you missing a using directive or an assembly reference?)` | 少了 `using Dapper;`，或项目没装 Dapper 包 | 文件顶部加 `using Dapper;`；csproj 里确认有 `Dapper` 的 `PackageReference` | [第 2 章](ch02-first-query.md) |
| `InvalidOperationException: The ConnectionString property has not been initialized.`<br/>中文：`ConnectionString 属性尚未初始化。` | `GetConnectionString("键名")` 写错，返回 null 一直传到了 `SqlConnection`（第一次查询时才报） | 核对 JSON 键名；连接工厂构造时检查 null，启动就报清楚（第 16 章示例抛的是 `appsettings.json 里没有找到 ConnectionStrings:PriceDemo`） | [第 16 章](ch16-connection-and-repository.md) |
| `error NU1202: Package Microsoft.Data.SqlClient 7.1.1 is not compatible with net10 (.NETFramework,Version=v1.0).` | `TargetFramework` 写成了 `net10`，被解析成 .NET Framework 1.0 | 改回 `net10.0` | [第 1 章](ch01-setup.md) |
| `error NU1102: Unable to find package Microsoft.Data.SqlClient with version (>= 7.1.99)`，下一行 `Nearest version: 7.1.1` | 包版本不存在 | 用报错给出的最接近版本，或查 NuGet 上的版本列表 | [第 1 章](ch01-setup.md) |
| `error CS0234: The type or namespace name 'Data' does not exist in the namespace 'Microsoft'` | csproj 里没有 `Microsoft.Data.SqlClient` 包 | 装回这个包 | [第 1 章](ch01-setup.md) |
| `error CS1069: The type name 'SqlConnection' could not be found in the namespace 'System.Data.SqlClient'` | 从老项目复制代码时带过来了 `using System.Data.SqlClient;` | 改成 `using Microsoft.Data.SqlClient;` | [第 1 章](ch01-setup.md) |
| `warning CS8602: Dereference of a possibly null reference.`<br/>中文 VS：`CS8602 解引用可能出现空引用。` | `…OrDefault` 的返回类型是 `T?`，没判断 null 就访问成员 | 先判断 `is null`；不要忽略这条警告，它预告了 `NullReferenceException` | [第 5 章](ch05-single-row.md) |
| `error CS0411: The type arguments for method 'Enumerable.ToList<TSource>(IEnumerable<TSource>)' cannot be inferred from the usage. Try specifying the type arguments explicitly.` | `QueryAsync` 前面漏了 `await`，对 `Task` 调了 `.ToList()` | 写成 `(await conn.QueryAsync<T>(…)).ToList()` | [第 14 章](ch14-async-timeout-cancel.md) |
| `warning CS4014: Because this call is not awaited, execution of the current method continues before the call is completed.` | `ExecuteAsync(…)` 前面漏了 `await`，能编译、不等执行完就往下走 | 补上 `await`；把 CS4014 当错误对待 | [第 14 章](ch14-async-timeout-cancel.md) |
| `error : Testing with VSTest target is no longer supported by Microsoft.Testing.Platform on .NET 10 SDK and later. If you use dotnet test, you should opt-in to the new dotnet test experience.` | xUnit v3 4.x 默认走 Microsoft.Testing.Platform，.NET 10 SDK 的 `dotnet test` 需要显式选用 | 解决方案根目录加 `global.json`：`{ "test": { "runner": "Microsoft.Testing.Platform" } }` | [第 18 章](ch18-changing-queries.md) |

---

## 6. Web（第 22 章）

| 报错 / 现象 | 原因 | 改法 | 详见 |
|---|---|---|---|
| 500，正文 `System.InvalidOperationException: Body was inferred but the method does not allow inferred body parameters.` … `Did you mean to register the "Body (Inferred)" parameter(s) as a Service or apply the [FromServices] or [FromBody] attribute?` | 端点参数的类型（仓储）没在容器里登记，框架以为它来自请求体 | `builder.Build()` 之前 `builder.Services.AddScoped<那个仓储>()` | [第 22 章](ch22-minimal-api.md) |
| 启动即崩：`System.AggregateException: Some services are not able to be constructed (… Cannot consume scoped service 'IDbConnectionFactory' from singleton 'CustomerRepository'.)` | 单例依赖了 Scoped 服务（开发环境下 `Build()` 时就检查） | 工厂 `AddSingleton`、仓储 `AddScoped`；长生命周期只能依赖不短于它的 | [第 22 章](ch22-minimal-api.md) |
| 500，正文是完整的 `SqlException` 和堆栈（开发者异常页），服务端日志 `fail: Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware[1]` | 外键 547、唯一键 2627 没在端点里接住 | `catch (SqlException ex) when (ex.Number == 547)` 回 400，2627 回 409 | [第 22 章](ch22-minimal-api.md) |
| `GET /requests/abc` 返回 404，正文为空 | 路由模板 `{id:int}` 不匹配，端点没执行 | 正常现象，调用方传整数 Id | [第 22 章](ch22-minimal-api.md) |

---

## 附：没有报错、结果却不对

这类问题没有报错可查，按"现象"找：

| 现象 | 最常见的原因 | 第一步查什么 | 详见 |
|---|---|---|---|
| 属性全是 `0`、`""`、`null`、`0001-01-01` | 列名和属性名对不上（别名、下划线、拼错） | 结果集的真实列名 | [第 2 章](ch02-first-query.md)、[第 7 章](ch07-mapping-issues.md)、[第 21 章](ch21-troubleshooting.md) 场景 1 |
| 多重映射部分属性为空 / 为 0 | 列放错了段，或 `splitOn` 顺序不对 | 按切分列把 SELECT 列表分段 | [第 10 章](ch10-multi-mapping-one-to-one.md)、[第 21 章](ch21-troubleshooting.md) 场景 6 |
| 查不到返回 0，和真实的 0 分不清 | `OrDefault` / `ExecuteScalar` 的 T 是值类型 | T 是否可空 | [第 5 章](ch05-single-row.md) |
| "新 Id" 总是 1 | `OUTPUT INSERTED.Id` 用 `Execute` 接 | 用的是哪个方法 | [第 6 章](ch06-insert-update-delete.md) |
| `Execute` 总返回 -1 | `SET NOCOUNT ON` | SQL / 过程里有没有这句 | [第 6 章](ch06-insert-update-delete.md)、[第 8 章](ch08-stored-procedures.md) |
| Output 参数总是 NULL | `Add` 时没写 `direction` | `p.Add` 那一行 | [第 8 章](ch08-stored-procedures.md) |
| 一对多主对象重复 / 分页总数比列表多 | JOIN 后行数膨胀 | JOIN 之后的真实行数 | [第 11 章](ch11-one-to-many.md)、[第 21 章](ch21-troubleshooting.md) 场景 5 |
| `QueryMultiple` 读出的属性全是默认值 | Read 顺序和 SELECT 顺序不一致 | 每条 SELECT 与每次 Read 的序号 | [第 12 章](ch12-query-multiple.md) |
| 中文变成 `??` | varchar 列在 Latin1 排序规则下；字面量没加 `N`；`DbString { IsAnsi = true }` | `CAST(列 AS VARBINARY)` 看字节、库的排序规则 | [第 21 章](ch21-troubleshooting.md) 场景 7 |
| 改表后某页面数据为空 | `SELECT *` + 列改名 | 表现在的列 | [第 18 章](ch18-changing-queries.md)、[第 21 章](ch21-troubleshooting.md) 场景 8 |
| 数据只写了一半 | 多条写操作没放进事务，或事务里吞了异常照样 Commit | 主表有、子表不全的数据 | [第 9 章](ch09-transactions.md)、[第 21 章](ch21-troubleshooting.md) 场景 2 |
| 用户取消了，数据库里的查询还在跑 | 令牌没传进 `CommandDefinition` | 每个 `ct` 有没有一路传下去 | [第 14 章](ch14-async-timeout-cancel.md)、[第 22 章](ch22-minimal-api.md) |

[返回目录](../README.md)
