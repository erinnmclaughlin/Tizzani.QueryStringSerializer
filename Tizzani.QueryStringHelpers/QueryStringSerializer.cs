using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.Collections;
using System.Text.Json;

namespace Tizzani.QueryStringHelpers;

public static class QueryStringSerializer
{
    public static string Serialize<T>(T obj, QueryStringSerializerOptions? options = null)
    {
        options ??= new QueryStringSerializerOptions();

        var json = JsonSerializer.Serialize(obj, options.GetJsonSerializerOptions());
        var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(json, options.GetJsonSerializerOptions());

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

        return uri.TrimStart('?');
    }

    public static string Serialize<T>(T obj, string baseUri, QueryStringSerializerOptions? options = null)
    {
        return $"{baseUri}?{Serialize(obj, options)}";
    }

    public static T? Deserialize<T>(string uri)
    {
        var json = GetJson<T>(uri);
        return JsonSerializer.Deserialize<T>(json);
    }

    private static string GetJson<T>(string uri)
    {
        var queryParams = QueryHelpers.ParseQuery(uri);
        var dict = queryParams.ToObjectDictionary(typeof(T));
        return JsonSerializer.Serialize(dict);
    }

    private static Dictionary<string, object?> ToObjectDictionary(this Dictionary<string, StringValues> stringDict, Type type)
    {
        var dict = new Dictionary<string, object?>();

        foreach (var p in type.GetProperties())
        {
            // Nested classes
            if (p.PropertyType.IsClass && !typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
            {
                var childStringDict = stringDict
                    .Where(x => x.Key.StartsWith(p.Name + '.'))
                    .ToDictionary(kvp => kvp.Key.Replace(p.Name + '.', ""), kvp => kvp.Value);

                var childDict = ToObjectDictionary(childStringDict, p.PropertyType);
                dict.Add(p.Name, childDict);
                continue;
            }

            // if value is not in query string, skip (class default value will be used)
            if (!stringDict.ContainsKey(p.Name))
            {
                continue;
            }

            var stringValue = stringDict[p.Name];

            // Lists
            if (p.PropertyType.IsCollection())
            {
                var enumerableType = GetCollectionType(p.PropertyType);

                if (enumerableType == null)
                    continue; // TODO: should we do something here?

                if (!enumerableType.IsClass || enumerableType == typeof(string))
                {
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

            // Simple cases
            if (!p.PropertyType.IsClass || p.PropertyType == typeof(string))
            {
                var str = stringValue.ToString();

                var value = p.PropertyType == typeof(string) 
                    ? str : JsonSerializer.Deserialize(stringValue.ToString(), p.PropertyType);

                if (value != null)
                    dict.Add(p.Name, value);

                continue;
            }

            // I don't know how you got here
            throw new InvalidOperationException($"Cannot add type {p.PropertyType} to object dictionary.");
        }

        return dict;
    }

    private static Type? GetCollectionType(Type type)
    {
        foreach (var i in type.GetInterfaces())
        {
            if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))
            {
                return i.GenericTypeArguments[0];
            }
        }
        
        return null;
    }

    private static bool IsCollection(this Type type)
    {
        return type != typeof(string) && type.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>));
    }
}
