# Query String Serializer

`Tizzani.QueryStringSerializer` provides methods for serializing & deserializing objects as URL-encoded query strings.

## Example Usage

```csharp
var order = new Order
{
    Customer = new Customer { Name = "Jack Sparrpw" },
    Items = new List<OrderItem>
    {
        new OrderItem { Description = "Rum", Quantity = 5 },
        new OrderItem { Description = "Jar of Dirt", Quantity = 1 }
    }
};

var queryString = QueryStringSerializer.Serialize(order);
// "Customer.Name=Jack+Sparrow&Items.Description=Rum&Items.Quantity=5&Items.Description=Jar+of+Dirt&Items.Quantity=1";
```

### Deserialization

```c#
var queryString = "Customer.Name=Jack+Sparrow&Items.Description=Rum&Items.Quantity=5&Items.Description=Jar+of+Dirt&Items.Quantity=1";
var order = QueryStringSerializer.Deserialize<Order>(queryString);
```
