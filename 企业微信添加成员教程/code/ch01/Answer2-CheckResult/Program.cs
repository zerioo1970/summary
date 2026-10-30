// code/ch01/Answer2-CheckResult/Program.cs
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Data.SqlClient;

string connStr = "Server=.;Database=HRDB;Integrated Security=True;TrustServerCertificate=True;";
string corpId = "你的企业ID";
string secret = "通讯录同步的Secret";
string loginName = "zhangsan";

// 1. 读数据库
using var conn = new SqlConnection(connStr);
conn.Open();
var cmd = new SqlCommand("SELECT EmpName, Mobile FROM dbo.Employee WHERE LoginName = @LoginName", conn);
cmd.Parameters.AddWithValue("@LoginName", loginName);
using var reader = cmd.ExecuteReader();
reader.Read();
string name = reader.GetString(0);
string mobile = reader.GetString(1);

// 2. 取 access_token
using var http = new HttpClient();
string tokenJson = await http.GetStringAsync(
    $"https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid={corpId}&corpsecret={secret}");
string token = (string)JsonNode.Parse(tokenJson)!["access_token"]!;

// 3. 添加成员
var user = new { userid = loginName, name, mobile, department = new[] { 1 } };
var resp = await http.PostAsJsonAsync(
    $"https://qyapi.weixin.qq.com/cgi-bin/user/create?access_token={token}", user);

// 4. 解析返回：出错时 HTTP 状态码照样是 200，只能看 errcode
JsonNode result = JsonNode.Parse(await resp.Content.ReadAsStringAsync())!;
int errcode = (int)result["errcode"]!;
string errmsg = (string)result["errmsg"]!;

if (errcode == 0)
    Console.WriteLine("添加成功");
else
    Console.WriteLine($"添加失败：{errcode} {errmsg}");
