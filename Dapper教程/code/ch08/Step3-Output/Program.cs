// code/ch08/Step3-Output/Program.cs
// 用 DynamicParameters 调"提交审批"过程：输入参数、OUTPUT 参数、RETURN 返回值
// 运行前先执行 code/sql/ch08-procs.sql
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

Submit("PR20261015");   // 3 号单是草稿 → 成功
Submit("PR20261015");   // 再提交一次 → 已经是待审，失败
Submit("PR20261001");   // 1 号单早已通过 → 失败
Submit("PR99999999");   // 单号不存在 → 失败，NewStatus 是 NULL

// 过程里写的审批记录
var logs = conn.Query<string>(@"
    SELECT l.Approver + N' ' + l.Action
    FROM dbo.ApprovalLog l JOIN dbo.PriceRequest r ON r.Id = l.RequestId
    WHERE r.RequestNo = 'PR20261015'");
Console.WriteLine("PR20261015 的审批记录：" + string.Join("、", logs));

void Submit(string requestNo)
{
    var p = new DynamicParameters();
    p.Add("@RequestNo", requestNo);                     // 输入参数：名字 + 值，类型 Dapper 推断
    p.Add("@Operator", "张伟");
    p.Add("@NewStatus", dbType: DbType.Byte,            // OUTPUT 参数：不给值，给类型和方向
          direction: ParameterDirection.Output);
    p.Add("@ReturnValue", dbType: DbType.Int32,         // RETURN 返回值：名字随便起
          direction: ParameterDirection.ReturnValue);

    int rows = conn.Execute("dbo.Update_PriceRequestSubmit_Proc", p,
        commandType: CommandType.StoredProcedure);

    // Execute 返回之后，过程已经执行完，这时才能 Get
    int ret = p.Get<int>("@ReturnValue");
    byte? status = p.Get<byte?>("@NewStatus");          // 可能是 NULL，所以用 byte?
    Console.WriteLine($"{requestNo}：返回值 {ret}（{(ret == 0 ? "成功" : "失败")}），" +
                      $"新状态 {(status is null ? "NULL" : status)}，Execute 返回 {rows}");
}
