-- code/sql/ch15-tvp.sql
-- 第 15 章：创建两个表值参数类型（TVP）。可以反复执行。
-- 在 SSMS 里连接 PriceDemo 后整段执行（F5）。

USE PriceDemo;
GO

-- 类型被存储过程引用时不能直接删；本章没有存储过程用到它们，所以直接 DROP 再 CREATE
DROP TYPE IF EXISTS dbo.PriceRequestItemType;
DROP TYPE IF EXISTS dbo.ProductCodeType;
GO

-- 一行 = 一条申请明细。列的顺序很重要：DataTable 是按"第几列"对上的，不是按列名
CREATE TYPE dbo.PriceRequestItemType AS TABLE (
    RequestId    INT           NOT NULL,
    ProductId    INT           NOT NULL,
    Qty          INT           NOT NULL,
    RequestPrice DECIMAL(18,2) NOT NULL
);
GO

-- 一行 = 一个产品编码。练习 1 用：把一批编码一次传给服务器
CREATE TYPE dbo.ProductCodeType AS TABLE (
    Code VARCHAR(20) NOT NULL PRIMARY KEY      -- 和 dbo.Product.Code 类型一致，避免隐式转换
);
GO

-- 执行完应看到两行：PriceRequestItemType、ProductCodeType
SELECT name FROM sys.table_types WHERE schema_id = SCHEMA_ID('dbo') ORDER BY name;
