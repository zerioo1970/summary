-- code/sql/ch19-xe-stop.sql
-- 第 19 章：用完把会话停掉并删除。扩展事件开着会持续占用一点资源，调试完就关。可以反复执行。

IF EXISTS (SELECT 1 FROM sys.dm_xe_sessions WHERE name = N'Ch19_Capture')
    ALTER EVENT SESSION Ch19_Capture ON SERVER STATE = STOP;
GO

IF EXISTS (SELECT 1 FROM sys.server_event_sessions WHERE name = N'Ch19_Capture')
    DROP EVENT SESSION Ch19_Capture ON SERVER;
GO

-- 执行完应看到 0
SELECT COUNT(*) AS 剩余会话数 FROM sys.server_event_sessions WHERE name = N'Ch19_Capture';
