# SQL 语句转换为存储过程规则

> 适用环境：**C# ASP.NET WebForm + SQL Server**
> 用途：把页面里的内联 SQL（`CommandType.Text`）改写为**存储过程**。
> 版本：v1.0 ｜ 最后更新：2026-06-29

---

## 总原则

把页面里的内联 SQL 拆成两部分：

1. **SQL Server 端**：新建一个存储过程。
2. **C# 端**：把代码改成"调用存储过程"。

> 核心要求：**参数的类型、长度、名字保持一一对应。**

---

## A. SQL Server 端存储过程脚本

### ① 脚本头固定模板

```sql
USE [sankodata]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<这里写功能说明>
-- =============================================
CREATE PROCEDURE [dbo].[动作_业务名_Proc]
	@参数1  类型(长度),
	@参数2  类型(长度)
AS
	-- 原来的 SQL 主体，把字面值换成 @参数
```

### ② 命名规范：`动作前缀_业务名_Proc`

| 操作 | 前缀 | 例子 |
|------|------|------|
| 插入 | `Insert_` | `Insert_超声部特殊价格申请_AttachFile_Proc` |
| 更新 | `Update_` | `Update_bx_Sales_Proc` |
| 删除 | `Delete_` | `Delete_bx_SalesBybxid_Proc` |
| 查询 | `Select_` | `Select_AttachFileById_Proc` |

### ③ 新建 / 修改

- **新建**用 `CREATE PROCEDURE [dbo].[名称]`
- **以后修改**用 `ALTER PROCEDURE [dbo].[名称]`（其余不变）

### ④ 参数类型对照表（C# `SqlDbType` → T-SQL 类型）

| C# SqlDbType   | T-SQL 类型      |
|----------------|-----------------|
| `VarChar, n`   | `varchar(n)`    |
| `NVarChar, n`  | `nvarchar(n)`   |
| `Char, n`      | `char(n)`       |
| `Int`          | `int`           |
| `DateTime`     | `datetime`      |
| `Decimal`      | `decimal(p,s)`  |
| `Bit`          | `bit`           |

> 长度必须和 C# 里 `SqlParameter` 的 size 一致。

---

## B. C# WebForm 端调用代码

固定套路：

```csharp
SqlCommand myCommand = new SqlCommand("存储过程名", myConnection);
    myCommand.CommandType = CommandType.StoredProcedure;   // ← 关键，必加

    // 每个参数三行：声明 → 方向 → 赋值
    myCommand.Parameters.Add(new SqlParameter("@参数名", SqlDbType.类型, 长度));
    myCommand.Parameters["@参数名"].Direction = ParameterDirection.Input;
    myCommand.Parameters["@参数名"].Value = 控件值;
    // ...其余参数同理

    myConnection.Open();
    try
    {
        myCommand.ExecuteNonQuery();   // 查询用 ExecuteReader / 取单值用 ExecuteScalar
    }
    catch
    {
        Response.Write("<script language='javascript'>alert('Error！');</" + "script>");
        return;
    }
    myConnection.Close();
```

---

## C. 细节约定

1. **`DateTime` 参数**直接传 `DateTime.Now`，不要 `.ToString()`。
2. **参数名大小写统一**（SQL Server 不区分，但保持整齐美观）。
3. **`myConnection.Open()`** 放在 `try` 之前，**`myConnection.Close()`** 放在最后。
4. **跨库调用 MySQL** 时，在过程内部用动态 SQL：

   ```sql
   DECLARE @sql NVARCHAR(MAX);
   SET @sql = 'CALL 过程名(''' + @参数 + ''', ''值'')';
   EXECUTE (@sql) AT 链接服务器名;   -- 例如 AT inputiv
   ```

---

## 完整示例（参考）

### 原始内联 SQL（改造前）

```csharp
String insertCmd = "insert into 超声部特殊价格申请_AttachFileTable (申请号码, 文件类别, FileName, Location, UploadTime, Uploader) values (@申请号码, @文件类别, @FileName, @Location, @UploadTime, @Uploader)";
SqlCommand myCommand = new SqlCommand(insertCmd, myConnection);
// ... 各参数赋值 ...
myCommand.Connection.Open();
myCommand.ExecuteNonQuery();
myCommand.Connection.Close();
```

### 改造后 —— SQL Server 存储过程

```sql
USE [sankodata]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	插入超声部特殊价格申请附件记录
-- =============================================
CREATE PROCEDURE [dbo].[Insert_超声部特殊价格申请_AttachFile_Proc]
	@申请号码  varchar(10),
	@文件类别  nvarchar(10),
	@FileName   nvarchar(30),
	@Location   nvarchar(50),
	@UploadTime datetime,
	@Uploader   nvarchar(50)
AS
	insert into 超声部特殊价格申请_AttachFileTable
		(申请号码, 文件类别, FileName, Location, UploadTime, Uploader)
	values
		(@申请号码, @文件类别, @FileName, @Location, @UploadTime, @Uploader)
```

### 改造后 —— C# 调用代码

```csharp
SqlCommand myCommand = new SqlCommand("Insert_超声部特殊价格申请_AttachFile_Proc", myConnection);
    myCommand.CommandType = CommandType.StoredProcedure;

    myCommand.Parameters.Add(new SqlParameter("@申请号码", SqlDbType.VarChar, 10));
    myCommand.Parameters["@申请号码"].Direction = ParameterDirection.Input;
    myCommand.Parameters["@申请号码"].Value = 申请号码.Text;

    myCommand.Parameters.Add(new SqlParameter("@文件类别", SqlDbType.NVarChar, 10));
    myCommand.Parameters["@文件类别"].Direction = ParameterDirection.Input;
    myCommand.Parameters["@文件类别"].Value = 文件类别.SelectedItem.Text;

    myCommand.Parameters.Add(new SqlParameter("@FileName", SqlDbType.NVarChar, 30));
    myCommand.Parameters["@FileName"].Direction = ParameterDirection.Input;
    myCommand.Parameters["@FileName"].Value = fileName;

    myCommand.Parameters.Add(new SqlParameter("@Location", SqlDbType.NVarChar, 50));
    myCommand.Parameters["@Location"].Direction = ParameterDirection.Input;
    myCommand.Parameters["@Location"].Value = "./Uploads/" + Text1.Value + extName;

    myCommand.Parameters.Add(new SqlParameter("@UploadTime", SqlDbType.DateTime));
    myCommand.Parameters["@UploadTime"].Direction = ParameterDirection.Input;
    myCommand.Parameters["@UploadTime"].Value = DateTime.Now;

    myCommand.Parameters.Add(new SqlParameter("@Uploader", SqlDbType.NVarChar, 50));
    myCommand.Parameters["@Uploader"].Direction = ParameterDirection.Input;
    myCommand.Parameters["@Uploader"].Value = User.Identity.Name.Substring(4);

    myConnection.Open();
    try
    {
        myCommand.ExecuteNonQuery();
    }
    catch
    {
        Response.Write("<script language='javascript'>alert('Error！');</" + "script>");
        return;
    }
    myConnection.Close();
```

---

## 修改记录

| 日期 | 版本 | 修改内容 |
|------|------|----------|
| 2026-06-29 | v1.0 | 初版 |
