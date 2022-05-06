using Microsoft.AspNetCore.Components;

namespace Tizzani.QueryStringSerializer.Blazor;

public static class NavigationManagerExtensions
{
    public static void NavigateToWithQuery(this NavigationManager navManager, string baseUri, object obj)
    {
        var uri = QueryStringSerializer.Serialize(obj, baseUri);
        navManager.NavigateTo(uri);
    }

    public static void NavigateToWithQuery(this NavigationManager navManager, string baseUri, object obj, QueryStringSerializerOptions options)
    {
        var uri = QueryStringSerializer.Serialize(obj, baseUri, options);
        navManager.NavigateTo(uri);
    }

    public static T? GetQueryObject<T>(this NavigationManager navManager)
    {
        var qs = navManager.ToAbsoluteUri(navManager.Uri).Query;
        return QueryStringSerializer.Deserialize<T>(qs);
    }

    public static T? GetQueryObject<T>(this NavigationManager navManager, QueryStringSerializerOptions options)
    {
        var qs = navManager.ToAbsoluteUri(navManager.Uri).Query;
        return QueryStringSerializer.Deserialize<T>(qs, options);
    }
}
