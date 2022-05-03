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
    [InlineData("SomeParameter=hello, world!", "hello, world!")]
    [InlineData("SomeParameter=hello,+world!", "hello, world!")]
    [InlineData("SomeParameter=hello,%20world!", "hello, world!")]
    [InlineData("SomeParameter=hello%2C%20world!", "hello, world!")]
    public void Deserialize_CreatesCorrectObject_ForStrings(string queryString, string? expectedValue)
    {
        var result = QueryStringSerializer.Deserialize<SomeClassWithParameter<string?>>(queryString);
        result.Should().NotBeNull();
        result!.SomeParameter.Should().Be(expectedValue);
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
        var expectedQueryString = string.IsNullOrWhiteSpace(value) ? string.Empty : QueryHelpers.AddQueryString("", "SomeParameter", value).Trim('?');
        actualQueryString.Should().Be(expectedQueryString);
    }

    [Theory]
    [InlineData("SomeParameter=0", 0)]
    [InlineData("SomeParameter=1", 1)]
    [InlineData("SomeParameter=-1", -1)]
    [InlineData("SomeParameter=12345", 12345)]
    public void Deserialize_CreatesCorrectObject_ForIntegers(string queryString, int? expectedValue)
    {
        var result = QueryStringSerializer.Deserialize<SomeClassWithParameter<int?>>(queryString);
        result.Should().NotBeNull();
        result!.SomeParameter.Should().Be(expectedValue);
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
        var expectedQueryString = value.HasValue ? $"SomeParameter={value}" : string.Empty;
        var actualQueryString = QueryStringSerializer.Serialize(someClass).Trim('?');
        actualQueryString.Should().Be(expectedQueryString);
    }

    [Theory]
    [InlineData("SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=0&SomeParameter=0&SomeParameter=0", 0, 0, 0)]
    [InlineData("SomeParameter=-1&SomeParameter=1", -1, 1)]
    [InlineData("SomeParameter=1", 1)]
    public void Deserialize_CreatesCorrectObject_ForArrays(string queryString, params int[] expectedValues)
    {
        var result = QueryStringSerializer.Deserialize<SomeClassWithParameter<int[]>>(queryString);
        result.Should().NotBeNull();
        result!.SomeParameter.Should().BeEquivalentTo(expectedValues);
    }

    [Theory]
    [InlineData("SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=0&SomeParameter=0&SomeParameter=0", 0, 0, 0)]
    [InlineData("SomeParameter=-1&SomeParameter=1", -1, 1)]
    [InlineData("SomeParameter=1", 1)]
    public void Serialize_CreatesCorrectQueryString_ForArrays(string expectedQueryString, params int[] value)
    {
        var someClass = new SomeClassWithParameter<int[]>(value);
        var actualQueryString = QueryStringSerializer.Serialize(someClass);
        actualQueryString.Should().Be(expectedQueryString);
    }

    [Theory]
    [InlineData("SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=0&SomeParameter=0&SomeParameter=0", 0, 0, 0)]
    [InlineData("SomeParameter=-1&SomeParameter=1", -1, 1)]
    [InlineData("SomeParameter=1", 1)]
    public void Serialize_CreatesCorrectQueryString_ForLists(string expectedQueryString, params int[] value)
    {
        var someClass = new SomeClassWithParameter<List<int>>(value.ToList());
        var actualQueryString = QueryStringSerializer.Serialize(someClass);
        actualQueryString.Should().Be(expectedQueryString);
    }

    [Theory]
    [InlineData("SomeParameter.SomeParameter=1", 1)]
    [InlineData("SomeParameter.SomeParameter=-1", -1)]
    [InlineData("SomeParameter.SomeParameter=0", 0)]
    [InlineData("SomeParameter.SomeParameter=1234", 1234)]
    public void Deserialize_CreatesCorrectObject_ForNestedObjects(string queryString, int expectedValue)
    {
        var result = QueryStringSerializer.Deserialize<SomeClassWithParameter<SomeClassWithParameter<int>>>(queryString);
        result?.SomeParameter.Should().NotBeNull();
        result!.SomeParameter!.SomeParameter.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("SomeParameter.SomeParameter=1", 1)]
    [InlineData("SomeParameter.SomeParameter=-1", -1)]
    [InlineData("SomeParameter.SomeParameter=0", 0)]
    [InlineData("SomeParameter.SomeParameter=1234", 1234)]
    public void Serialize_CreatesCorrectQueryString_ForNestedObjects(string expectedQueryString, int value)
    {
        var someClass = new SomeClassWithParameter<SomeClassWithParameter<int>>(new SomeClassWithParameter<int>(value));

        var actualQueryString = QueryStringSerializer.Serialize(someClass);
        actualQueryString.Should().Be(expectedQueryString);
    }

    [Theory]
    [InlineData("SomeParameter.SomeString=hello,world!&SomeParameter.SomeParameter=1", 1, "hello,world!")]
    public void Serialize_CreatesCorrectQueryString_ForMoreNestedObjects(string expectedQueryString, int intValue, string stringValue)
    {
        var someClassWithInt = new SomeClassWithParameter<int>(intValue) { SomeString = stringValue };
        var someClass = new SomeClassWithParameter<SomeClassWithParameter<int>>(someClassWithInt);

        var actualQueryString = QueryStringSerializer.Serialize(someClass);
        actualQueryString.Should().Be(expectedQueryString);
    }
}
