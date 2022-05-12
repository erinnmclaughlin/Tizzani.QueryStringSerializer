using System;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Linq;
using Tizzani.QueryStringSerializer.Tests.Mocks;
using Xunit;

namespace Tizzani.QueryStringSerializer.Tests;

public class QueryStringSerializerTests
{

    [Theory]
    [InlineData("?SomeParameter=hello, world!", "hello, world!")]
    [InlineData("SomeParameter=hello, world!", "hello, world!")]
    [InlineData("SomeParameter=hello,+world!", "hello, world!")]
    [InlineData("SomeParameter=hello,%20world!", "hello, world!")]
    [InlineData("SomeParameter=hello%2C%20world!", "hello, world!")]
    public void Deserialize_CreatesCorrectObject_ForStrings(string queryString, string? expectedValue)
    {
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<string?>>(queryString);
        result.Should().NotBeNull();
        result!.SomeParameter.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("\n \t")]
    [InlineData("hello, world!")]
    public void Serialize_CreatesCorrectQueryString_ForStringsInGenericStringClass(string? value)
    {
        var someClass = new SomeGenericClassWithParameter<string?>(value);
        var actualQueryString = QueryStringSerializer.Serialize(someClass);
        var expectedQueryString = string.IsNullOrWhiteSpace(value) ? string.Empty : QueryHelpers.AddQueryString("", "SomeParameter", value).Trim('?');
        actualQueryString.Should().Be(expectedQueryString);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("\n \t")]
    [InlineData("hello, world!")]
    public void Serialize_CreatesCorrectQueryString_ForStringsInNonGenericStringClass(string value)
    {
        var someClass = new SomeClassWithStringParameter() { SomeParameter = value };
        var actualQueryString = QueryStringSerializer.Serialize(someClass);
        var expectedQueryString = string.IsNullOrWhiteSpace(value) ? string.Empty : QueryHelpers.AddQueryString("", "SomeParameter", value).Trim('?');
        actualQueryString.Should().Be(expectedQueryString);
    }

    [Theory]
    [InlineData("?SomeParameter=0", 0)]
    [InlineData("SomeParameter=0", 0)]
    [InlineData("SomeParameter=1", 1)]
    [InlineData("SomeParameter=-1", -1)]
    [InlineData("SomeParameter=12345", 12345)]
    [InlineData("", null)]
    [InlineData("?", null)]
    public void Deserialize_CreatesCorrectObject_ForNullableIntegersInGenericIntClass(string queryString, int? expectedValue)
    {
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<int?>>(queryString);
        result.Should().NotBeNull();
        result!.SomeParameter.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("?SomeParameter=0", 0)]
    [InlineData("SomeParameter=0", 0)]
    [InlineData("SomeParameter=1", 1)]
    [InlineData("SomeParameter=-1", -1)]
    [InlineData("SomeParameter=12345", 12345)]
    public void Deserialize_CreatesCorrectObject_ForIntegersInNonGenericIntClass(string queryString, int expectedValue)
    {
        var result = QueryStringSerializer.Deserialize<SomeClassWithIntParameter>(queryString);
        result.Should().NotBeNull();
        result!.SomeParameter.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("?SomeParameter=1&SomeExtraneousParameter=anything", 1)]
    [InlineData("SomeParameter=1&SomeExtraneousParameter=anything", 1)]
    [InlineData("SomeExtraneousParameter=anything", 0)]
    public void Deserialize_IgnoresExtraneousProperties(string queryString, int expectedValue)
    {
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<int>>(queryString);
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
        var someClass = new SomeGenericClassWithParameter<int?>(value);
        var expectedQueryString = value.HasValue ? $"SomeParameter={value}" : string.Empty;
        var actualQueryString = QueryStringSerializer.Serialize(someClass).Trim('?');
        actualQueryString.Should().Be(expectedQueryString);
    }

    [Theory]
    [InlineData("?SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=0&SomeParameter=0&SomeParameter=0", 0, 0, 0)]
    [InlineData("SomeParameter=-1&SomeParameter=1", -1, 1)]
    [InlineData("SomeParameter=1", 1)]
    public void Deserialize_CreatesCorrectObject_ForArrays(string queryString, params int[] expectedValues)
    {
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<int[]>>(queryString);
        result.Should().NotBeNull();
        result!.SomeParameter.Should().BeEquivalentTo(expectedValues);
    }

