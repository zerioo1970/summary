-- code/sql/ch17-json-column.sql
-- 第 17 章：给 dbo.Product 加一列 Spec（JSON 文本，存产品规格），并填入示例数据。
-- 可以反复执行：列已存在就不再加，数据每次都重新写一遍。
-- 在 SSMS 里连接 PriceDemo 后整段执行（F5）。

USE PriceDemo;
GO

IF COL_LENGTH('dbo.Product', 'Spec') IS NULL
    ALTER TABLE dbo.Product ADD Spec NVARCHAR(MAX) NULL;   -- 允许 NULL：不是每个产品都录了规格
GO

-- JSON 里的名字和 C# 类 ProductSpec 的属性名一致：频率（MHz）、重量（kg）、类别
UPDATE dbo.Product SET Spec = N'{"FrequencyMHz":5.0,"WeightKg":4.2,"Category":"主机"}'  WHERE Code = 'US-100';
UPDATE dbo.Product SET Spec = N'{"FrequencyMHz":7.5,"WeightKg":65.0,"Category":"主机"}' WHERE Code = 'US-200';
UPDATE dbo.Product SET Spec = N'{"FrequencyMHz":10.0,"WeightKg":0.3,"Category":"探头"}' WHERE Code = 'PB-01';
UPDATE dbo.Product SET Spec = NULL                                                       WHERE Code = 'PB-02';
GO

-- 执行完应看到 4 行，PB-02 的 Spec 是 NULL
SELECT Code, Spec FROM dbo.Product ORDER BY Id;
