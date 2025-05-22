using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Web;

namespace Tizzani.QueryStringSerializer;

public static class QueryStringSerializer
{
    /// <summary>
    /// Serializes an object into a query string format.
    /// </summary>
    /// <param name="uri">The destination URI.</param>
    /// <param name="obj">The object to serialize into a query string.</param>
    /// <param name="options">The options to use for serialization.</param>
    /// <typeparam name="T">The type being serialized.</typeparam>
    /// <returns>The resulting uri and query string.</returns>
    public static string Serialize<T>(
        [StringSyntax("Uri")] string uri,
        T? obj,
        JsonSerializerOptions? options = null)
    {
        return obj is null ? uri : $"{uri}?{Serialize(obj, options)}";
    }

    /// <summary>
    /// Serializes an object into a query string format.
    /// </summary>
    /// <param name="obj">The object to serialize into a query string.</param>
    /// <param name="options">The options to use for serialization.</param>
    /// <typeparam name="T">The type being serialized.</typeparam>
    /// <returns>The resulting uri and query string.</returns>
    public static string Serialize<T>(
        T? obj,
        JsonSerializerOptions? options = null)
    {
        if (obj is null) 
            return "";

        if (options == null)
        {
            options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
        }
        
        var json = JsonSerializer.Serialize(obj, options);

        var jObject = JsonNode.Parse(json);
        return ParseToken(jObject);
    }

    /// <summary>
    /// Deserializes a query string into an object of the specified type.
    /// </summary>
    /// <param name="uri">The uri that contains the query string.</param>
    /// <param name="options">The options to use for deserialization.</param>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <returns></returns>
    public static T? Deserialize<T>(
        [StringSyntax("Uri")] string uri, 
        JsonSerializerOptions? options = null)
    {
        if (options == null)
        {
            options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
        }

        var qs = uri.Contains('?') ? uri.Split('?')[1] : uri;
        var qParams = QueryHelpers.ParseQuery(qs);
        var dict = GetObjectDictionary(typeof(T), qParams, options);
        var json = JsonSerializer.Serialize(dict, options);
        return JsonSerializer.Deserialize<T>(json, options);
    }

    private static Dictionary<string, object?> GetObjectDictionary(Type objectType, Dictionary<string, StringValues> qParams, JsonSerializerOptions jsonSerializerOptions)
    {
        var dict = new Dictionary<string, object?>();

        foreach (var key in qParams.Keys.Select(k => k.Split('.')[0]).Distinct())
        {
            if (objectType.GetProperty(key) is not { } pInfo)
                continue;

            var pType = pInfo.PropertyType;

            if (pType == typeof(string))
            {
                dict.Add(key, qParams[key].ToString());
            }
            else if (typeof(IEnumerable).IsAssignableFrom(pType))
            {
                var arrayType = pType.GetElementType() ?? pType.GetGenericArguments()[0];

                if (arrayType == typeof(string) || !arrayType.IsClass)
                {
                    dict.Add(key, qParams[key].Select(p => ParsePrimitiveType(arrayType, p, jsonSerializerOptions)));
                    continue;
                }

                var elementParams = qParams
                    .Where(kvp => kvp.Key.StartsWith($"{key}."))
                    .ToDictionary(kvp => kvp.Key[(key.Length + 1)..], kvp => kvp.Value);

                var elements = new List<object>();
                for (int i = 0; i < elementParams.Values.Max(v => v.Count); i++)
                {
                    var elementDict = new Dictionary<string, StringValues>();
                    
                    foreach (var p in elementParams)
                    {
                        elementDict.Add(p.Key, p.Value[i]);
                    }

                    elements.Add(GetObjectDictionary(arrayType, elementDict, jsonSerializerOptions));
                }

                dict.Add(key, elements);
            }
            else if (pType.IsClass)
            {
                var childParams = qParams
                    .Where(kvp => kvp.Key.StartsWith($"{key}."))
                    .ToDictionary(kvp => kvp.Key[(key.Length + 1)..], kvp => kvp.Value);

                dict.Add(key, GetObjectDictionary(pType, childParams, jsonSerializerOptions));
            }
            else
            {
                if (ParsePrimitiveType(pType, qParams[key].ToString(), jsonSerializerOptions) is { } value)
                    dict.Add(key, value);
            }
        }

        return dict;
    }

    private static object? ParsePrimitiveType(Type targetType, string? value, JsonSerializerOptions jsonSerializerOptions)
    {
        try
        {
            if (targetType == typeof(string))
            {
                return value;
            }
            
            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, value ?? "");
            }
            
            return JsonSerializer.Deserialize(value ?? "", targetType, jsonSerializerOptions);
        }
        catch { return null; }
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
