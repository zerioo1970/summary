// code/ch02/Answer2-MinPrice/Program.cs
var builder = WebApplication.CreateBuilder(args);
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

// 两个查询条件都可以不传：decimal? 没传时是 null
app.MapGet("/api/products", (string? keyword, decimal? minPrice) =>
{
    IEnumerable<Product> result = products;
    if (!string.IsNullOrWhiteSpace(keyword))
        result = result.Where(p => p.Name.Contains(keyword));
    if (minPrice is not null)
        result = result.Where(p => p.Price >= minPrice);
    return result.ToList();
});

// {id:int} 是路由参数，只匹配整数；/api/products/abc 不会进到这里
app.MapGet("/api/products/{id:int}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    // 查不到返回 404，前端靠状态码区分"没有"和"出错"
    return product is null ? Results.NotFound() : Results.Ok(product);
});

app.Run();

class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}
