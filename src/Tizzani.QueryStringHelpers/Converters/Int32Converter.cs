using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tizzani.QueryStringHelpers.Converters;

public class Int32Converter : JsonConverter<int?>
{
    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetInt32();

        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();

            if (string.IsNullOrWhiteSpace(stringValue))
                return null;

            if (int.TryParse(stringValue, out int value))
                return value;
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteNumberValue(value.Value);
    }
}