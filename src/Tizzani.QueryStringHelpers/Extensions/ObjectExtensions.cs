using Microsoft.AspNetCore.WebUtilities;
using System.Collections;
using System.Web;

namespace Tizzani.QueryStringHelpers.Extensions;

public static class ObjectExtensions
{
    public static string ToQueryString(this object obj)
    {
        var dict = obj.ToQueryStringDictionary();

        string uri = "";

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

    public static Dictionary<string, object?> ToQueryStringDictionary(this object obj, string namePrefix = "")
    {
        var dict = new Dictionary<string, object?>();

        foreach (var p in obj.GetType().GetProperties())
        {
            var name = string.IsNullOrWhiteSpace(namePrefix) ? p.Name : namePrefix + "." + p.Name;
            var value = p.GetValue(obj);

            if (!p.PropertyType.IsClass || typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
            {
                dict.Add(name, value);
            }
            else
            {
                var childDict = value?.ToQueryStringDictionary(name);
                dict.Add(name, childDict);
            }
        }

        return dict;
    }
}
