<h3 align="center"><img src="./assets/QssLogo.png" width="70"><br /><b>Query String Serializer</b></h3>
<p align="center"><b>Lightweight serializer for query strings and .NET objects.</b></p>
<p align="center">
    <!-- TODO: Badges -->
</p>

<hr />

## Installation
```
dotnet add package Tizzani.QueryStringSerializer --version 8.0.0
```

## Example Usage

### Serialization

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
