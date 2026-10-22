// code/ch01/Backend/Program.cs
// 第 1 章：只有接口，没有页面。用浏览器地址栏和 curl 就能验证。

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 用内存列表代替数据库：本教程关注"页面和接口怎么通讯"，先不引入数据库。
// 代价是重启后数据回到初始状态。
var todos = new List<Todo>
{
    new(1, "买牛奶", false),
    new(2, "写周报", true),
};
var nextId = 3;

// List<T> 不是线程安全的：两部手机同时提交，可能出现 id 重复。
// 所有读写都包在同一把锁里。
var locker = new object();

// 查询全部。返回 C# 对象，框架负责转成 JSON 并加上 Content-Type: application/json。
app.MapGet("/api/todos", () =>
{
    lock (locker)
    {
        // ToList() 复制一份再返回：序列化发生在锁外，直接返回原列表会和写入撞车
        return Results.Ok(todos.ToList());
    }
});

// 新增。请求体里的 JSON 会被自动反序列化成 CreateTodoRequest。
app.MapPost("/api/todos", (CreateTodoRequest req) =>
{
    // 后端必须自己校验：页面上的校验谁都能绕过，直接用 curl 就能调这个接口
    if (string.IsNullOrWhiteSpace(req.Title))
    {
        return Results.BadRequest(new ErrorBody("标题不能为空"));
    }

    lock (locker)
    {
        var todo = new Todo(nextId++, req.Title.Trim(), false);
        todos.Add(todo);
        // 201 表示"新建成功"，同时把新对象（含后端分配的 id）返回给调用方
        return Results.Created($"/api/todos/{todo.Id}", todo);
    }
});

app.Run();

// record 自带构造函数和只读属性，适合做"只在接口上传输的数据"。
// 序列化时属性名自动转成小驼峰：Id → id、Title → title、Done → done
record Todo(int Id, string Title, bool Done);

// Title 声明成 string?：请求体里可能根本没有 title 字段，要允许它是 null 再自己校验
record CreateTodoRequest(string? Title);

// 统一的错误返回格式，页面只需要读 message 一个字段
record ErrorBody(string Message);
