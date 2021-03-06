using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.Collections;
using System.Text.Json;

namespace Tizzani.QueryStringSerializer;

public static class QueryStringSerializer
{
    public static string Serialize<T>(T obj, QueryStringSerializerOptions? options = null)
    {
        options ??= new QueryStringSerializerOptions();
        var jsonSerializerOptions = options.GetJsonSerializerOptions();

        var json = JsonSerializer.Serialize(obj, jsonSerializerOptions);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(json, jsonSerializerOptions);

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

    public static T? Deserialize<T>(string uri, QueryStringSerializerOptions? options = null)
    {
        options ??= new QueryStringSerializerOptions();
        var jsonSerializerOptions = options.GetJsonSerializerOptions();

        var json = GetJson<T>(uri, jsonSerializerOptions);
        return JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
    }

    private static string GetJson<T>(string uri, JsonSerializerOptions jsonSerializerOptions)
    {
        var queryParams = QueryHelpers.ParseQuery(uri);
        var dict = queryParams.ToObjectDictionary(typeof(T), jsonSerializerOptions);
        return JsonSerializer.Serialize(dict, jsonSerializerOptions);
    }

    private static Dictionary<string, object?> ToObjectDictionary(this Dictionary<string, StringValues> stringDict, Type type, JsonSerializerOptions jsonSerializerOptions)
    {
        var dict = new Dictionary<string, object?>();

        foreach (var p in type.GetProperties())
        {
            var propertyType = p.PropertyType;
            
            // Nested classes
            if (propertyType.IsClass && !typeof(IEnumerable).IsAssignableFrom(propertyType))
            {
                var childStringDict = stringDict
                    .Where(x => x.Key.StartsWith(p.Name + '.'))
                    .ToDictionary(kvp => kvp.Key.Replace(p.Name + '.', ""), kvp => kvp.Value);

                var childDict = ToObjectDictionary(childStringDict, propertyType, jsonSerializerOptions);
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
            if (TryGetCollectionType(propertyType, out var enumerableType))
            {
                if (enumerableType.IsEnum)
                {
                    var collection = stringValue
                        .Where(x =>
                        {
                            try
                            {
                                Enum.Parse(enumerableType, x);
                                return true;
                            }
                            catch (Exception)
                            {
                                return false;
                            }
                        })
                        .Select(x => Enum.Parse(enumerableType, x))
                        .ToList();

                    dict.Add(p.Name, collection);
                }
                else if (!enumerableType.IsClass || enumerableType == typeof(string))
                {
                    var collection = stringValue
                        .Select(x => JsonSerializer.Deserialize(x, enumerableType, jsonSerializerOptions))
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
            if (propertyType.IsGenericType)
                propertyType = propertyType.GenericTypeArguments.First();
                
            if (!propertyType.IsClass || propertyType == typeof(string))
            {
                var str = stringValue.ToString();

                if (propertyType.IsEnum)
                {
                    if(Enum.TryParse(propertyType, str, out var value))
                        dict.Add(p.Name, value);
                    
                    continue;
                }

                if (propertyType == typeof(bool))
                {
                    if(Boolean.TryParse(str, out var value))
                        dict.Add(p.Name, value);
                    
                    continue;
                }

                if (propertyType == typeof(string))
                {
                    dict.Add(p.Name, str);
                    continue;
                }

                try
                {
                    dict.Add(p.Name, JsonSerializer.Deserialize(str, p.PropertyType, jsonSerializerOptions));
                }
                catch (Exception)
                {
                    // if we fail to deserialize, that's fine; ignore the value
                }
                
                continue;
            }

            // I don't know how you got here
            throw new InvalidOperationException($"Cannot add type {p.PropertyType} to object dictionary.");
        }

        return dict;
    }

    private static bool TryGetCollectionType(Type type, out Type collectionType)
    {
        foreach (var i in type.GetInterfaces())
        {
            if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))
            {
                collectionType = i.GenericTypeArguments[0];
                return true;
            }
        }

        collectionType = default!;
        return false;
    }
}