    [Fact]
    public void Deserialize_CreatesCorrectObject_ForListOfEnums()
    {
        const string qs = "SomeParameter=Milk&SomeParameter=Sugar";
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<List<SomeEnum>>>(qs);
        result!.SomeParameter.Should().BeEquivalentTo(new List<SomeEnum> { SomeEnum.Milk, SomeEnum.Sugar });
    }

    [Fact]
    public void Deserialize_IgnoresInvalidEnumValues()
    {
        const string qs = "SomeParameter=anInvalid_value";
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<SomeEnum?>>(qs);
        result!.SomeParameter.Should().Be(null);
    }

    [Theory]
    [InlineData("?SomeParameter=invalid3")] // alpha before and after a number
    [InlineData("SomeParameter=invalid3")]
    [InlineData("SomeParameter=3invalid")]
    [InlineData("SomeParameter=!3")] // symbols before and after a number
    [InlineData("SomeParameter=3!")]
    [InlineData("SomeParameter=3.2")] // decimal values
    [InlineData("SomeParameter=99999999999999999999")] // > Int.MaxValue
    public void Deserialize_IgnoresInvalidIntValues_ForGenericIntClass(string queryString)
    {
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<int>>(queryString);
        result!.SomeParameter.Should().Be(0);
    }

    [Theory]
    [InlineData("?SomeParameter=invalid3")] // alpha before and after a number
    [InlineData("SomeParameter=invalid3")]
    [InlineData("SomeParameter=3invalid")]
    [InlineData("SomeParameter=!3")] // symbols before and after a number
    [InlineData("SomeParameter=3!")]
    [InlineData("SomeParameter=3.2")] // decimal values
    [InlineData("SomeParameter=99999999999999999999")] // > Int.MaxValue
    public void Deserialize_IgnoresInvalidIntValues_ForNonGenericIntClass(string queryString)
    {
        var result = QueryStringSerializer.Deserialize<SomeClassWithIntParameter>(queryString);
        result!.SomeParameter.Should().Be(0);
    }

    [Theory]
    [InlineData("?SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=0&SomeParameter=0&SomeParameter=0", 0, 0, 0)]
    [InlineData("SomeParameter=-1&SomeParameter=1", -1, 1)]
    [InlineData("SomeParameter=1", 1)]
    public void Deserialize_CreatesCorrectObject_ForLists(string queryString, params int[] expectedValues)
    {
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<List<int>>>(queryString);
        result.Should().NotBeNull();
        result!.SomeParameter.Should().BeEquivalentTo(expectedValues);
    }

    [Theory]
    [InlineData("?SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=0&SomeParameter=0&SomeParameter=0", 0, 0, 0)]
    [InlineData("SomeParameter=-1&SomeParameter=1", -1, 1)]
    [InlineData("SomeParameter=1", 1)]
    public void Deserialize_CreatesCorrectObject_ForILists(string queryString, params int[] expectedValues)
    {
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<IList<int>>>(queryString);
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
        var someClass = new SomeGenericClassWithParameter<int[]>(value);
        var actualQueryString = QueryStringSerializer.Serialize(someClass);
        actualQueryString.Should().Be(expectedQueryString);
    }

    [Theory]
    [InlineData("SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=0&SomeParameter=0&SomeParameter=0", 0, 0, 0)]
    [InlineData("SomeParameter=-1&SomeParameter=1", -1, 1)]
    [InlineData("SomeParameter=1", 1)]
    public void Serialize_CreatesCorrectQueryString_ForSystemArray(string expectedQueryString, params int[] value)
    {
        var someClass = new SomeGenericClassWithParameter<Array>(value);
        var actualQueryString = QueryStringSerializer.Serialize(someClass);
        actualQueryString.Should().Be(expectedQueryString);
    }

    [Theory]
    [InlineData("SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=0&SomeParameter=0&SomeParameter=0", 0, 0, 0)]
    [InlineData("SomeParameter=-1&SomeParameter=1", -1, 1)]
    [InlineData("SomeParameter=1", 1)]
    public void Serialize_CreatesCorrectQueryString_ForICollection(string expectedQueryString, params int[] value)
    {
        var someClass = new SomeGenericClassWithParameter<ICollection<int>>(value);
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
        var someClass = new SomeGenericClassWithParameter<List<int>>(value.ToList());
        var actualQueryString = QueryStringSerializer.Serialize(someClass);
        actualQueryString.Should().Be(expectedQueryString);
    }

