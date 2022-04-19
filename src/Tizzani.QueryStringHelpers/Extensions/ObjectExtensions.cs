namespace Tizzani.QueryStringHelpers.Extensions;

public static class ObjectExtensions
{
    public static QueryString AsQueryString<T>(this T obj) where T : class
    {
        return QueryString.FromObject(obj);
    }

    public static string AsQueryString<T>(this T obj, string baseUri) where T : class
    {
        return obj.AsQueryString().ToUri(baseUri);
    }
}
