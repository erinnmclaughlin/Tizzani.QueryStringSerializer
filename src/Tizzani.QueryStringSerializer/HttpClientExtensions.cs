using System.Diagnostics.CodeAnalysis;
using Tizzani.QueryStringSerializer;

// ReSharper disable once CheckNamespace
namespace System.Net.Http.Json;

public static class HttpClientExtensions
{
    /// <summary>
    /// Sends a GET request to the specified URL.
    /// </summary>
    /// <param name="client">The client used to send the request.</param>
    /// <param name="requestUri">The URI the request is sent to.</param>
    /// <param name="requestQuery">The object that should be serialized to the request query string.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>The response message.</returns>
    public static async Task<HttpResponseMessage> GetWithQueryAsync(
        this HttpClient client,
        [StringSyntax("Uri")] string requestUri,
        object? requestQuery,
        CancellationToken cancellationToken = default)
    {
        var uri = QueryStringSerializer.Serialize(requestUri, requestQuery);
        return await client.GetAsync(uri, cancellationToken);
    }
    
    /// <summary>
    /// Sends a GET request to the specified URL and deserializes the response into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="client">The client used to send the request.</param>
    /// <param name="requestUri">The URI the request is sent to.</param>
    /// <param name="requestQuery">The object that should be serialized to the request query string.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <returns>The deserialized response.</returns>
    public static async Task<T?> GetFromJsonWithQueryAsync<T>(
        this HttpClient client,
        [StringSyntax("Uri")] string requestUri,
        object? requestQuery,
        CancellationToken cancellationToken = default)
    {
        var uri = QueryStringSerializer.Serialize(requestUri, requestQuery);
        return await client.GetFromJsonAsync<T>(uri, cancellationToken);
    }
}