namespace Tizzani.QueryStringHelpers;

public static class QueryStringSerializer
{
    public static string Serialize<T>(T obj) where T : class
    {
        return QueryString.FromObject(obj).ToString();
    }
}
