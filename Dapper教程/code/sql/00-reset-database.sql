-- code/sql/00-reset-database.sql
-- 用途：创建（或重置）整本教程使用的示例库 PriceDemo。
-- 可以反复执行：每次都会删掉 5 张表重建，并灌入同一批示例数据。
-- 任何一章把数据改乱了，重新跑一遍这个脚本即可回到初始状态。
-- 在 SSMS 中连接你的 SQL Server 实例后，整段执行（F5）。

IF DB_ID(N'PriceDemo') IS NULL
    CREATE DATABASE PriceDemo;
GO

USE PriceDemo;
GO

-- 先删子表再删主表，否则外键会挡住 DROP
DROP TABLE IF EXISTS dbo.ApprovalLog;
DROP TABLE IF EXISTS dbo.PriceRequestItem;
DROP TABLE IF EXISTS dbo.PriceRequest;
DROP TABLE IF EXISTS dbo.Product;
DROP TABLE IF EXISTS dbo.Customer;
GO

-- 客户
CREATE TABLE dbo.Customer (
    Id      INT IDENTITY(1,1) PRIMARY KEY,
    Name    NVARCHAR(50) NOT NULL,      -- 客户名称
    City    NVARCHAR(20) NOT NULL,      -- 所在城市
    Level   CHAR(1)      NOT NULL       -- 客户等级：A/B/C
);

-- 产品
CREATE TABLE dbo.Product (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    Code      VARCHAR(20)   NOT NULL UNIQUE,  -- 产品编码
    Name      NVARCHAR(50)  NOT NULL,         -- 产品名称
    ListPrice DECIMAL(18,2) NOT NULL          -- 目录价
);

-- 价格申请单（主表）
CREATE TABLE dbo.PriceRequest (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    RequestNo  VARCHAR(20)   NOT NULL UNIQUE,              -- 申请单号
    CustomerId INT           NOT NULL REFERENCES dbo.Customer(Id),
    Applicant  NVARCHAR(20)  NOT NULL,                     -- 申请人
    Status     TINYINT       NOT NULL DEFAULT 0,           -- 0 草稿 1 待审 2 通过 3 驳回
    CreatedAt  DATETIME2(0)  NOT NULL DEFAULT SYSDATETIME(),
    Remark     NVARCHAR(200) NULL                          -- 允许为空，第 7 章讲 NULL 映射时用
);

-- 申请明细（子表）
CREATE TABLE dbo.PriceRequestItem (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    RequestId    INT           NOT NULL REFERENCES dbo.PriceRequest(Id),
    ProductId    INT           NOT NULL REFERENCES dbo.Product(Id),
    Qty          INT           NOT NULL,
    RequestPrice DECIMAL(18,2) NOT NULL                    -- 申请价
);

-- 审批记录
CREATE TABLE dbo.ApprovalLog (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    RequestId INT           NOT NULL REFERENCES dbo.PriceRequest(Id),
    Approver  NVARCHAR(20)  NOT NULL,
    Action    NVARCHAR(10)  NOT NULL,                      -- 提交 / 通过 / 驳回
    Comment   NVARCHAR(200) NULL,
    ActedAt   DATETIME2(0)  NOT NULL DEFAULT SYSDATETIME()
);
GO

-- ===== 示例数据 =====
INSERT dbo.Customer (Name, City, Level) VALUES
 (N'华东第一医院', N'上海', 'A'),
 (N'江南检测中心', N'杭州', 'B'),
 (N'北方影像诊所', N'天津', 'C');

INSERT dbo.Product (Code, Name, ListPrice) VALUES
 ('US-100', N'便携超声主机',   180000.00),
 ('US-200', N'彩超工作站',     420000.00),
 ('PB-01',  N'线阵探头',        26000.00),
 ('PB-02',  N'凸阵探头',        24000.00);

INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant, Status, CreatedAt, Remark) VALUES
 ('PR20261001', 1, N'张伟', 2, '2026-10-01 09:30:00', N'年度框架采购'),
 ('PR20261008', 2, N'李娜', 1, '2026-10-08 14:10:00', NULL),
 ('PR20261015', 1, N'张伟', 0, '2026-10-15 10:00:00', N'追加探头');

INSERT dbo.PriceRequestItem (RequestId, ProductId, Qty, RequestPrice) VALUES
 (1, 1, 2, 165000.00),
 (1, 3, 4,  23000.00),
 (2, 2, 1, 395000.00),
 (3, 3, 2,  24500.00),
 (3, 4, 2,  22500.00);

INSERT dbo.ApprovalLog (RequestId, Approver, Action, Comment, ActedAt) VALUES
 (1, N'张伟', N'提交', NULL,           '2026-10-01 09:35:00'),
 (1, N'王经理', N'通过', N'同意框架价', '2026-10-02 11:00:00'),
 (2, N'李娜', N'提交', NULL,           '2026-10-08 14:15:00');
GO

-- 执行完应看到 3 / 4 / 3 / 5 / 3
SELECT (SELECT COUNT(*) FROM dbo.Customer)         AS 客户数,
       (SELECT COUNT(*) FROM dbo.Product)          AS 产品数,
       (SELECT COUNT(*) FROM dbo.PriceRequest)     AS 申请单数,
       (SELECT COUNT(*) FROM dbo.PriceRequestItem) AS 明细数,
       (SELECT COUNT(*) FROM dbo.ApprovalLog)      AS 审批记录数;
