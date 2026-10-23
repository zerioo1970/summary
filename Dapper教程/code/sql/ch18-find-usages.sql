-- code/sql/ch18-find-usages.sql
-- 第 18 章：改表之前，先查数据库里有哪些存储过程 / 视图 / 函数引用了这张表。
-- 只读查询，可以反复执行。把 @Table 的值换成你要改的表。
-- 在 SSMS 里连接 PriceDemo 后整段执行（F5）。

USE PriceDemo;
GO

DECLARE @Table SYSNAME = N'PriceRequest';   -- 要改的表（不带架构名）

SELECT OBJECT_SCHEMA_NAME(d.referencing_id) + '.' + OBJECT_NAME(d.referencing_id) AS 引用它的对象,
       o.type_desc                                                               AS 对象类型
FROM sys.sql_expression_dependencies AS d
JOIN sys.objects AS o ON o.object_id = d.referencing_id
WHERE d.referenced_entity_name = @Table
ORDER BY 1;
