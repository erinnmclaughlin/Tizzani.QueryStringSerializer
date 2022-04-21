using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Linq;
using Tizzani.QueryStringHelpers.Tests.Mocks;
using Xunit;

namespace Tizzani.QueryStringHelpers.Tests;
public class QueryStringSerializerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void ToQueryStringDictionary_CreatesCorrectDictionary_ForIntegers(int? value)
    {
        var someClass = new SomeClassWithParameter<int?>(value);

        var expectedDictionary = new Dictionary<string, object?>
        {
            { nameof(someClass.SomeParameter), value }
        };

        var actualDictionary = QueryStringSerializer.ToQueryStringDictionary(someClass);
        actualDictionary.Should().BeEquivalentTo(expectedDictionary);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void ToObjectDictionary_CreatesCorrectDictionary_ForIntegers(int? value)
    {
        var someClass = new SomeClassWithParameter<int?>(value);
        var queryString = QueryStringSerializer.Serialize(someClass);

        var expectedDictionary = new Dictionary<string, object?>();

        if (value != null)
            expectedDictionary.Add(nameof(someClass.SomeParameter), value);

        var actualDictionary = QueryStringSerializer.ToObjectDictionary<SomeClassWithParameter<int?>>(queryString);
        actualDictionary.Should().BeEquivalentTo(expectedDictionary);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Deserialize_CreatesCorrectObject_ForIntegers(int? value)
    {
        var someClass = new SomeClassWithParameter<int?>(value);
        var queryString = QueryStringSerializer.Serialize(someClass);

        var result = QueryStringSerializer.Deserialize<SomeClassWithParameter<int?>>(queryString);
        result.Should().BeEquivalentTo(someClass);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Serialize_CreatesCorrectQueryString_ForIntegers(int? value)
    {
        var someClass = new SomeClassWithParameter<int?>(value);
        var expectedQueryString = value.HasValue ? $"?SomeParameter={value}" : string.Empty;
        var actualQueryString = QueryStringSerializer.Serialize(someClass);
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

        var actualDictionary = QueryStringSerializer.ToQueryStringDictionary(someClass);
        actualDictionary.Should().BeEquivalentTo(expectedDictionary);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("\n \t")]
    [InlineData("hello, world!")]
    public void Serialize_CreatesCorrectQueryString_ForStrings(string? value)
    {
        var someClass = new SomeClassWithParameter<string?>(value);
        var actualQueryString = QueryStringSerializer.Serialize(someClass);

        if (string.IsNullOrWhiteSpace(value))
        {
            actualQueryString.Should().Be(string.Empty);
        }
        else
        {
            var expectedQueryString = QueryHelpers.AddQueryString("", "SomeParameter", value);
            actualQueryString.Should().Be(expectedQueryString);
        }
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

        var actualQueryString = QueryStringSerializer.Serialize(someClass);
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

        var actualQueryString = QueryStringSerializer.Serialize(someClass);
        actualQueryString.Should().Be(expectedQueryString);
    }





    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("\n \t")]
    [InlineData("hello, world!")]
    public void ToObjectDictionary_CreatesCorrectDictionary_ForStrings(string? value)
    {
        var someClass = new SomeClassWithParameter<string?>(value);
        var queryString = QueryStringSerializer.Serialize(someClass);

        var expectedDictionary = new Dictionary<string, object?>();

        if (!string.IsNullOrWhiteSpace(value))
            expectedDictionary.Add(nameof(someClass.SomeParameter), value);

        var actualDictionary = QueryStringSerializer.ToObjectDictionary<SomeClassWithParameter<string?>>(queryString);
        actualDictionary.Should().BeEquivalentTo(expectedDictionary);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1, 0, 1, 2)]
    [InlineData(12, 12)]
    [InlineData(null, 1, 12, 12)]
    public void ToObjectDictionary_CreatesCorrectDictionary_ForCollections(params int?[] values)
    {
        var someClass = new SomeClassWithParameter<int?[]>(values);
        var queryString = QueryStringSerializer.Serialize(someClass);

        var expectedDictionary = new Dictionary<string, object?>();
        var expectedValues = values?.Where(v => v != null).ToArray();

        if (expectedValues?.Any() == true)
            expectedDictionary.Add(nameof(someClass.SomeParameter), expectedValues);

        var actualDictionary = QueryStringSerializer.ToObjectDictionary<SomeClassWithParameter<int?[]>>(queryString);
        actualDictionary.Should().BeEquivalentTo(expectedDictionary);
    }
}
