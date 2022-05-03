## Query String Helpers

`Tizzani.QueryStringHelpers` provides methods for serializing & deserializing objects as URL-encoded query strings.

### Example Usage

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

#### Serialization

```c#
var person = new Person() {
    Name = new Name("Some", null, "Person"),
    Age = 25,
    FavoriteWebsites = new List<string>() { "https://github.com/Tizzani" }
}

// get a plain query string, suitable for use in a POST request body:
var personQs = QueryStringHelpers.Serialize(person);

// get the query string appended to an existing URL:
var personUrl = QueryStringHelpers.Serialize(person, "https://mysite.com/directory/search");
```

#### Deserialization

```c#
// also works if personQs has a leading "?"
var personQs = "Name.Given=Some&Name.Family=Person&Age=25&FavoriteWebsites=https%3A%2F%2Fgithub.com%2FTizzani";

var person = QueryStringHelpers.Deserialize(personQs);
```

### Other Features

* Enums are supported
* Enums are serialized as integers by default, but can be configured to serialize as strings by using `QueryStringSerializerOptions` (passed into `QueryStringHelpers.Serialize`)

### Known Limitations

* Lists of objects are not yet supported (arrays of primitives and enums are!)
* Dictionaries are not yet supported



