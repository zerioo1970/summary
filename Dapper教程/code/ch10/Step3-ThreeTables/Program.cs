// code/ch10/Step3-ThreeTables/Program.cs
// 三表映射：明细 + 产品 + 申请单，一行拆成三个对象，做一张"申请价折扣率"报表
using Dapper;
using Microsoft.Data.SqlClient;

const string connStr =
    "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

using var conn = new SqlConnection(connStr);

// 三段列：明细 | 产品（从 p.Id 开始）| 申请单（从 r.Id 开始）
const string sql = @"
    SELECT i.Id, i.Qty, i.RequestPrice,
           p.Id, p.Code, p.Name, p.ListPrice,
           r.Id, r.RequestNo, r.Applicant
    FROM dbo.PriceRequestItem i
    JOIN dbo.Product p      ON p.Id = i.ProductId
    JOIN dbo.PriceRequest r ON r.Id = i.RequestId
    ORDER BY i.Id";

// 四个类型参数：<第 1 段, 第 2 段, 第 3 段, 返回类型>
var items = conn.Query<RequestItem, Product, PriceRequest, RequestItem>(
    sql,
    (item, product, request) =>
    {
        item.Product = product;
        item.Request = request;
        return item;
    },
    splitOn: "Id,Id");     // 三个对象 = 两个切分点，用逗号分隔，从左到右对应第 2、第 3 个对象

foreach (var i in items)
{
    // 折扣率 = 申请价 ÷ 目录价。自己算百分比，避免不同区域设置下 P 格式的写法不同
    decimal rate = i.RequestPrice / i.Product!.ListPrice * 100;
    Console.WriteLine($"{i.Request!.RequestNo}  {i.Request.Applicant}  {i.Product.Code,-7}" +
                      $"目录价 {i.Product.ListPrice,10:N2}  申请价 {i.RequestPrice,10:N2}  " +
                      $"× {i.Qty}  折扣率 {rate:F2}%");
}

public class RequestItem
{
    public int Id { get; set; }
    public int Qty { get; set; }
    public decimal RequestPrice { get; set; }
    public Product? Product { get; set; }
    public PriceRequest? Request { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal ListPrice { get; set; }
}

public class PriceRequest
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string Applicant { get; set; } = "";
}
