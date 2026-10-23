-- code/sql/ch19-xe-read.sql
-- 第 19 章：读出 Ch19_Capture 会话抓到的语句。只读查询，可以反复执行。
-- 先执行 ch19-xe-start.sql，再运行程序，等 1 秒以上，然后在 SSMS 里执行本脚本（F5）。

SELECT
    e.value('@name', 'varchar(30)')                                AS 事件,
    e.value('(data[@name="row_count"]/value)[1]', 'bigint')        AS 行数,
    e.value('(data[@name="duration"]/value)[1]', 'bigint')         AS 耗时微秒,
    COALESCE(e.value('(data[@name="statement"]/value)[1]', 'nvarchar(max)'),    -- rpc_completed 的语句在 statement 里
             e.value('(data[@name="batch_text"]/value)[1]', 'nvarchar(max)'))   -- sql_batch_completed 的在 batch_text 里
                                                                   AS 服务器收到的语句
FROM (
    SELECT CAST(t.target_data AS XML) AS x
    FROM sys.dm_xe_sessions AS s
    JOIN sys.dm_xe_session_targets AS t ON t.event_session_address = s.address
    WHERE s.name = N'Ch19_Capture' AND t.target_name = N'ring_buffer'
) AS d
CROSS APPLY d.x.nodes('/RingBufferTarget/event') AS n(e)
ORDER BY e.value('@timestamp', 'datetime2(7)');                    -- 按发生时间排序
