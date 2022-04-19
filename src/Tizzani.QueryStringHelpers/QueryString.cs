using Microsoft.AspNetCore.WebUtilities;
using System.Collections;

namespace Tizzani.QueryStringHelpers;

public class QueryString
{
    private Dictionary<string, string> Parameters { get; } = new();

    public string[] Keys => Parameters.Keys.ToArray();
    public string this[string key] => Parameters[key];

    internal QueryString(Dictionary<string, string> parameters)
    {
        Parameters = parameters;
    }

    public static QueryString FromObject<T>(T obj) where T : class
    {
        var parameters = GetParametersFromObject(obj);
        return new QueryString(parameters);
    }

    public static QueryString FromUri(string uri)
    {
        var parameters = QueryHelpers.ParseQuery(uri)
            .ToDictionary(p => p.Key, p => p.Value.ToString());

        return new QueryString(parameters);
    }

    public bool ContainsKey(string key) => Parameters.ContainsKey(key);

    public string ToUri(string baseUri)
    {
        var uri = baseUri;

        foreach (var qp in Parameters)
            uri = QueryHelpers.AddQueryString(uri, qp.Key, qp.Value);

        return uri;
    }

    public override string ToString()
    {
        return ToUri("").Trim('?');
    }

    internal QueryString GetChild(string parentKey)
    {
        var parameters = Parameters.Where(p => p.Key.StartsWith(parentKey))
            .ToDictionary(p => p.Key.Replace($"{parentKey}", ""), p => p.Value);

        return new QueryString(parameters);
    }

    private static Dictionary<string, string> GetParametersFromObject(object obj, string namePrefix = "")
    {
        var parameters = new Dictionary<string, string>();

        foreach (var p in obj.GetType().GetProperties())
        {
            var value = p.GetValue(obj);

            if (value == null)
                continue;

            var name = string.IsNullOrWhiteSpace(namePrefix) ? p.Name : namePrefix + "." + p.Name;

            if (!p.PropertyType.IsClass || p.PropertyType == typeof(string))
            {
                var valueString = value.ToString();

                if (!string.IsNullOrWhiteSpace(valueString))
                    parameters.Add(name, valueString);
            }
            else if (IsCollection(p.PropertyType))
            {
                var collection = ((IEnumerable)value).OfType<string?>().Where(c => !string.IsNullOrWhiteSpace(c));
                var collectionString = string.Join(",", collection);

                if (!string.IsNullOrWhiteSpace(collectionString))
                    parameters.Add(name, collectionString);
            }
            else
            {
                var childDict = GetParametersFromObject(value, p.Name);

                foreach (var kvp in childDict)
                    parameters.Add(kvp.Key, kvp.Value);
            }
        }

        return parameters;
    }

    private static bool IsCollection(Type t) => typeof(IEnumerable).IsAssignableFrom(t) && t != typeof(string);
}
