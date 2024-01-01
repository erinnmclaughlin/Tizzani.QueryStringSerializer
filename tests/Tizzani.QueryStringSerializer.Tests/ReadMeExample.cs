using System.Collections.Generic;
using Xunit;

namespace Tizzani.QueryStringSerializer.Tests;

public class ReadMeExample
{
    public Order ExampleOrder => new Order
    {
        Customer = new() { Name = "Jack Sparrow" },
        Items =
        [
            new() { Description = "Rum", Quantity = 5 },
            new() { Description = "Jar of Dirt", Quantity = 1 }
        ]
    };

    [Fact]
    public void SerializesCorrectly()
    {
        var expected = "Customer.Name=Jack+Sparrow&Items.Description=Rum&Items.Quantity=5&Items.Description=Jar+of+Dirt&Items.Quantity=1";
        var actual = QueryStringSerializer.Serialize(ExampleOrder);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DeserializesCorrectly()
    {
        var queryString = "Customer.Name=Jack+Sparrow&Items.Description=Rum&Items.Quantity=5&Items.Description=Jar+of+Dirt&Items.Quantity=1";
        var actual = QueryStringSerializer.Deserialize<Order>(queryString);
        Assert.Equivalent(ExampleOrder, actual);
    }

    public class Order
    {
        public Customer? Customer { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }

    public class OrderItem
    {
        public string Description { get; set; } = "";
        public int Quantity { get; set; } = 1;
    }

    public class Customer
    {
        public string Name { get; set; } = "";
    }
}
