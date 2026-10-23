-- code/sql/ch07-legacy-view.sql
-- 用途：第 7 章模拟一张"老系统的表"——列名全是下划线风格（request_no 这种）。
-- 做法是建一个视图，用 AS 把 PriceRequest 的列改成下划线名字。可以反复执行。
-- 在 SSMS 中连上 PriceDemo 库后整段执行（F5）。

USE PriceDemo;
GO

CREATE OR ALTER VIEW dbo.legacy_price_request
AS
SELECT Id         AS request_id,
       RequestNo  AS request_no,
       Applicant  AS applicant_name,
       CreatedAt  AS created_at
FROM dbo.PriceRequest;
GO
