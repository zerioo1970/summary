-- code/sql/ch13-more-requests.sql
-- 用途：第 13 章分页与查询页演示。在初始 3 张申请单的基础上再加 7 张，共 10 张。
-- 可以反复执行：先删掉 Id > 3 的申请单（连同明细、审批记录），把自增种子退回 3，再插入。
-- 前提：已经执行过 00-reset-database.sql。

USE PriceDemo;
GO

-- 先删子表再删主表，否则外键会挡住 DELETE
DELETE FROM dbo.ApprovalLog      WHERE RequestId > 3;
DELETE FROM dbo.PriceRequestItem WHERE RequestId > 3;
DELETE FROM dbo.PriceRequest     WHERE Id > 3;

-- 种子退回 3：下一张申请单的 Id 一定是 4，重复执行结果也相同
DBCC CHECKIDENT ('dbo.PriceRequest', RESEED, 3) WITH NO_INFOMSGS;
GO

INSERT dbo.PriceRequest (RequestNo, CustomerId, Applicant, Status, CreatedAt, Remark) VALUES
 ('PR20261102', 3, N'王强', 1, '2026-11-02 09:00:00', NULL),            -- Id 4
 ('PR20261105', 2, N'李娜', 2, '2026-11-05 15:20:00', N'检测中心扩容'),  -- Id 5
 ('PR20261109', 1, N'张伟', 3, '2026-11-09 11:45:00', N'价格低于底价'),  -- Id 6
 ('PR20261112', 3, N'王强', 0, '2026-11-12 16:30:00', NULL),            -- Id 7
 ('PR20261118', 2, N'李娜', 1, '2026-11-18 10:05:00', NULL),            -- Id 8
 ('PR20261123', 1, N'张伟', 1, '2026-11-23 13:40:00', N'年底补单'),      -- Id 9
 ('PR20261128', 3, N'王强', 2, '2026-11-28 08:50:00', NULL);            -- Id 10
GO

-- 执行完应看到：申请单 10 张
SELECT COUNT(*) AS 申请单数 FROM dbo.PriceRequest;
