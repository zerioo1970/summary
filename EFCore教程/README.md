# EF Core 10 入门教程（C# + Visual Studio 2026）

> 版本基线：**.NET 10（LTS）+ C# 14** ｜ **Visual Studio 2026** ｜ `Microsoft.EntityFrameworkCore.SqlServer 10.0.14` ｜ `Microsoft.EntityFrameworkCore.Tools 10.0.14`（迁移命令）｜ SQL Server 2016 及以上
> 示例库：`EfDemo`，**由代码通过迁移生成**（Code First），不需要手写建表脚本。从第 5 章开始只有 `Products` 一张表，第 12 章加 `Categories`，第 14 章再加一张标签表，始终保持简单。
> 连接字符串统一为 `Server=.;Database=EfDemo;Integrated Security=True;TrustServerCertificate=True;`，按你的实例修改 `Server=` 那一段。
>
> 本教程是按《教程编写规范》6.1 节登记的独立 C# 教程（2026-10-26 确认），ADO.NET / SQL 写法作为正式对照参与讲解。

---

## 这本教程写给谁

- 写过多年 ADO.NET 和 SQL，但没有用过 EF Core，也不熟悉 LINQ。
- 想知道 EF Core 替你写的 SQL 长什么样，出了问题能自己查。
- 想做的是新项目：**先写 C# 类，再由 EF Core 生成表**。

**为什么从 LINQ 开始**：EF Core 的查询就是用 LINQ 写的。前 4 章只在内存的 `List` 上练习 LINQ，不连数据库，报错马上能看到。到第 5 章接 EF Core 时，只需要关心"这句 LINQ 怎么变成 SQL"这一个新问题。

**最短路径**：第 1–9 章 + 第 12 章，约 13 小时。学完能用 EF Core 从零建库，完成增删改查和一对多查询。
**完整路径**：16 章 + 3 个附录，每天 2 小时约 3 周。

### 关于截图

VS 操作都给出了精确的菜单路径。需要截图的位置都留了 `📷 截图占位 chNN-XX`，并写明了截图内容和文件名。截好的图放到 `images/`，再去掉对应的 `<!-- -->` 注释，图就会显示出来。

---

## 进度

所有代码都在 .NET 10 SDK 上实际运行过，正文贴出的输出和报错是运行原文。沙箱里无法验证的部分（VS 图形界面、Windows 身份验证等）会在正文中标注。

| 章节 | 状态 |
| --- | --- |
| [第 1 章 LINQ 是什么：原理与第一个查询](docs/ch01-first-linq.md) | ✅ |
| [第 2 章 挑列、排序、取一条](docs/ch02-select-orderby-first.md) | ✅ |
| [第 3 章 汇总、分组与分页](docs/ch03-aggregate-groupby-paging.md) | ✅ |
| [第 4 章 延迟执行与查询语法](docs/ch04-deferred-query-syntax.md) | ✅ |
| **第 5 章 第一个 EF Core 程序：由代码生成表（下一章）** | 📝 |
| 第 6 章 映射：约定与配置 | 📝 |
| 第 7 章 迁移进阶：改表、回滚、发布 | 📝 |
| 第 8 章 LINQ 是怎么变成 SQL 的 | 📝 |
| 第 9 章 增删改与变更跟踪 | 📝 |
| 第 10 章 批量操作与原生 SQL | 📝 |
| 第 11 章 事务与并发冲突 | 📝 |
| 第 12 章 一对多：外键与导航属性 | 📝 |
| 第 13 章 加载策略与 N+1 问题 | 📝 |
| 第 14 章 不一一对应的情况：多对多、复杂类型、继承 | 📝 |
| 第 15 章 接入 ASP.NET Core Minimal API | 📝 |
| 第 16 章 综合项目：产品库存管理 | 📝 |
| 附录 A–C | 📝 |

---

## 目录

