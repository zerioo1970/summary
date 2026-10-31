// code/ch08/Answer2-ClosePlayer/Program.cs
var builder = WebApplication.CreateBuilder(args);

// 第一层限制：整个请求正文的大小。Kestrel 默认 30,000,000 字节（约 28.6MB），装不下一段手机视频
// 设成 110MB：比视频上限 100MB 多留一点，给表单的其他部分；超过 100MB 的由接口自己给出友好提示
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 110 * 1024 * 1024;
});
// 部署到 IIS（进程内托管）时，上面那行不起作用，生效的是这一行；本机运行时这一行不起作用
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 110 * 1024 * 1024;
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

var products = new List<Product>
{
    new() { Id = 1, Code = "P001", Name = "彩超主机 X1", Price = 268000m },
    new() { Id = 2, Code = "P002", Name = "凸阵探头 C5", Price = 32000m },
    new() { Id = 3, Code = "P003", Name = "线阵探头 L12", Price = 28500m },
    new() { Id = 4, Code = "P004", Name = "耦合剂 250ml", Price = 12.5m },
    new() { Id = 5, Code = "P005", Name = "便携彩超 M3", Price = 156000m },
};

app.MapGet("/api/products", (string? keyword) =>
    string.IsNullOrWhiteSpace(keyword)
        ? products
        : products.Where(p => p.Name.Contains(keyword)).ToList());

app.MapGet("/api/products/{id:int}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    return product is null ? Results.NotFound() : Results.Ok(product);
});

app.MapPost("/api/products", (Product input) =>
{
    var errors = Validate(input, null);
    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    input.Id = products.Count == 0 ? 1 : products.Max(p => p.Id) + 1;
    input.ImageUrl = null; // 图片、视频只能走下面的上传接口，不接受前端直接填地址
    input.VideoUrl = null;
    products.Add(input);
    return Results.Created($"/api/products/{input.Id}", input);
});

// PUT：地址里的 id 决定改哪一条，正文是修改后的完整数据
app.MapPut("/api/products/{id:int}", (int id, Product input) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    // 可能在你打开编辑之后，别人已经把它删了
    if (product is null)
        return Results.NotFound();

    var errors = Validate(input, id);
    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    product.Code = input.Code;
    product.Name = input.Name;
    product.Price = input.Price;
    // 不改 ImageUrl：修改文字信息时，图片保持原样
    return Results.Ok(product);
});

app.MapDelete("/api/products/{id:int}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product is null)
        return Results.NotFound();
    products.Remove(product);
    DeleteUpload(product.ImageUrl); // 产品删了，它的图片、视频文件也删掉
    DeleteUpload(product.VideoUrl);
    // 204 No Content：删掉了，没有要返回的内容
    return Results.NoContent();
});

// 允许的扩展名和大小上限。扩展名统一转小写再比，P1.JPG 也能通过
string[] imageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
const long MaxImageBytes = 2 * 1024 * 1024; // 2MB

// 参数类型是 IFormFile：框架从 multipart/form-data 正文里按名字 image 取出文件
app.MapPost("/api/products/{id:int}/image", async (int id, IFormFile? image) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product is null)
        return Results.NotFound();

    var errors = new Dictionary<string, string[]>();
    var ext = Path.GetExtension(image?.FileName ?? "").ToLowerInvariant();
    if (image is null || image.Length == 0)
        errors["image"] = ["请选择图片"];
    else if (!imageExtensions.Contains(ext))
        errors["image"] = ["只能上传 jpg、png、webp 图片"];
    else if (image.Length > MaxImageBytes)
        errors["image"] = ["图片不能超过 2MB"];
    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    // 文件名自己生成，不用用户的：用户的文件名可能重复、带中文空格，甚至带 ..\ 跳出目录
    var fileName = $"{Guid.NewGuid():N}{ext}";
    var folder = Path.Combine(app.Environment.WebRootPath, "uploads");
    Directory.CreateDirectory(folder); // 目录已存在时什么也不做
    await using (var stream = File.Create(Path.Combine(folder, fileName)))
    {
        await image!.CopyToAsync(stream);
    }

    DeleteUpload(product.ImageUrl); // 换图时删掉旧文件，不然 uploads 里越积越多
    product.ImageUrl = $"/uploads/{fileName}";
    return Results.Ok(product);
})
// 不加这一行，接收表单文件的接口会要求防伪令牌（antiforgery token）。7.5 节演示
.DisableAntiforgery();

// 视频：和图片同样的三条规矩（自己起文件名、白名单扩展名、限制大小），只是数值不同
string[] videoExtensions = [".mp4", ".webm", ".mov"];
const long MaxVideoBytes = 100 * 1024 * 1024; // 100MB

app.MapPost("/api/products/{id:int}/video", async (int id, IFormFile? video) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product is null)
        return Results.NotFound();

    var errors = new Dictionary<string, string[]>();
    var ext = Path.GetExtension(video?.FileName ?? "").ToLowerInvariant();
    if (video is null || video.Length == 0)
        errors["video"] = ["请选择视频"];
    else if (!videoExtensions.Contains(ext))
        errors["video"] = ["只能上传 mp4、webm、mov 视频"];
    else if (video.Length > MaxVideoBytes)
        errors["video"] = ["视频不能超过 100MB"];
    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    var fileName = $"{Guid.NewGuid():N}{ext}";
    var folder = Path.Combine(app.Environment.WebRootPath, "uploads");
    Directory.CreateDirectory(folder);
    await using (var stream = File.Create(Path.Combine(folder, fileName)))
    {
        await video!.CopyToAsync(stream);
    }

    DeleteUpload(product.VideoUrl);
    product.VideoUrl = $"/uploads/{fileName}";
    return Results.Ok(product);
})
.DisableAntiforgery();

app.Run();

// 新增和修改共用一套规则。selfId 是正在修改的那条，查重时要排除它自己
Dictionary<string, string[]> Validate(Product input, int? selfId)
{
    var errors = new Dictionary<string, string[]>();
    if (string.IsNullOrWhiteSpace(input.Code))
        errors["code"] = ["编码必填"];
    else if (products.Any(p => p.Code == input.Code && p.Id != selfId))
        errors["code"] = ["编码已存在"];
    if (string.IsNullOrWhiteSpace(input.Name))
        errors["name"] = ["名称必填"];
    if (input.Price <= 0)
        errors["price"] = ["价格必须大于 0"];
    return errors;
}

// 按 /uploads/xxx.jpg 这种地址删除 wwwroot 下的文件
void DeleteUpload(string? url)
{
    if (string.IsNullOrEmpty(url))
        return;
    var path = Path.Combine(app.Environment.WebRootPath, url.TrimStart('/'));
    File.Delete(path); // 文件不存在时不会报错
}

class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    // 图片访问地址；没上传过是 null
    public string? ImageUrl { get; set; }
    // 视频访问地址；没上传过是 null
    public string? VideoUrl { get; set; }
}