    [Theory]
    [InlineData("?SomeParameter.SomeParameter=1", 1)]
    [InlineData("SomeParameter.SomeParameter=1", 1)]
    [InlineData("SomeParameter.SomeParameter=-1", -1)]
    [InlineData("SomeParameter.SomeParameter=0", 0)]
    [InlineData("SomeParameter.SomeParameter=1234", 1234)]
    public void Deserialize_CreatesCorrectObject_ForNestedObjects(string queryString, int expectedValue)
    {
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<SomeGenericClassWithParameter<int>>>(queryString);
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
        var someClass = new SomeGenericClassWithParameter<SomeClassWithIntParameter>(new SomeClassWithIntParameter() { SomeParameter = value });

        var actualQueryString = QueryStringSerializer.Serialize(someClass);
        actualQueryString.Should().Be(expectedQueryString);
    }

    [Theory]
    [InlineData("SomeParameter.SomeString=hello,world!&SomeParameter.SomeParameter=1", 1, "hello,world!")]
    public void Serialize_CreatesCorrectQueryString_ForMoreNestedObjects(string expectedQueryString, int intValue, string stringValue)
    {
        var someClassWithInt = new SomeGenericClassWithParameter<int>(intValue) { SomeString = stringValue };
        var someClass = new SomeGenericClassWithParameter<SomeGenericClassWithParameter<int>>(someClassWithInt);

        var actualQueryString = QueryStringSerializer.Serialize(someClass);
        actualQueryString.Should().Be(expectedQueryString);
    }

    [Theory]
    [InlineData("?SomeString=Anything", "Anything")]
    [InlineData("SomeString=Anything", "Anything")]
    public void Deserialize_CreatesCorrectObject_ForRecords(string queryString, string expectedValue)
    {
        var result = QueryStringSerializer.Deserialize<SomeRecord>(queryString);
        result?.SomeString.Should().NotBeNull();
        result!.SomeString.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("?SomeString=Anything", "Anything")]
    [InlineData("SomeString=Anything", "Anything")]
    public void Deserialize_CreatesCorrectObject_ForStructs(string queryString, string expectedValue)
    {
        var result = QueryStringSerializer.Deserialize<SomeStruct>(queryString);
        result.SomeString.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("?SomeString=Anything", "Anything")]
    [InlineData("SomeString=Anything", "Anything")]
    public void Deserialize_CreatesCorrectObject_ForReadonlyStructs(string queryString, string expectedValue)
    {
        var result = QueryStringSerializer.Deserialize<SomeReadonlyStruct>(queryString);
        result.SomeString.Should().Be(expectedValue);
    }

    [Fact]
    public void Serialize_SerializesEnumAsString_WithDefaultOptions()
    {
        var someClass = new SomeGenericClassWithParameter<SomeEnum>(SomeEnum.BigBanana);
        var expectedQueryString = "SomeParameter=BigBanana";

        var actualQueryString = QueryStringSerializer.Serialize(someClass);
        actualQueryString.Should().Be(expectedQueryString);
    }

    [Fact]
    public void Serialize_SerializesEnumAsInt_WhenOptionsHaveEnumAsStringAsFalse()
    {
        var options = new QueryStringSerializerOptions { EnumsAsStrings = false };
        var someClass = new SomeGenericClassWithParameter<SomeEnum>(SomeEnum.BigBanana);
        var expectedQueryString = $"SomeParameter={(int)SomeEnum.BigBanana}";

        var actualQueryString = QueryStringSerializer.Serialize(someClass, options);
        actualQueryString.Should().Be(expectedQueryString);
    }

    [Fact]
    public void Deserialize_CreatesCorrectObject_ForNestedClassesWithEnums()
    {
        var queryString = "SomeParameter.SomeParameter=Milk";
        var someClass = new SomeGenericClassWithParameter<SomeGenericClassWithParameter<SomeEnum>>
        {
            SomeParameter = new SomeGenericClassWithParameter<SomeEnum>(SomeEnum.Milk)
        };

        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<SomeGenericClassWithParameter<SomeEnum>>>(queryString);
        result!.SomeParameter!.SomeParameter.Should().Be(someClass.SomeParameter.SomeParameter);
    }
}
