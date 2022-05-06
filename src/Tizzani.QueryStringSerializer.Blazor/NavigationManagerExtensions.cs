using Microsoft.AspNetCore.Components;

namespace Tizzani.QueryStringSerializer.Blazor;

public static class NavigationManagerExtensions
{
    public static void NavigateToWithQuery(this NavigationManager navManager, string baseUri, object obj, bool forceLoad = false, bool replace = false)
    {
        var uri = QueryStringSerializer.Serialize(obj, baseUri);
        navManager.NavigateTo(uri, forceLoad, replace);
    }

    public static void NavigateToWithQuery(this NavigationManager navManager, string baseUri, object obj, QueryStringSerializerOptions options, bool forceLoad = false, bool replace = false)
    {
        var uri = QueryStringSerializer.Serialize(obj, baseUri, options);
        navManager.NavigateTo(uri, forceLoad, replace);
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
