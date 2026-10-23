# Dapper 新手教程（C# + Visual Studio 2026）

> 版本基线：**.NET 10（LTS）+ C# 14** ｜ **Visual Studio 2026** ｜ `Dapper 2.1.89` ｜ `Microsoft.Data.SqlClient 7.1.1` ｜ SQL Server 2016 及以上（教程实测用 SQL Server 2022）
> 示例库：`PriceDemo`（客户 / 产品 / 价格申请单 / 申请明细 / 审批记录），建库脚本 [`code/sql/00-reset-database.sql`](code/sql/00-reset-database.sql)，可反复执行
>
> 本教程是按《教程编写规范》6.1 节登记的独立 C# 教程，ADO.NET 写法作为正式对照参与讲解。

---

## 这本教程写给谁

- 写过多年 ADO.NET（`SqlCommand` + `SqlDataReader`），SQL 很熟，但没用过 Dapper。
- 想知道 Dapper 那一行代码背后做了什么，出错时能自己查出问题。
- 想要的是能直接用到项目里的写法，不是 API 大全。

**最短路径**：第 1–6、9 章，约 10 小时，能用 Dapper 完成增删改查和事务。
**完整路径**：23 章 + 4 个附录，每天 2 小时约 4–5 周（第 21–23 章是综合实战，篇幅较长，各需 2–3 天）。

### 关于截图

VS 操作都给出了精确的菜单路径。需要截图的位置都留了 `📷 截图占位 chNN-XX`，并写明了截图内容和文件名。截好的图放到 `images/`，再去掉对应 `<!-- -->` 注释，图就会显示出来。

---

## 进度

全部完成：23 章 + 4 个附录。所有代码都在 .NET 10 SDK + SQL Server 2022 上实际运行过，正文贴出的输出和报错都是运行原文；无法在沙箱验证的部分（VS 图形界面、Windows 身份验证等）已在正文标注。

| 章节 | 状态 |
| --- | --- |
| [第 1 章 搭环境：VS 2026 + .NET 10 + SQL Server](docs/ch01-setup.md) | ✅ |
| [第 2 章 第一个 Dapper 查询](docs/ch02-first-query.md) | ✅ |
| [第 3 章 原理：Dapper 在背后替你干了什么](docs/ch03-how-it-works.md) | ✅ |
| [第 4 章 参数化查询](docs/ch04-parameters.md) | ✅ |
| [第 5 章 只要一行或一个值](docs/ch05-single-row.md) | ✅ |
| [第 6 章 增、删、改](docs/ch06-insert-update-delete.md) | ✅ |
| [第 7 章 映射不上怎么办](docs/ch07-mapping-issues.md) | ✅ |
| [第 8 章 调存储过程](docs/ch08-stored-procedures.md) | ✅ |
| [第 9 章 事务](docs/ch09-transactions.md) | ✅ |
| [第 10 章 多表关联：一对一](docs/ch10-multi-mapping-one-to-one.md) | ✅ |
| [第 11 章 一对多：主表 + 明细](docs/ch11-one-to-many.md) | ✅ |
| [第 12 章 一次取多个结果集](docs/ch12-query-multiple.md) | ✅ |
| [第 13 章 分页与动态条件](docs/ch13-paging-dynamic-where.md) | ✅ |
| [第 14 章 异步、超时与取消](docs/ch14-async-timeout-cancel.md) | ✅ |
| [第 15 章 大数据量与性能](docs/ch15-performance.md) | ✅ |
| [第 16 章 连接管理与封装](docs/ch16-connection-and-repository.md) | ✅ |
| [第 17 章 自定义类型处理](docs/ch17-type-handlers.md) | ✅ |
| [第 18 章 改需求：安全地修改已有查询](docs/ch18-changing-queries.md) | ✅ |
| [第 19 章 看清到底发出了什么 SQL](docs/ch19-see-the-sql.md) | ✅ |
| [第 20 章 读懂报错](docs/ch20-read-errors.md) | ✅ |
| [第 21 章 常见故障排查实战](docs/ch21-troubleshooting.md) | ✅ |
| [第 22 章 在 ASP.NET Core Minimal API 里用 Dapper](docs/ch22-minimal-api.md) | ✅ |
| [第 23 章 综合项目：价格申请单管理](docs/ch23-project.md) | ✅ |
| 附录 A–D | ✅ |

---

## 目录

### 第一部分 起步：先跑起来，再看清原理

**[第 1 章 搭环境：VS 2026 + .NET 10 + SQL Server](docs/ch01-setup.md)**
先用一页讲清 Dapper 是什么、不是什么；再搭好环境，用 ADO.NET 写出对照组；最后预览 Dapper 一行版本。新概念：SDK 风格 csproj、NuGet 包引用、连接字符串（默认加密）。

