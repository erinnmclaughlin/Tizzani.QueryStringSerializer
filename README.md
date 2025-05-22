# Query String Serializer

**Lightweight serializer for query strings and .NET objects.**

![NuGet Version](https://img.shields.io/nuget/v/tizzani.querystringserializer)
![NuGet Downloads](https://img.shields.io/nuget/dt/tizzani.querystringserializer)
![Last Commit](https://img.shields.io/github/last-commit/erinnmclaughlin/Tizzani.QueryStringSerializer/main)
![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/erinnmclaughlin/Tizzani.QueryStringSerializer/dotnet.yml)

---

## Installation
Download from [NuGet](https://www.nuget.org/packages/Tizzani.QueryStringSerializer).
```sh
dotnet add package Tizzani.QueryStringSerializer
```

## Example Usage

### Serialization

```csharp
var order = new Order
{
    Customer = new Customer { Name = "Jack Sparrow" },
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

## Configuration
To configure how query strings are serialized, use `QueryStringSerializerOptions`.

```c#
enum Status { Placed, Canceled, Completed }
var order = new Order { Status = Status.Canceled };
var qs1 = QueryStringSerializer.Serialize(order); // "Status=Canceled"
var qs2 = QueryStringSerializer.Serialize(order, JsonSerializerOptions.Default); // "Status=1"
```
