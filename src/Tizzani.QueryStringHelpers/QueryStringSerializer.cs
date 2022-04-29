using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.Collections;
using System.Text.Json;
using Tizzani.QueryStringHelpers.Extensions;

namespace Tizzani.QueryStringHelpers;

public static class QueryStringSerializer
{
    public static string Serialize<T>(T obj) where T : class
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

            if (type.IsCollection())
            {
                foreach (var val in (IEnumerable)kvp.Value)
                {
                    var valString = val?.ToString();

                    if (!string.IsNullOrWhiteSpace(valString))
                        uri = QueryHelpers.AddQueryString(uri, kvp.Key, valString);
                }
            }
            else
            {
                var valString = kvp.Value.ToString();

                if (!string.IsNullOrWhiteSpace(valString))
                    uri = QueryHelpers.AddQueryString(uri, kvp.Key, valString);
            }
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

    public static string GetJson<T>(string uri) where T : class
    {
        var queryParams = QueryHelpers.ParseQuery(uri);
        var dict = queryParams.ToObjectDictionary(typeof(T));
        return JsonSerializer.Serialize(dict);
    }

    private static Dictionary<string, object?> ToObjectDictionary(this Dictionary<string, StringValues> stringDict, Type type, string namePrefix = "")
    {
        var dict = new Dictionary<string, object?>();
        var obj = Activator.CreateInstance(type);

        foreach (var p in type.GetProperties())
        {
            var name = string.IsNullOrWhiteSpace(namePrefix) ? p.Name : namePrefix + "." + p.Name;

            // Use object's default value if not in query string
            if (!stringDict.ContainsKey(name))
            {
                var defaultValue = p.GetValue(obj);

                if (defaultValue != null)
                    dict.Add(name, defaultValue);

                continue;
            }

            var stringValue = stringDict[name];

            // Simple cases
            if (!p.PropertyType.IsClass || p.PropertyType == typeof(string))
            {
                var value = p.PropertyType == typeof(string) 
                    ? stringValue.ToString()
                    : JsonSerializer.Deserialize(stringValue, p.PropertyType);

                if (value != null)
                    dict.Add(name, value);

                continue;
            }

            // Collections
            if (typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
            {
                var enumerableType = p.PropertyType.GetGenericArguments()[0];

                if (!enumerableType.IsClass || enumerableType == typeof(string))
                {
                    // TODO: Do this better

                    var collection = stringValue
                        .Select(x => JsonSerializer.Deserialize(x, enumerableType))
                        .Where(x => x != null)
                        .ToList();

                    dict.Add(name, collection);
                }
                else
                {
                    // TODO: Deal with complex collections
                    throw new NotImplementedException();
                }

                continue;
            }
            
            // Nested classes
            if (p.PropertyType.IsClass)
            {
                var childStringDict = stringDict.Where(x => x.Key.StartsWith(p.Name))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                var childDict = ToObjectDictionary(childStringDict, p.PropertyType, p.Name);
                dict.Add(name, childDict);
                continue;
            }

            // I don't know how you got here
            throw new InvalidOperationException($"Cannot add type {p.PropertyType} to object dictionary.");
        }

        return dict;
    }
}
