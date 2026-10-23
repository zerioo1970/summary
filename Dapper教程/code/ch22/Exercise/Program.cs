// code/ch22/Exercise/Program.cs
// 第 22 章练习参考答案：GET /requests?applicant=张伟，按申请人筛选（可不传），接上取消令牌
using System.Data;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

string connStr = builder.Configuration.GetConnectionString("PriceDemo")
    ?? throw new InvalidOperationException("appsettings.json 里没有 ConnectionStrings:PriceDemo");
builder.Services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connStr));
builder.Services.AddScoped<PriceRequestRepository>();

var app = builder.Build();

// string? applicant：简单类型、路由里又没有同名参数 → 框架从查询字符串 ?applicant= 里取；不传就是 null
app.MapGet("/requests", async (string? applicant, PriceRequestRepository repo, CancellationToken ct) =>
    await repo.SearchAsync(applicant, ct));

app.Run();

public interface IDbConnectionFactory
{
    IDbConnection Create();
}

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connStr;
    public SqlConnectionFactory(string connStr) => _connStr = connStr;
    public IDbConnection Create() => new SqlConnection(_connStr);
}

public class PriceRequestRepository
{
    private readonly IDbConnectionFactory _factory;
    public PriceRequestRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<List<RequestBrief>> SearchAsync(string? applicant, CancellationToken ct)
    {
        // 第 13 章：只拼固定的 SQL 片段，值永远走参数
        var sql = new StringBuilder("SELECT Id, RequestNo, Applicant, Status FROM dbo.PriceRequest WHERE 1 = 1");
        if (!string.IsNullOrWhiteSpace(applicant))
            sql.Append(" AND Applicant = @Applicant");
        sql.Append(" ORDER BY Id");

        using var conn = _factory.Create();
        var cmd = new CommandDefinition(sql.ToString(), new { Applicant = applicant }, cancellationToken: ct);
        return (await conn.QueryAsync<RequestBrief>(cmd)).ToList();   // 查不到 → 空列表 → JSON []
    }
}

public class RequestBrief
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
    public byte Status { get; set; }
}
