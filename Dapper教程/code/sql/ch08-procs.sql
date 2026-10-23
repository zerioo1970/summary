-- code/sql/ch08-procs.sql
-- 用途：第 8 章用到的 3 个存储过程。可以反复执行（CREATE OR ALTER，SQL Server 2016 SP1 起支持）。
-- 命名规则：动作前缀_业务名_Proc。在 SSMS 中连上 PriceDemo 库后整段执行（F5）。

USE PriceDemo;
GO

-- ① 按单号查一张申请单：只有输入参数，返回一个结果集（0 或 1 行）
CREATE OR ALTER PROCEDURE dbo.Select_PriceRequestByNo_Proc
    @RequestNo VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, RequestNo, CustomerId, Applicant, Status, CreatedAt, Remark
    FROM dbo.PriceRequest
    WHERE RequestNo = @RequestNo;
END
GO

-- ② 提交审批：只有"草稿(0)"能提交。改成"待审(1)"并写一条审批记录。
--    @NewStatus（OUTPUT）：执行后单据的状态；单号不存在时为 NULL
--    RETURN 0 = 成功，1 = 失败（单号不存在或不是草稿）
CREATE OR ALTER PROCEDURE dbo.Update_PriceRequestSubmit_Proc
    @RequestNo VARCHAR(20),
    @Operator  NVARCHAR(20),
    @NewStatus TINYINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Id INT;

    SELECT @Id = Id, @NewStatus = Status
    FROM dbo.PriceRequest
    WHERE RequestNo = @RequestNo;

    IF @Id IS NULL OR @NewStatus <> 0
        RETURN 1;                       -- 失败：@NewStatus 保留查到的当前状态（查不到就是 NULL）

    BEGIN TRAN;                         -- 两条写操作要么都成功要么都不做（事务第 9 章细讲）
        UPDATE dbo.PriceRequest SET Status = 1 WHERE Id = @Id;
        INSERT dbo.ApprovalLog (RequestId, Approver, Action) VALUES (@Id, @Operator, N'提交');
    COMMIT;

    SET @NewStatus = 1;
    RETURN 0;                           -- 成功
END
GO

-- ③ 按申请人分页查申请单：返回一页结果集，同时用 OUTPUT 参数带回总行数
CREATE OR ALTER PROCEDURE dbo.Select_PriceRequestPage_Proc
    @Applicant  NVARCHAR(20),
    @PageIndex  INT,                    -- 从 1 开始
    @PageSize   INT,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @TotalCount = COUNT(*)
    FROM dbo.PriceRequest
    WHERE Applicant = @Applicant;

    SELECT Id, RequestNo, Applicant, Status, CreatedAt
    FROM dbo.PriceRequest
    WHERE Applicant = @Applicant
    ORDER BY Id
    OFFSET (@PageIndex - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
