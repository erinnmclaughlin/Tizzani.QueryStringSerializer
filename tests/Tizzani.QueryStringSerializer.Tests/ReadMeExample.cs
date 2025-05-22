namespace Tizzani.QueryStringSerializer.Tests;

public class ReadMeExample
{
    private static Order ExampleOrder => new()
    {
        Customer = new Customer { Name = "Jack Sparrow" },
        Items =
        [
            new OrderItem { Description = "Rum", Quantity = 5 },
            new OrderItem { Description = "Jar of Dirt", Quantity = 1 }
        ]
    };

    [Fact]
    public void SerializesCorrectly()
    {
        const string qs = "Customer.Name=Jack+Sparrow&Items.Description=Rum&Items.Quantity=5&Items.Description=Jar+of+Dirt&Items.Quantity=1";
        var actual = QueryStringSerializer.Serialize(ExampleOrder);
        Assert.Equal(qs, actual);
    }

    [Fact]
    public void DeserializesCorrectly()
    {
        const string qs = "Customer.Name=Jack+Sparrow&Items.Description=Rum&Items.Quantity=5&Items.Description=Jar+of+Dirt&Items.Quantity=1";
        var actual = QueryStringSerializer.Deserialize<Order>(qs);
        Assert.Equivalent(ExampleOrder, actual);
    }

    private sealed class Order
    {
        public Customer? Customer { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }

    private sealed class OrderItem
    {
        public string Description { get; set; } = "";
        public int Quantity { get; set; } = 1;
    }

    private sealed class Customer
    {
        public string Name { get; set; } = "";
    }
}