### 第一部分 LINQ 基础（内存数据，不连数据库）

**[第 1 章 LINQ 是什么：原理与第一个查询](docs/ch01-first-linq.md)**
先看清原理：查询方法就是 .NET 替你写好的 `foreach`，规则由你交给它（自己写一个 `Where` 来证明）。新概念：LINQ 的组成与扩展方法、lambda 表达式、`Where`。

**[第 2 章 挑列、排序、取一条](docs/ch02-select-orderby-first.md)** —— `Select` 与匿名类型；`OrderBy`/`ThenBy`；`First`/`FirstOrDefault`/`Single` 的区别。

**[第 3 章 汇总、分组与分页](docs/ch03-aggregate-groupby-paging.md)** —— `Count`/`Any`/`Sum`/`Max`/`Average`；`GroupBy`；`Skip`/`Take`。

**[第 4 章 延迟执行与查询语法](docs/ch04-deferred-query-syntax.md)** —— 为什么末尾要 `ToList()`、查询到底什么时候执行；`IEnumerable<T>`；像 SQL 一样的 `from ... where ... select` 写法。

### 第二部分 EF Core 起步（先写类，再生成表）

**第 5 章 第一个 EF Core 程序：由代码生成表** —— `DbContext` 与 `DbSet`；第一次迁移（`Add-Migration` / `Update-Database`）建出 `Products` 表；`Add` + `SaveChanges` 写入数据，再用 LINQ 查出来。

**第 6 章 映射：约定与配置** —— EF Core 怎么决定表名、列名、主键和列类型；数据注解与 Fluent API；改了类之后再迁移一次。

**第 7 章 迁移进阶：改表、回滚、发布** —— 读懂迁移文件和模型快照；回滚与删除迁移；生成 SQL 脚本交给 DBA 发布到正式库。

**第 8 章 LINQ 是怎么变成 SQL 的** —— 打开日志看生成的 SQL；`IQueryable` 与 `IEnumerable` 的区别（在数据库里过滤还是在内存里过滤）；哪些写法翻译不了、会怎么报错。

### 第三部分 增删改

**第 9 章 增删改与变更跟踪** —— 实体状态；EF Core 怎么发现你改了哪个属性、为什么只更新改动的列；只读查询用 `AsNoTracking`。

**第 10 章 批量操作与原生 SQL** —— `ExecuteUpdate`/`ExecuteDelete` 一条语句改一批；LINQ 写不出来时用 `FromSql`、`SqlQuery`、`ExecuteSql`，参数化照样安全。

**第 11 章 事务与并发冲突** —— `SaveChanges` 自带事务；多次保存放进一个事务；两个人同时改同一行时，用 `rowversion` 发现冲突。

### 第四部分 表之间的关系

**第 12 章 一对多：外键与导航属性** —— 把"分类"从字符串拆成 `Categories` 表；外键属性与导航属性；用 `Include` 把关联数据一起查出来。

**第 13 章 加载策略与 N+1 问题** —— `ThenInclude` 与 `Select` 投影；N+1 问题是怎么产生的、怎么从日志里发现它；拆分查询。

**第 14 章 不一一对应的情况：多对多、复杂类型、继承** —— 中间表不用写类；一个类展开成同一张表里的多列；多个类共用一张表。

### 第五部分 用起来

**第 15 章 接入 ASP.NET Core Minimal API** —— `AddDbContext` 依赖注入；`DbContext` 为什么是"每个请求一个"；`ToListAsync` 等异步方法。

**第 16 章 综合项目：产品库存管理** —— 分类、产品、入库出库串成一个完整的 Web API（不引入新概念）。

### 附录

- 附录 A LINQ ↔ SQL 写法对照速查
- 附录 B 报错速查（全书实测过的报错原文 → 原因 → 改法 → 详见第几章）
- 附录 C 迁移命令速查（程序包管理器控制台与 `dotnet ef` 两种写法对照）
