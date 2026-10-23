-- code/sql/ch19-xe-start.sql
-- 第 19 章：建一个扩展事件会话 Ch19_Capture，只抓应用程序名为 Ch19Demo 的连接发来的语句，存进内存（ring_buffer）。
-- 可以反复执行：会话已存在就先删掉重建。需要较高权限（sysadmin，或 ALTER ANY EVENT SESSION 一类权限），生产库请找 DBA。
-- 在 SSMS 里连接服务器后整段执行（F5）。

IF EXISTS (SELECT 1 FROM sys.server_event_sessions WHERE name = N'Ch19_Capture')
    DROP EVENT SESSION Ch19_Capture ON SERVER;
GO

CREATE EVENT SESSION Ch19_Capture ON SERVER
    -- 带参数的命令、存储过程：客户端用 RPC 方式发送，完成时触发 rpc_completed
    ADD EVENT sqlserver.rpc_completed (
        WHERE sqlserver.client_app_name = N'Ch19Demo'),
    -- 不带参数的普通 SQL：客户端按"批处理"发送，完成时触发 sql_batch_completed
    ADD EVENT sqlserver.sql_batch_completed (
        WHERE sqlserver.client_app_name = N'Ch19Demo')
    ADD TARGET package0.ring_buffer          -- 存在内存里，会话删掉就没了；够调试用
    WITH (MAX_DISPATCH_LATENCY = 1 SECONDS); -- 默认 30 秒才把事件写进目标；改成 1 秒，跑完马上能查到
GO

ALTER EVENT SESSION Ch19_Capture ON SERVER STATE = START;
GO

-- 执行完应看到一行：Ch19_Capture
SELECT name FROM sys.dm_xe_sessions WHERE name = N'Ch19_Capture';
