-- code/sql/ch18-add-discount.sql
-- 第 18 章：给申请明细 dbo.PriceRequestItem 加一列 Discount（折扣，0.95 表示九五折，NULL 表示不打折），并填入示例数据。
-- 可以反复执行：列已存在就不再加，数据每次都重新写一遍。
-- 在 SSMS 里连接 PriceDemo 后整段执行（F5）。

USE PriceDemo;
GO

IF COL_LENGTH('dbo.PriceRequestItem', 'Discount') IS NULL
    ALTER TABLE dbo.PriceRequestItem ADD Discount DECIMAL(5,2) NULL;   -- 允许 NULL：老数据没有折扣
GO

UPDATE dbo.PriceRequestItem SET Discount = NULL;                 -- 先清空，保证每次执行结果相同
UPDATE dbo.PriceRequestItem SET Discount = 0.95 WHERE Id = 1;    -- 第 1 单的主机：九五折
UPDATE dbo.PriceRequestItem SET Discount = 0.90 WHERE Id = 3;    -- 第 2 单的彩超：九折
GO

-- 执行完应看到 5 行，只有 Id 1、3 有折扣
SELECT Id, RequestId, ProductId, Qty, RequestPrice, Discount
FROM dbo.PriceRequestItem
ORDER BY Id;
