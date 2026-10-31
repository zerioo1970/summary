// code/ch01/Answer2-AutoLoad/Program.cs
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 访问 / 时自动改成 /index.html。必须写在 UseStaticFiles 前面，否则不生效
app.UseDefaultFiles();
// 让浏览器能直接下载 wwwroot 里的 html、js、css
app.UseStaticFiles();

// 数据先放内存，程序一重启就恢复成这 5 条
var products = new List<Product>
{
    new() { Id = 1, Code = "P001", Name = "彩超主机 X1", Price = 268000m },
    new() { Id = 2, Code = "P002", Name = "凸阵探头 C5", Price = 32000m },
    new() { Id = 3, Code = "P003", Name = "线阵探头 L12", Price = 28500m },
    new() { Id = 4, Code = "P004", Name = "耦合剂 250ml", Price = 12.5m },
    new() { Id = 5, Code = "P005", Name = "便携彩超 M3", Price = 156000m },
};

// 直接返回 List，框架会把它序列化成 JSON 数组
app.MapGet("/api/products", () => products);

app.Run();

class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}
