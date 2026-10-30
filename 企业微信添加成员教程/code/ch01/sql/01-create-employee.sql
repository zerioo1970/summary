-- code/ch01/sql/01-create-employee.sql
-- 在 SSMS 里执行。HRDB 只是示例库名，已有员工表可以跳过本脚本
IF DB_ID(N'HRDB') IS NULL
    CREATE DATABASE HRDB;
GO
USE HRDB;
GO
IF OBJECT_ID(N'dbo.Employee', N'U') IS NOT NULL
    DROP TABLE dbo.Employee;
GO
CREATE TABLE dbo.Employee (
    LoginName varchar(64)  NOT NULL PRIMARY KEY,  -- 登录名，用作企业微信 userid
    EmpName   nvarchar(50) NOT NULL,              -- 姓名
    Mobile    varchar(20)  NULL,                  -- 手机号
    Position  nvarchar(50) NULL                   -- 职务（练习 1 用）
);
GO
-- 手机号请换成真实可用的号码，企业微信会按手机号发邀请
INSERT INTO dbo.Employee (LoginName, EmpName, Mobile, Position) VALUES
    ('zhangsan', N'张三', '13800000000', N'销售'),
    ('lisi',     N'李四', '13900000000', N'采购主管');
GO
