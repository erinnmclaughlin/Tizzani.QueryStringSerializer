using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
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
    public void GetJson_ReturnsCorrectJson_ForIntegers(int? value)
    {
        var queryString = string.Empty;

        if (value != null)
            queryString = QueryHelpers.AddQueryString(queryString, "SomeParameter", value.ToString());

        var expectedJson = value == null ? "{}" : $"{{\"SomeParameter\":{value}}}";
        var actualJson = QueryStringSerializer.GetJson<SomeClassWithParameter<int?>>(queryString);
        actualJson.Should().BeEquivalentTo(expectedJson);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("\n \t")]
    [InlineData("hello, world!")]
    public void GetJson_ReturnsCorrectJson_ForStrings(string? value)
    {
        var queryString = string.Empty;

        if (!string.IsNullOrWhiteSpace(value))
            queryString = QueryHelpers.AddQueryString(queryString, "SomeParameter", value.ToString());

        var expectedJson = string.IsNullOrWhiteSpace(value) ? "{}" : $"{{\"SomeParameter\":\"{value}\"}}";
        var actualJson = QueryStringSerializer.GetJson<SomeClassWithParameter<string?>>(queryString);
        actualJson.Should().BeEquivalentTo(expectedJson);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(0)]
    [InlineData(-1, 0, 1, 2)]
    [InlineData(12, 12)]
    [InlineData(null, 1, 12, 12)]
    public void GetJson_ReturnsCorrectJson_ForCollections(params int?[] values)
    {
        var queryString = string.Empty;
        var stringValues = values.Where(v => v != null);

        foreach (var value in stringValues)
            queryString = QueryHelpers.AddQueryString(queryString, "SomeParameter", value.ToString());

        var expectedJson = stringValues.Any() ? $"{{\"SomeParameter\":[{string.Join(',', stringValues)}]}}" : "{}"; 
        var actualJson = QueryStringSerializer.GetJson<SomeClassWithParameter<int?[]>>(queryString);
        actualJson.Should().BeEquivalentTo(expectedJson);
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
}
