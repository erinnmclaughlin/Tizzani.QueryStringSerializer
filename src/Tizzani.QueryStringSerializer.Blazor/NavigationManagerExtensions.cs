using System.Diagnostics.CodeAnalysis;
using Tizzani.QueryStringSerializer;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Components;

public static class NavigationManagerExtensions
{
    /// <summary>
    /// Navigates to the specified URI with query string parameters serialized from an object.
    /// </summary>
    /// <param name="navManager"></param>
    /// <param name="uri">The destination URI. This can be absolute, or relative to the base URI.</param>
    /// <param name="obj">The object to serialize into the query string.</param>
    /// <param name="forceLoad">If true, bypasses client-side routing and forces the browser to load the new page from the server, whether or not the URI would normally be handled by the client-side router.</param>
    /// <param name="replace">If true, replaces the current entry in the history stack. If false, appends the new entry to the history stack.</param>
    /// <typeparam name="T"></typeparam>
    public static void NavigateToWithQuery<T>(
        this NavigationManager navManager,
        [StringSyntax("Uri")] string uri, 
        T? obj, 
        bool forceLoad = false, 
        bool replace = false)
    {
        uri = QueryStringSerializer.Serialize(uri, obj);
        navManager.NavigateTo(uri, forceLoad, replace);
    }

    /// <summary>
    /// Navigates to the specified URI with query string parameters serialized from an object.
    /// </summary>
    /// <param name="navManager"></param>
    /// <param name="uri">The destination URI. This can be absolute, or relative to the base URI.</param>
    /// <param name="obj">The object to serialize into the query string.</param>
    /// <param name="options">The options to use for serialization.</param>
    /// <param name="forceLoad">If true, bypasses client-side routing and forces the browser to load the new page from the server, whether or not the URI would normally be handled by the client-side router.</param>
    /// <param name="replace">If true, replaces the current entry in the history stack. If false, appends the new entry to the history stack.</param>
    /// <typeparam name="T"></typeparam>
    public static void NavigateToWithQuery<T>(
        this NavigationManager navManager,
        [StringSyntax("Uri")] string uri, 
        T? obj,
        QueryStringSerializerOptions options,
        bool forceLoad = false,
        bool replace = false)
    {
        uri = QueryStringSerializer.Serialize(uri, obj, options);
        navManager.NavigateTo(uri, forceLoad, replace);
    }

    /// <summary>
    /// Gets the query string parameters from the current URI and deserializes them into an object.
    /// </summary>
    /// <param name="navManager"></param>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <returns>The deserialized object.</returns>
    public static T? GetQueryObject<T>(this NavigationManager navManager)
    {
        var qs = navManager.ToAbsoluteUri(navManager.Uri).Query;
        return QueryStringSerializer.Deserialize<T>(qs);
    }

    /// <summary>
    /// Gets the query string parameters from the current URI and deserializes them into an object.
    /// </summary>
    /// <param name="navManager"></param>
    /// <param name="options">The options to use for deserialization.</param>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <returns>The deserialized object.</returns>
    public static T? GetQueryObject<T>(this NavigationManager navManager, QueryStringSerializerOptions options)
    {
        var qs = navManager.ToAbsoluteUri(navManager.Uri).Query;
        return QueryStringSerializer.Deserialize<T>(qs, options);
    }
}
