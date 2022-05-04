using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tizzani.QueryStringHelpers;

public class QueryStringSerializerOptions
{
    public bool EnumsAsStrings { get; set; } = true;

    internal JsonSerializerOptions GetJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions();

        if (EnumsAsStrings)
            options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }
}
