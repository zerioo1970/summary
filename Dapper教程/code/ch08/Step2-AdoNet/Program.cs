// code/ch08/Step2-AdoNet/Program.cs
// 对照组：用 ADO.NET 调"提交审批"过程，拿 OUTPUT 参数和 RETURN 返回值
// 运行前先执行 code/sql/ch08-procs.sql
using System.Data;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);
conn.Open();

using var cmd = new SqlCommand("dbo.Update_PriceRequestSubmit_Proc", conn);
cmd.CommandType = CommandType.StoredProcedure;

// 输入参数
cmd.Parameters.Add("@RequestNo", SqlDbType.VarChar, 20).Value = "PR20261015";
cmd.Parameters.Add("@Operator", SqlDbType.NVarChar, 20).Value = "张伟";

// OUTPUT 参数：声明类型 + 方向，三行
var pStatus = cmd.Parameters.Add("@NewStatus", SqlDbType.TinyInt);
pStatus.Direction = ParameterDirection.Output;

// RETURN 返回值：也是一个"参数"，方向是 ReturnValue
var pReturn = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
pReturn.Direction = ParameterDirection.ReturnValue;

cmd.ExecuteNonQuery();

// 执行完才能读；值可能是 DBNull，要自己判断
int ret = (int)pReturn.Value;
string status = pStatus.Value == DBNull.Value ? "NULL" : pStatus.Value.ToString()!;
Console.WriteLine($"ADO.NET：返回值 = {ret}，新状态 = {status}");
