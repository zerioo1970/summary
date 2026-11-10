# 第 3 章 · 第一步：SQL Server 建库建表

> 上一章：[02-环境准备](02-环境准备.md) ｜ 下一章：[04-创建SpringBoot后端](04-创建SpringBoot后端.md)

## 3.1 开启 SQL Server 网络与登录

1. 打开 **SQL Server Configuration Manager（配置管理器）**。
2. 「SQL Server 网络配置」→「MSSQLSERVER 的协议」→ 右键 **TCP/IP** → **启用**。
3. 双击 TCP/IP →「IP 地址」标签 → 拉到最底部 `IPAll` → 把 **TCP 端口** 设为 `1433`。
4. 重启「SQL Server」服务。
5. 用 **SSMS（SQL Server Management Studio）** 连接，右键服务器 →「属性」→「安全性」→ 勾选 **「SQL Server 和 Windows 身份验证模式」**（即混合模式），这样才能用账号密码从 Java 连。
6. 建一个登录账号（示例用 `sa`，生产环境请另建专用账号）。

## 3.2 建库建表脚本

在 SSMS 里新建查询，执行下面的脚本。我们做一个最经典的「用户表」。

```sql
-- 1. 创建数据库
IF DB_ID('demo_db') IS NULL
    CREATE DATABASE demo_db;
GO

USE demo_db;
GO

-- 2. 创建用户表
IF OBJECT_ID('dbo.sys_user', 'U') IS NULL
CREATE TABLE dbo.sys_user (
    id          BIGINT IDENTITY(1,1) PRIMARY KEY,   -- 自增主键
    username    NVARCHAR(50)  NOT NULL,             -- 用户名
    age         INT           NULL,                 -- 年龄
    email       NVARCHAR(100) NULL,                 -- 邮箱
    create_time DATETIME      NOT NULL DEFAULT GETDATE()  -- 创建时间
);
GO

-- 3. 插入几条测试数据
INSERT INTO dbo.sys_user (username, age, email) VALUES
    (N'张三', 25, N'zhangsan@test.com'),
    (N'李四', 30, N'lisi@test.com'),
    (N'王五', 28, N'wangwu@test.com');
GO

SELECT * FROM dbo.sys_user;
```

**字段说明**：
- `IDENTITY(1,1)`：SQL Server 的自增列，等价于 MySQL 的 `AUTO_INCREMENT`。MyBatis-Flex 里对应主键策略 `KeyType.Auto`。
- `NVARCHAR`：存储 Unicode（能存中文），前面加 `N'...'` 表示 Unicode 字符串字面量。
- `GETDATE()`：数据库端默认当前时间。

执行后应能看到 3 行数据。数据库端准备完成 ✅

---

> 下一章 👉 [04-创建SpringBoot后端](04-创建SpringBoot后端.md)
