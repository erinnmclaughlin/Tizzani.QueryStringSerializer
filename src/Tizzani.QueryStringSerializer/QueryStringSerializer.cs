using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace Tizzani.QueryStringSerializer;

public static class QueryStringSerializer
{
    public static string Serialize<T>(T obj, string baseUri, QueryStringSerializerOptions? options = null)
    {
        return $"{baseUri}?{Serialize(obj, options)}";
    }

    public static string Serialize<T>(T obj, QueryStringSerializerOptions? options = null)
    {
        options ??= new QueryStringSerializerOptions();
        var jsonSerializerOptions = options.GetJsonSerializerOptions();

        var json = JsonSerializer.Serialize(obj, jsonSerializerOptions);

        var jObject = JsonNode.Parse(json);
        return ParseToken(jObject);
    }

    public static T? Deserialize<T>(string uri, QueryStringSerializerOptions? options = null)
    {
        options ??= new QueryStringSerializerOptions();
        var jsonSerializerOptions = options.GetJsonSerializerOptions();

        var dict = GetObjectDictionary(typeof(T), HttpUtility.ParseQueryString(uri));
        var json = JsonSerializer.Serialize(dict, jsonSerializerOptions);
        return JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
    }

    private static Dictionary<string, object?> GetObjectDictionary(Type objectType, NameValueCollection query)
    {
        var dict = new Dictionary<string, object?>();

        foreach (var key in query.AllKeys)
        {
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            var propertyName = key.Split('.')[0];

            if (dict.ContainsKey(propertyName) || objectType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) is not { } property)
            {
                continue;
            }

            var propertyType = property.PropertyType;

            // Handle complex types:
            if (key.Contains('.'))
            {
                if (typeof(IEnumerable).IsAssignableFrom(propertyType) && query.GetValues(key) is { Length: > 0 } complexArrayValues)
                {
                    throw new NotSupportedException("Collections of complex types are not supported.");
                }
                else
                {
                    var nestedQuery = new NameValueCollection();
                    var nestedTypes = query.AllKeys
                        .Where(x => x!.StartsWith(propertyName + ".") == true)
                        .Select(x => new KeyValuePair<string, string?>(x!, query.Get(x)));

                    foreach (var (nestedKey, nestedValue) in nestedTypes)
                    {
                        nestedQuery.Add(nestedKey[(propertyName.Length + 1)..], nestedValue);
                    }
                    dict.Add(propertyName, GetObjectDictionary(propertyType, nestedQuery));
                }

                continue;
            }

            // Handle simple types:
            if (IsSimpleType(propertyType))
            {
                dict.Add(key, ConvertSimpleType(propertyType, query[key]));
                continue;
            }

            // Handle collection types:
            if (typeof(IEnumerable).IsAssignableFrom(propertyType) && query.GetValues(key) is { Length: > 0 } arrayValues)
            {
                var arrayType = propertyType.GetElementType() ?? propertyType.GenericTypeArguments[0];

                if (IsSimpleType(arrayType))
                {
                    var parsedValues = arrayValues.Select(x => ConvertSimpleType(arrayType, x)).ToArray();
                    dict.Add(key, parsedValues);
                }
                else
                {
                }
            }
        }

        return dict;
    }

    private static object? CreateDefaultInstanceOfType(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    private static object? ConvertSimpleType(Type targetType, object? valueToConvert)
    {
        var stringValue = valueToConvert?.ToString();

        if (targetType == typeof(string))
        {
            return stringValue;
        }

        if (string.IsNullOrEmpty(stringValue))
        {
            return CreateDefaultInstanceOfType(targetType);
        }

        var typeConverter = TypeDescriptor.GetConverter(targetType);

        if (typeConverter.IsValid(stringValue))
        {
            return typeConverter.ConvertFromString(null, CultureInfo.InvariantCulture, stringValue);
        }

        return CreateDefaultInstanceOfType(targetType);
    }

    private static bool IsSimpleType(Type type)
    {
        if (Nullable.GetUnderlyingType(type) is { } underlyingType)
        {
            type = underlyingType;
        }

        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(TimeSpan)
            || type == typeof(DateOnly)
            || type == typeof(TimeOnly)
            || type == typeof(Guid);
    }

    private static string ParseToken(JsonNode? token, string prefix = "")
    {
        var parts = new List<string>();

        if (token is JsonObject obj)
        {
            foreach (var prop in obj)
            {
                string subPrefix = string.IsNullOrEmpty(prefix) ? prop.Key : $"{prefix}.{prop.Key}";
                string part = ParseToken(prop.Value, subPrefix);

                if (!string.IsNullOrWhiteSpace(part))
                    parts.Add(part);
            }
        }
        else if (token is JsonArray array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                string part = ParseToken(array[i], prefix);

                if (!string.IsNullOrWhiteSpace(part))
                    parts.Add(part);
            }
        }
        else if (token is JsonValue)
        {
            var value = token.ToString();

            if (!string.IsNullOrWhiteSpace(value))
                parts.Add($"{prefix}={HttpUtility.UrlEncode(value)}");
        }

        return string.Join('&', parts);
    }
}
