using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.Collections;
using System.Text.Json;

namespace Tizzani.QueryStringHelpers;

public static class QueryStringSerializer
{
    public static string Serialize<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(json);

        string uri = "";

        if (dict == null)
            return uri;

        foreach (var kvp in dict)
        {
            if (kvp.Value == null)
                continue;

            var type = kvp.Value.GetType();

            if (type != typeof(JsonElement))
                continue;

            var jsonElement = (JsonElement)kvp.Value;

            // Collections
            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var val in jsonElement.EnumerateArray())
                {
                    var valString = val.ToString();

                    if (!string.IsNullOrWhiteSpace(valString))
                        uri = QueryHelpers.AddQueryString(uri, kvp.Key, valString);
                }

                continue;
            }

            if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                var childUri = Serialize(kvp.Value);
                var childQuery = QueryHelpers.ParseQuery(childUri);

                foreach (var cq in childQuery)
                    uri = QueryHelpers.AddQueryString(uri, kvp.Key + "." + cq.Key, cq.Value);

                continue;
            }

            var valueString = kvp.Value.ToString();

            if (!string.IsNullOrWhiteSpace(valueString))
                uri = QueryHelpers.AddQueryString(uri, kvp.Key, valueString);            
        }

        return uri;
    }
    public static string Serialize<T>(T obj, string baseUri) where T : class
    {
        return baseUri + Serialize(obj);
    }
    public static T? Deserialize<T>(string uri) where T : class
    {
        var json = GetJson<T>(uri);
        return JsonSerializer.Deserialize<T>(json);
    }

    private static string GetJson<T>(string uri) where T : class
    {
        var queryParams = QueryHelpers.ParseQuery(uri);
        var dict = queryParams.ToObjectDictionary(typeof(T));
        return JsonSerializer.Serialize(dict);
    }
    private static Dictionary<string, object?> ToObjectDictionary(this Dictionary<string, StringValues> stringDict, Type type)
    {
        var dict = new Dictionary<string, object?>();
        var obj = Activator.CreateInstance(type);

        foreach (var p in type.GetProperties())
        {
            var name = p.Name;

            // Nested classes
            if (p.PropertyType.IsClass && !typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
            {
                var childStringDict = stringDict
                    .Where(x => x.Key.StartsWith(name + '.'))
                    .ToDictionary(kvp => kvp.Key.Replace(name + '.', ""), kvp => kvp.Value);

                var childDict = ToObjectDictionary(childStringDict, p.PropertyType);
                dict.Add(p.Name, childDict);
                continue;
            }

            // Use object's default value if not in query string
            if (!stringDict.ContainsKey(p.Name))
            {
                var defaultValue = p.GetValue(obj);

                if (defaultValue != null)
                    dict.Add(p.Name, defaultValue);

                continue;
            }

            var stringValue = stringDict[p.Name];

            // Simple cases
            if (!p.PropertyType.IsClass || p.PropertyType == typeof(string))
            {
                var value = p.PropertyType == typeof(string) 
                    ? stringValue.ToString()
                    : JsonSerializer.Deserialize(stringValue, p.PropertyType);

                if (value != null)
                    dict.Add(p.Name, value);

                continue;
            }

            // Collections
            if (typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
            {
                var enumerableType = p.PropertyType.GetElementType();

                if (enumerableType is null)
                    throw new NotImplementedException(); // I don't know what to do here yet

                if (!enumerableType.IsClass || enumerableType == typeof(string))
                {
                    // TODO: Do this better

                    var collection = stringValue
                        .Select(x => JsonSerializer.Deserialize(x, enumerableType))
                        .Where(x => x != null)
                        .ToList();

                    dict.Add(p.Name, collection);
                }
                else
                {
                    // TODO: Deal with complex collections
                    throw new NotImplementedException();
                }

                continue;
            }

            // I don't know how you got here
            throw new InvalidOperationException($"Cannot add type {p.PropertyType} to object dictionary.");
        }

        return dict;
    }
}
