# 企业微信添加成员教程（C# + ADO.NET）

> 版本基线：.NET 10 + C# 14 + Visual Studio 2026｜`Microsoft.Data.SqlClient 7.1.1`（另引用 `System.Security.Cryptography.Pkcs 10.0.14` 消除 NU1903 警告）｜SQL Server｜企业微信服务端 API（`gettoken`、`user/create`）
>
> HTTP 与 JSON 只用 .NET 自带的 `HttpClient`、`System.Text.Json`，不引入第三方企业微信 SDK。

---

## 这本教程写给谁

- 熟悉 C# 和 ADO.NET，能用 `SqlConnection` / `SqlCommand` 查 SQL Server。
- 没有调用过企业微信接口。
- 想要一个 30 多行就能跑通的程序：从员工表读出一个人，加进企业微信通讯录。

**最短路径**：只有第 1 章，约 60 分钟（含企业微信后台配置）。
**完整路径**：同上，共 1 章。

---

## 进度

| 章节 | 状态 |
| --- | --- |
| **第 1 章 把数据库里的员工添加到企业微信** | ✅ |

---

## 目录

**[第 1 章 把数据库里的员工添加到企业微信](docs/ch01-add-member.md)**
- 企业微信后台准备：企业ID、通讯录同步 Secret、企业可信IP
- 1.1 整体流程：读库 → 取 token → 创建成员
- 1.2 access_token：先换通行证
- 1.3 用匿名对象发 JSON：调用 `user/create`
- ⚠️ 常见错误码：60020 / 60011 / 41001 / 60102

---

## 代码

| 目录 | 内容 |
| --- | --- |
| `code/ch01/sql/` | 建 HRDB 示例库和 Employee 表 |
| `code/ch01/Step1-GetToken/` | 只取 access_token |
| `code/ch01/Step2-AddMember/` | 完整程序：读库并添加成员 |
| `code/ch01/Answer1-Position/` | 练习 1 参考答案 |
| `code/ch01/Answer2-CheckResult/` | 练习 2 参考答案 |

## 验证情况

- 所有项目在 .NET 10 SDK 10.0.403 下编译通过，0 警告 0 错误。
- 用错误的企业ID和无效 token 调用了真实的企业微信服务器，教程中 40013 / 41001 / 40014 的输出是实际运行结果。
- 读数据库部分、真实添加成功的输出未实机验证（需要真实企业ID、Secret 和可信 IP），正文中已标注。
