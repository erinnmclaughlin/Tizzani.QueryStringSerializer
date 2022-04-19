using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Linq;
using Tizzani.QueryStringHelpers.Extensions;
using Xunit;

namespace Tizzani.QueryStringHelpers.Tests.Extensions;
public class ObjectExtensionsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void ToDictionary_CreatesCorrectDictionary_ForIntegers(int? value)
    {
        var someClass = new SomeClassWithParameter<int?>(value);

        var expectedDictionary = new Dictionary<string, object?>
        {
            { nameof(someClass.SomeParameter), value }
        };

        var actualDictionary = ObjectExtensions.ToQueryStringDictionary(someClass);
        actualDictionary.Should().BeEquivalentTo(expectedDictionary);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void ToQueryString_CreatesCorrectQueryString_ForIntegers(int? value)
    {
        var someClass = new SomeClassWithParameter<int?>(value);
        var expectedQueryString = value.HasValue ? $"?SomeParameter={value}" : string.Empty;
        var actualQueryString = ObjectExtensions.ToQueryString(someClass);
        actualQueryString.Should().Be(expectedQueryString);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("\n \t")]
    [InlineData("hello, world!")]
    public void ToDictionary_CreatesCorrectDictionary_ForStrings(string? value)
    {
        var someClass = new SomeClassWithParameter<string?>(value);

        var expectedDictionary = new Dictionary<string, object?>
        {
            { nameof(someClass.SomeParameter), value }
        };

        var actualDictionary = ObjectExtensions.ToQueryStringDictionary(someClass);
        actualDictionary.Should().BeEquivalentTo(expectedDictionary);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1, 0, 1, 2)]
    [InlineData(12, 12)]
    [InlineData(null, 1, 12, 12)]
    public void ToDictionary_CreatesCorrectDictionary_ForCollections(params int?[] values)
    {
        var someClass = new SomeClassWithParameter<int?[]>(values);

        var expectedQueryString = "";

        if (values != null)
        {
            var name = nameof(someClass.SomeParameter);

            values.Where(v => v != null).ToList()
                .ForEach(v => expectedQueryString = QueryHelpers
                    .AddQueryString(expectedQueryString, name, v.ToString()));
        }

        var actualQueryString = ObjectExtensions.ToQueryString(someClass);
        actualQueryString.Should().Be(expectedQueryString);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("\n \t")]
    [InlineData("hello, world!")]
    [InlineData("12345")]
    [InlineData("SomeParameter=SomeValue")]
    [InlineData("!@#$%^&*()?=")]
    public void ToQueryString_CreatesCorrectQueryString_ForStrings(string? value)
    {
        var someClass = new SomeClassWithParameter<string?>(value);

        var expectedQueryString = string.IsNullOrWhiteSpace(value)
            ? "" : QueryHelpers.AddQueryString("", nameof(someClass.SomeParameter), value);

        var actualQueryString = ObjectExtensions.ToQueryString(someClass);
        actualQueryString.Should().Be(expectedQueryString);
    }

    internal class SomeClassWithParameter<T>
    {
        public T? SomeParameter { get; set; }

        public SomeClassWithParameter(T? value)
        {
            SomeParameter = value;
        }
    }
}
