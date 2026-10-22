// code/ch04/Backend/Program.cs
// 第 4 章：代码与第 3 章相同；要让手机访问，改的是 Properties/launchSettings.json 里的监听地址。

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 访问 "/" 这种目录地址时，把请求路径改写成 "/index.html"。
// 它只改写路径、不发文件，所以必须排在 UseStaticFiles 前面。
app.UseDefaultFiles();

// 请求路径能对上 wwwroot 里的某个文件，就直接把文件发出去，后面的接口不再执行；
// 对不上（比如 /api/todos）就放行给下面的 MapGet / MapPost。
app.UseStaticFiles(new StaticFileOptions
{
    // main.js 每次构建都叫同一个名字。不加这个头，浏览器可能继续用缓存里的旧版本。
    // no-cache 的意思不是"不缓存"，而是"用之前先问服务器一句有没有变"：
    // 没变时服务器只回 304（没有内容），代价很小。
    OnPrepareResponse = ctx =>
        ctx.Context.Response.Headers.CacheControl = "no-cache",
});

var todos = new List<Todo>
{
    new(1, "买牛奶", false),
    new(2, "写周报", true),
};
var nextId = 3;
var locker = new object();

app.MapGet("/api/todos", () =>
{
    lock (locker)
    {
        return Results.Ok(todos.ToList());
    }
});

app.MapPost("/api/todos", (CreateTodoRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Title))
    {
        return Results.BadRequest(new ErrorBody("标题不能为空"));
    }

    lock (locker)
    {
        var todo = new Todo(nextId++, req.Title.Trim(), false);
        todos.Add(todo);
        return Results.Created($"/api/todos/{todo.Id}", todo);
    }
});

app.Run();

record Todo(int Id, string Title, bool Done);
record CreateTodoRequest(string? Title);
record ErrorBody(string Message);
