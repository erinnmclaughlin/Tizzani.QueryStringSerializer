using System.Collections.Generic;
using Xunit;

namespace Tizzani.QueryStringSerializer.Tests;

public class ReadMeExample
{
    [Fact]
    public void SerializesCorrectly()
    {
        var order = new Order
        {
            Customer = new() { Name = "Jack Sparrow" },
            Items =
            [
                new() { Description = "Rum", Quantity = 5 },
                new() { Description = "Jar of Dirt", Quantity = 1 }
            ]
        };

        var expected = "Customer.Name=Jack+Sparrow&Items.Description=Rum&Items.Quantity=5&Items.Description=Jar+of+Dirt&Items.Quantity=1";
        var actual = QueryStringSerializer.Serialize(order);
        Assert.Equal(expected, actual);
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
