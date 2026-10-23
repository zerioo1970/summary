// code/ch08/Exercise/Program.cs
// 第 8 章练习参考答案：调审批过程两次，再用查询过程确认状态，最后列出审批记录
// 运行前先执行 code/sql/ch08-procs.sql 和 code/sql/ch08-exercise.sql
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

for (int i = 1; i <= 2; i++)
{
    // 输入参数用匿名对象一次给齐；Output 和 ReturnValue 再单独 Add
    var p = new DynamicParameters(new
    {
        RequestNo = "PR20261008",
        Approver = "王经理",
        Pass = true,
        Comment = "同意",
    });
    p.Add("@NewStatus", dbType: DbType.Byte, direction: ParameterDirection.Output);
    p.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

    conn.Execute("dbo.Update_PriceRequestApprove_Proc", p,
        commandType: CommandType.StoredProcedure);

    Console.WriteLine($"第 {i} 次审批 PR20261008：返回值 {p.Get<int>("@ReturnValue")}，" +
                      $"新状态 {p.Get<byte?>("@NewStatus")}");
}

// 用第 8 章的查询过程确认状态
var row = conn.QuerySingleOrDefault<RequestRow>(
    "dbo.Select_PriceRequestByNo_Proc",
    new { RequestNo = "PR20261008" },
    commandType: CommandType.StoredProcedure);
Console.WriteLine($"查询过程确认：{row?.RequestNo} 状态 = {row?.Status}");

// 审批记录：ISNULL 把没写意见的 NULL 换成"无"，按时间先后列出
var logs = conn.Query<string>(@"
    SELECT l.Approver + N' ' + l.Action + N'（' + ISNULL(l.Comment, N'无') + N'）'
    FROM dbo.ApprovalLog l
    WHERE l.RequestId = @Id
    ORDER BY l.Id", new { row!.Id });
Console.WriteLine("审批记录：" + string.Join(" / ", logs));

public enum RequestStatus : byte { Draft = 0, Pending = 1, Approved = 2, Rejected = 3 }

public class RequestRow
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public RequestStatus Status { get; set; }
}
