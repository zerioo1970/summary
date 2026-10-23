-- code/sql/ch08-exercise.sql
-- 用途：第 8 章练习的参考答案——"审批"存储过程。可以反复执行。
-- 规则：只有"待审(1)"的单据能审批；通过改成 2，驳回改成 3，并写一条审批记录。
--   @NewStatus（OUTPUT）：执行后单据的状态；单号不存在时为 NULL
--   RETURN 0 = 成功，1 = 失败

USE PriceDemo;
GO

CREATE OR ALTER PROCEDURE dbo.Update_PriceRequestApprove_Proc
    @RequestNo VARCHAR(20),
    @Approver  NVARCHAR(20),
    @Pass      BIT,                     -- 1 通过，0 驳回
    @Comment   NVARCHAR(200) = NULL,    -- 审批意见，可以不传
    @NewStatus TINYINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Id INT;

    SELECT @Id = Id, @NewStatus = Status
    FROM dbo.PriceRequest
    WHERE RequestNo = @RequestNo;

    IF @Id IS NULL OR @NewStatus <> 1
        RETURN 1;

    SET @NewStatus = CASE WHEN @Pass = 1 THEN 2 ELSE 3 END;

    BEGIN TRAN;
        UPDATE dbo.PriceRequest SET Status = @NewStatus WHERE Id = @Id;
        INSERT dbo.ApprovalLog (RequestId, Approver, Action, Comment)
        VALUES (@Id, @Approver, CASE WHEN @Pass = 1 THEN N'通过' ELSE N'驳回' END, @Comment);
    COMMIT;

    RETURN 0;
END
GO