**[第 2 章 第一个 Dapper 查询](docs/ch02-first-query.md)**
把十几行 `reader.GetXxx` 换成一行 `Query<T>`。新概念：扩展方法、`Query<T>`、列名→属性自动映射。

**[第 3 章 原理：Dapper 在背后替你干了什么](docs/ch03-how-it-works.md)**
看清"那一行"拆开后的步骤，出问题时知道往哪里查。新概念：执行流程、映射代码的生成与缓存、缓冲读取与流式读取。

### 第二部分 基础增删改查

**[第 4 章 参数化查询](docs/ch04-parameters.md)** —— 匿名对象传参、`IN @ids` 列表展开、在 Profiler 里看到 `sp_executesql`。

**[第 5 章 只要一行或一个值](docs/ch05-single-row.md)** —— `QueryFirst/FirstOrDefault/Single/SingleOrDefault` 的区别、`ExecuteScalar<T>`。

**[第 6 章 增、删、改](docs/ch06-insert-update-delete.md)** —— `Execute` 与受影响行数、`OUTPUT INSERTED.Id` 取自增 ID、传 List 逐条执行。

**[第 7 章 映射不上怎么办](docs/ch07-mapping-issues.md)** —— 列名别名与 `MatchNamesWithUnderscores`、NULL 与可空类型、枚举和 `DateOnly`。

**[第 8 章 调存储过程](docs/ch08-stored-procedures.md)** —— `commandType: StoredProcedure`、`DynamicParameters`、Output 参数与 ReturnValue。

**[第 9 章 事务](docs/ch09-transactions.md)** —— `BeginTransaction`、每条命令都要传 `transaction`、回滚时机。

### 第三部分 进阶查询

**[第 10 章 多表关联：一对一](docs/ch10-multi-mapping-one-to-one.md)** —— 多重映射 `Query<A,B,R>`、`splitOn` 原理与写错时的现象。

**[第 11 章 一对多：主表 + 明细](docs/ch11-one-to-many.md)** —— 字典去重组装子集合，与"分两次查"的取舍。

**[第 12 章 一次取多个结果集](docs/ch12-query-multiple.md)** —— `QueryMultiple`、`Read<T>` 的顺序规则。

**[第 13 章 分页与动态条件](docs/ch13-paging-dynamic-where.md)** —— `OFFSET…FETCH`、安全拼接动态 WHERE、按需添加参数。

**[第 14 章 异步、超时与取消](docs/ch14-async-timeout-cancel.md)** —— `QueryAsync`、`commandTimeout`、`CommandDefinition` + `CancellationToken`。

**[第 15 章 大数据量与性能](docs/ch15-performance.md)** —— `buffered: false` 流式读取、`DbString`、表值参数批量写入。

### 第四部分 组织代码与修改

**[第 16 章 连接管理与封装](docs/ch16-connection-and-repository.md)** —— 连接池原理、连接工厂、Repository 封装，连接字符串移到配置文件。

**[第 17 章 自定义类型处理](docs/ch17-type-handlers.md)** —— `SqlMapper.TypeHandler<T>`、注册时机、什么时候不该用。

**[第 18 章 改需求：安全地修改已有查询](docs/ch18-changing-queries.md)** —— 改动三步走、`SELECT *` 的隐患、最小冒烟测试。

### 第五部分 查错

**[第 19 章 看清到底发出了什么 SQL](docs/ch19-see-the-sql.md)** —— 调试器看参数、Profiler / 扩展事件抓语句、给连接包一层日志。

**[第 20 章 读懂报错](docs/ch20-read-errors.md)** —— SQL / 映射 / 连接三类异常的定位法、`SqlException.Number`、"Error parsing column N"。

**[第 21 章 常见故障排查实战](docs/ch21-troubleshooting.md)** —— 8 个真实场景，按"现象→定位→修复"走一遍（不引入新概念）。

### 第六部分 用起来

**[第 22 章 在 ASP.NET Core Minimal API 里用 Dapper](docs/ch22-minimal-api.md)** —— 依赖注入注册连接工厂、在接口里调用 Repository、异步与取消接到 HTTP 请求上。

**[第 23 章 综合项目：价格申请单管理](docs/ch23-project.md)** —— 分页列表、新建（事务）、审批（存储过程）、明细查询串成一个完整程序。

### 附录

- [附录 A Dapper 常用 API 速查表](docs/appendix-a-api-cheatsheet.md) —— 每个 API 的返回、0 行 / 多行行为、最小示例和章节出处
- [附录 B ADO.NET ↔ Dapper 写法对照](docs/appendix-b-adonet-vs-dapper.md) —— 13 项常见操作，左右两段代码对照（全部实测）
- [附录 C 报错速查](docs/appendix-c-error-reference.md) —— 全书实测过的 69 条报错：原文 → 原因 → 改法 → 详见第几章
- [附录 D Dapper 和 EF Core 怎么选](docs/appendix-d-dapper-vs-efcore.md) —— 定位、适用场景、混用分工与决策图
