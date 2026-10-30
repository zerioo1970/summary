// code/ch01/Answer1-Stock/Program.cs
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

var products = new List<Product>
{
    new() { Id = 1, Code = "P001", Name = "彩超主机 X1", Price = 268000m, Stock = 3 },
    new() { Id = 2, Code = "P002", Name = "凸阵探头 C5", Price = 32000m, Stock = 12 },
    new() { Id = 3, Code = "P003", Name = "线阵探头 L12", Price = 28500m, Stock = 8 },
    new() { Id = 4, Code = "P004", Name = "耦合剂 250ml", Price = 12.5m, Stock = 400 },
    new() { Id = 5, Code = "P005", Name = "便携彩超 M3", Price = 156000m, Stock = 0 },
};

app.MapGet("/api/products", () => products);

app.Run();

class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    // 新加的属性，JSON 里会叫 stock
    public int Stock { get; set; }
}
