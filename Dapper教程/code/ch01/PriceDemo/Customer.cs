// code/ch01/PriceDemo/Customer.cs
namespace PriceDemo;

// 对应 dbo.Customer 一行。属性名故意与列名一致——第 2 章 Dapper 会靠这一点自动映射。
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";   // 给默认值，避免 Nullable 警告 CS8618
    public string City { get; set; } = "";
    public string Level { get; set; } = "";  // 列是 CHAR(1)，读出来是 string
}
