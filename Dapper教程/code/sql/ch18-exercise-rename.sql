-- code/sql/ch18-exercise-rename.sql
-- 第 18 章练习：DBA 把明细表的 Discount 列改名为 DiscountRate。
-- 先执行 ch18-add-discount.sql，再执行本脚本。可以反复执行：已经改过名就什么也不做。
-- 在 SSMS 里连接 PriceDemo 后整段执行（F5）。

USE PriceDemo;
GO

IF COL_LENGTH('dbo.PriceRequestItem', 'Discount') IS NOT NULL
   AND COL_LENGTH('dbo.PriceRequestItem', 'DiscountRate') IS NULL
    EXEC sp_rename 'dbo.PriceRequestItem.Discount', 'DiscountRate', 'COLUMN';
GO

-- 执行完应看到列名 DiscountRate
SELECT Id, RequestId, DiscountRate FROM dbo.PriceRequestItem ORDER BY Id;
