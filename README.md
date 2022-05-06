# Query String Serializer

`Tizzani.QueryStringSerializer` provides methods for serializing & deserializing objects as URL-encoded query strings.

## Example Usage

Given the following classes:

```c#
class Person
{
    public Name Name { get; set; }
    public int Age { get; set; }
    public List<string> FavoriteWebsites { get; set; } = new();
}

record Name(string Given, string? Middle, string Family);
```

### Serialization

```c#
var person = new Person() {
    Name = new Name("Some", null, "Person"),
    Age = 25,
    FavoriteWebsites = new List<string>() { "https://github.com/Tizzani" }
}

// get a plain query string, suitable for use in a POST request body:
var personQs = QueryStringSerializer.Serialize(person);

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



