// code/ch21/Scene7-Chinese/Program.cs
// 场景 7：中文变成问号——varchar 列、SQL 里没加 N 的字面量、IsAnsi 的 DbString
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);
conn.Open();                                   // 临时表只在当前会话里存在，所以全程用这一个打开的连接

// 三种列：varchar（跟随服务器默认排序规则）、nvarchar、指定中文排序规则的 varchar
conn.Execute(@"
    CREATE TABLE #Memo (
        Way        NVARCHAR(30),
        VarcharCol VARCHAR(20),
        NvarCol    NVARCHAR(20),
        CnVarchar  VARCHAR(20) COLLATE Chinese_PRC_CI_AS)");

// 写法 1：Dapper 参数（string 默认按 nvarchar 发送）
conn.Execute("INSERT #Memo VALUES (N'1 Dapper 参数', @Text, @Text, @Text)", new { Text = "成都" });

// 写法 2：SQL 里直接写字面量，忘了 N 前缀
conn.Execute("INSERT #Memo VALUES (N'2 字面量没加 N', '成都', '成都', '成都')");

// 写法 3：字面量加了 N
conn.Execute("INSERT #Memo VALUES (N'3 字面量加 N', N'成都', N'成都', N'成都')");

// 写法 4：第 15 章的 DbString，IsAnsi = true 表示按 varchar 发送
var ansi = new DbString { Value = "成都", IsAnsi = true, Length = 20 };
conn.Execute("INSERT #Memo VALUES (N'4 DbString IsAnsi', @Text, @Text, @Text)", new { Text = ansi });

Console.WriteLine("== 现象：写法 / varchar 列 / nvarchar 列 / 中文排序规则的 varchar 列");
foreach (var m in conn.Query<Memo>("SELECT Way, VarcharCol, NvarCol, CnVarchar FROM #Memo"))
    Console.WriteLine($"  {m.Way,-18}{m.VarcharCol,-6}{m.NvarCol,-6}{m.CnVarchar}");

Console.WriteLine("== 定位 1：库的排序规则，以及它对应的代码页");
var db = conn.QuerySingle<CollationInfo>(@"
    SELECT CAST(DATABASEPROPERTYEX(DB_NAME(), 'Collation') AS NVARCHAR(128)) AS Collation,
           CAST(COLLATIONPROPERTY(CAST(DATABASEPROPERTYEX(DB_NAME(), 'Collation') AS NVARCHAR(128)),
                'CodePage') AS INT) AS CodePage");
Console.WriteLine($"  {db.Collation}  代码页 {db.CodePage}");

Console.WriteLine("== 定位 2：看存进去的字节。问号是 0x3F，\"成都\"的 GBK 编码是 0xB3C9B6BC");
foreach (var b in conn.Query<BytesRow>(@"
    SELECT Way, CAST(VarcharCol AS VARBINARY(20)) AS Latin1Bytes,
           CAST(CnVarchar AS VARBINARY(20)) AS CnBytes
    FROM #Memo"))
    Console.WriteLine($"  {b.Way,-18}varchar 列 0x{Convert.ToHexString(b.Latin1Bytes!)}" +
                      $"   中文排序规则列 0x{Convert.ToHexString(b.CnBytes!)}");

public class Memo
{
    public string Way { get; set; } = "";
    public string? VarcharCol { get; set; }
    public string? NvarCol { get; set; }
    public string? CnVarchar { get; set; }
}

public class CollationInfo
{
    public string Collation { get; set; } = "";
    public int CodePage { get; set; }
}

public class BytesRow
{
    public string Way { get; set; } = "";
    public byte[]? Latin1Bytes { get; set; }
    public byte[]? CnBytes { get; set; }
}
