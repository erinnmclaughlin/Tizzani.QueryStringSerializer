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
// queryString = "Customer.Name=Jack%20Sparrpw&Items=%7B%22Description%22%3A%22Rum%22,%22Quantity%22%3A5%7D&Items=%7B%22Description%22%3A%22Jar%20of%20Dirt%22,%22Quantity%22%3A1%7D";

// get the query string appended to an existing URL:
var personUrl = QueryStringSerializer.Serialize(person, "https://mysite.com/directory/search");
```

### Deserialization

```c#
// also works if personQs has a leading "?"
var personQs = "Name.Given=Some&Name.Family=Person&Age=25&FavoriteWebsites=https%3A%2F%2Fgithub.com%2Ferinnmclaughlin";

var person = QueryStringSerializer.Deserialize(personQs);
```

## Other Features

* `Enum`s are supported
* `Enum`s are serialized as `string`s by default, but can be configured to serialize as `int`s by using `QueryStringSerializerOptions`
* Many basic collection types (such as `IList`, `System.Arrray`, `ICollection`) are supported
* `class`, `record`, `struct` and `readonly struct` are all supported

## Known Limitations

* Collections of *objects* are not yet supported; only of primitives
* Dictionaries are not yet supported



