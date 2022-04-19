using System.Collections;

namespace Tizzani.QueryStringHelpers.Extensions;

internal static class TypeExtensions
{
    public static bool IsCollection(this Type t) =>
        typeof(IEnumerable).IsAssignableFrom(t) && t != typeof(string);
}
