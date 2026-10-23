// code/ch18/Step4-SmokeTests/ItemRepositoryTests.cs
// 冒烟测试：仓储的每个方法真连库跑一次，断言关键属性"不是默认值"——专抓 Dapper 不报错的静默映射问题
// 运行前先执行 code/sql/ch18-add-discount.sql
using Xunit;

public class ItemRepositoryTests
{
    // 测试连的是开发库 PriceDemo，数据是 00-reset-database.sql + ch18-add-discount.sql 灌进去的那一批
    private const string ConnStr =
        "Server=.;Database=PriceDemo;Integrated Security=True;TrustServerCertificate=True;";

    private readonly ItemRepository _repo = new(ConnStr);

    [Fact]
    public void GetByRequestId_每一列都映射上了()
    {
        var items = _repo.GetByRequestId(1);

        Assert.Equal(2, items.Count);                        // 第 1 单有 2 行明细
        Assert.All(items, i =>
        {
            Assert.NotEqual(0, i.Id);                        // 每个属性都不该是默认值 0
            Assert.NotEqual(0, i.RequestId);
            Assert.NotEqual(0, i.ProductId);
            Assert.NotEqual(0, i.Qty);
            Assert.NotEqual(0m, i.RequestPrice);
        });
        // 可空列不能断言"每行都有值"，但至少有一行有值，才说明这一列真的查出来了
        Assert.Contains(items, i => i.Discount is not null);
    }

    [Fact]
    public void GetById_查得到时折扣有值()
    {
        var item = _repo.GetById(3);

        Assert.NotNull(item);
        Assert.Equal(2, item.RequestId);
        Assert.Equal(0.90m, item.Discount);                  // 新加的列：用已知数据断言精确值
    }

    [Fact]
    public void GetById_查不到返回null()
    {
        Assert.Null(_repo.GetById(9999));
    }

    [Fact]
    public void GetTotal_与逐行相加一致()
    {
        decimal sumInCode = _repo.GetByRequestId(1).Sum(i => i.NetAmount);

        Assert.Equal(405500m, _repo.GetTotal(1));            // 2×165000×0.95 + 4×23000
        Assert.Equal(sumInCode, _repo.GetTotal(1));          // SQL 公式和 C# 公式必须一致
    }
}
