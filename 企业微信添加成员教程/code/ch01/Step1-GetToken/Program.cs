// code/ch01/Step1-GetToken/Program.cs
using System.Text.Json.Nodes;

string corpId = "你的企业ID";
string secret = "通讯录同步的Secret";

using var http = new HttpClient();

// 用企业ID + Secret 换 access_token，后面调其他接口都要带上它
string tokenJson = await http.GetStringAsync(
    $"https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid={corpId}&corpsecret={secret}");
Console.WriteLine(tokenJson);

// 从返回的 JSON 里按字段名取出 access_token
string token = (string)JsonNode.Parse(tokenJson)!["access_token"]!;
Console.WriteLine("access_token = " + token);
