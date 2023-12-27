using System;
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
    public void Deserialize_CreatesCorrectObject_ForStrings(string queryString, string expected)
    {
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<string>>(queryString);

        Assert.NotNull(result);
        Assert.Equal(expected, result.SomeParameter);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("  ", "")]
    [InlineData("\n \t", "")]
    [InlineData("hello, world!", "SomeParameter=hello,%20world!")]
    public void Serialize_CreatesCorrectQueryString_ForStringsInGenericStringClass(string? value, string expected)
    {
        var someClass = new SomeGenericClassWithParameter<string>(value);
        var actual = QueryStringSerializer.Serialize(someClass);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("  ", "")]
    [InlineData("\n \t", "")]
    [InlineData("hello, world!", "SomeParameter=hello,%20world!")]
    public void Serialize_CreatesCorrectQueryString_ForStringsInNonGenericStringClass(string value, string expected)
    {
        var someClass = new SomeClassWithStringParameter() { SomeParameter = value };
        var actual = QueryStringSerializer.Serialize(someClass);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("?SomeParameter=0", 0)]
    [InlineData("SomeParameter=0", 0)]
    [InlineData("SomeParameter=1", 1)]
    [InlineData("SomeParameter=-1", -1)]
    [InlineData("SomeParameter=12345", 12345)]
    [InlineData("", null)]
    [InlineData("?", null)]
    public void Deserialize_CreatesCorrectObject_ForNullableIntegersInGenericIntClass(string queryString, int? expected)
    {
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<int?>>(queryString);

        Assert.NotNull(result);
        Assert.Equal(expected, result.SomeParameter);
    }

    [Theory]
    [InlineData("?SomeParameter=0", 0)]
    [InlineData("SomeParameter=0", 0)]
    [InlineData("SomeParameter=1", 1)]
    [InlineData("SomeParameter=-1", -1)]
    [InlineData("SomeParameter=12345", 12345)]
    public void Deserialize_CreatesCorrectObject_ForIntegersInNonGenericIntClass(string queryString, int expected)
    {
        var result = QueryStringSerializer.Deserialize<SomeClassWithIntParameter>(queryString);

        Assert.NotNull(result);
        Assert.Equal(expected, result.SomeParameter);
    }

    [Theory]
    [InlineData("?SomeParameter=1&SomeExtraneousParameter=anything", 1)]
    [InlineData("SomeParameter=1&SomeExtraneousParameter=anything", 1)]
    [InlineData("SomeExtraneousParameter=anything", 0)]
    public void Deserialize_IgnoresExtraneousProperties(string queryString, int expected)
    {
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<int>>(queryString);

        Assert.NotNull(result);
        Assert.Equal(expected, result.SomeParameter);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData(0, "SomeParameter=0")]
    [InlineData(1, "SomeParameter=1")]
    [InlineData(-1, "SomeParameter=-1")]
    [InlineData(int.MaxValue, "SomeParameter=2147483647")]
    [InlineData(int.MinValue, "SomeParameter=-2147483648")]
    public void Serialize_CreatesCorrectQueryString_ForIntegers(int? value, string expected)
    {
        var someClass = new SomeGenericClassWithParameter<int?>(value);
        var actual = QueryStringSerializer.Serialize(someClass).Trim('?');

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("?SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=0&SomeParameter=0&SomeParameter=0", 0, 0, 0)]
    [InlineData("SomeParameter=-1&SomeParameter=1", -1, 1)]
    [InlineData("SomeParameter=1", 1)]
    public void Deserialize_CreatesCorrectObject_ForArrays(string queryString, params int[] expected)
    {
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<int[]>>(queryString);

        Assert.NotNull(result);
        Assert.Equivalent(expected, result.SomeParameter);
    }

    [Theory]
    [InlineData("?SomeParameter=Milk&SomeParameter=Sugar", SomeEnum.Milk, SomeEnum.Sugar)]
    [InlineData("SomeParameter=Milk&SomeParameter=Sugar", SomeEnum.Milk, SomeEnum.Sugar)]
    public void Deserialize_CreatesCorrectObject_ForListOfEnums(string queryString, params SomeEnum[] expected)
    {
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<List<SomeEnum>>>(queryString);

        Assert.NotNull(result);
        Assert.Equivalent(expected, result.SomeParameter);
    }

    [Fact]
    public void Deserialize_IgnoresInvalidEnumValues()
    {
        const string qs = "SomeParameter=anInvalid_value";
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<SomeEnum?>>(qs);

        Assert.NotNull(result);
        Assert.Null(result.SomeParameter);
    }

    [Theory]
    [InlineData("?SomeParameter=invalid3")]
    [InlineData("SomeParameter=invalid3")]
    [InlineData("SomeParameter=3invalid")]
    [InlineData("SomeParameter=!3")]
    [InlineData("SomeParameter=3!")]
    [InlineData("SomeParameter=3.2")]
    [InlineData("SomeParameter=99999999999999999999")] // > int.MaxValue
    public void Deserialize_IgnoresInvalidIntValues_ForGenericIntClass(string queryString)
    {
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<int>>(queryString);

        Assert.NotNull(result);
        Assert.Equal(0, result.SomeParameter);
    }

    [Theory]
    [InlineData("?SomeParameter=invalid3")]
    [InlineData("SomeParameter=invalid3")]
    [InlineData("SomeParameter=3invalid")]
    [InlineData("SomeParameter=!3")]
    [InlineData("SomeParameter=3!")]
    [InlineData("SomeParameter=3.2")]
    [InlineData("SomeParameter=99999999999999999999")]
    public void Deserialize_IgnoresInvalidIntValues_ForNonGenericIntClass(string queryString)
    {
        var result = QueryStringSerializer.Deserialize<SomeClassWithIntParameter>(queryString);

        Assert.NotNull(result);
        Assert.Equal(0, result.SomeParameter);
    }

    [Theory]
    [InlineData("?SomeParameter=True", true)]
    [InlineData("SomeParameter=True", true)]
    [InlineData("SomeParameter=False", false)]
    [InlineData("", false)]
    public void Deserialize_CreatesCorrectObject_ForBooleans(string queryString, bool expected)
    {
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<bool>>(queryString);

        Assert.NotNull(result);
        Assert.Equal(expected, result.SomeParameter);
    }

    [Theory]
    [InlineData(true, "SomeParameter=True")]
    [InlineData(false, "SomeParameter=False")]
    public void Serialize_CreatesCorrectQueryString_ForBoolean(bool value, string expected)
    {
        var result = QueryStringSerializer.Serialize(new SomeGenericClassWithParameter<bool>(value));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("?SomeParameter=True", true)]
    [InlineData("SomeParameter=True", true)]
    [InlineData("SomeParameter=False", false)]
    [InlineData("", null)]
    public void Deserialize_CreatesCorrectObject_ForNullableBooleans(string queryString, bool? expected)
    {
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<bool?>>(queryString);
       
        Assert.NotNull(result);
        Assert.Equal(expected, result.SomeParameter);
    }

    [Theory]
    [InlineData(true, "SomeParameter=True")]
    [InlineData(false, "SomeParameter=False")]
    [InlineData(null, "")]
    public void Serialize_CreatesCorrectQueryString_ForNullableBoolean(bool? value, string expected)
    {
        var result = QueryStringSerializer.Serialize(new SomeGenericClassWithParameter<bool?>(value));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("?SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=0&SomeParameter=0&SomeParameter=0", 0, 0, 0)]
    [InlineData("SomeParameter=-1&SomeParameter=1", -1, 1)]
    [InlineData("SomeParameter=1", 1)]
    public void Deserialize_CreatesCorrectObject_ForLists(string queryString, params int[] expected)
    {
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<List<int>>>(queryString);

        Assert.NotNull(result);
        Assert.Equivalent(expected, result.SomeParameter);
    }

    [Theory]
    [InlineData("?SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=0&SomeParameter=0&SomeParameter=0", 0, 0, 0)]
    [InlineData("SomeParameter=-1&SomeParameter=1", -1, 1)]
    [InlineData("SomeParameter=1", 1)]
    public void Deserialize_CreatesCorrectObject_ForILists(string queryString, params int[] expected)
    {
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<IList<int>>>(queryString);

        Assert.NotNull(result);
        Assert.Equivalent(expected, result.SomeParameter);
    }

    [Theory]
    [InlineData("SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=0&SomeParameter=0&SomeParameter=0", 0, 0, 0)]
    [InlineData("SomeParameter=-1&SomeParameter=1", -1, 1)]
    [InlineData("SomeParameter=1", 1)]
    public void Serialize_CreatesCorrectQueryString_ForArrays(string expected, params int[] value)
    {
        var someClass = new SomeGenericClassWithParameter<int[]>(value);
        var actual = QueryStringSerializer.Serialize(someClass);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=0&SomeParameter=0&SomeParameter=0", 0, 0, 0)]
    [InlineData("SomeParameter=-1&SomeParameter=1", -1, 1)]
    [InlineData("SomeParameter=1", 1)]
    public void Serialize_CreatesCorrectQueryString_ForSystemArray(string expected, params int[] value)
    {
        var someClass = new SomeGenericClassWithParameter<Array>(value);
        var actual = QueryStringSerializer.Serialize(someClass);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=0&SomeParameter=0&SomeParameter=0", 0, 0, 0)]
    [InlineData("SomeParameter=-1&SomeParameter=1", -1, 1)]
    [InlineData("SomeParameter=1", 1)]
    public void Serialize_CreatesCorrectQueryString_ForICollection(string expected, params int[] value)
    {
        var someClass = new SomeGenericClassWithParameter<ICollection<int>>(value);
        var actual = QueryStringSerializer.Serialize(someClass);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("SomeParameter=1&SomeParameter=2&SomeParameter=3", 1, 2, 3)]
    [InlineData("SomeParameter=0&SomeParameter=0&SomeParameter=0", 0, 0, 0)]
    [InlineData("SomeParameter=-1&SomeParameter=1", -1, 1)]
    [InlineData("SomeParameter=1", 1)]
    public void Serialize_CreatesCorrectQueryString_ForLists(string expected, params int[] value)
    {
        var someClass = new SomeGenericClassWithParameter<List<int>>(value.ToList());
        var actual= QueryStringSerializer.Serialize(someClass);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("?SomeParameter.SomeParameter=1", 1)]
    [InlineData("SomeParameter.SomeParameter=1", 1)]
    [InlineData("SomeParameter.SomeParameter=-1", -1)]
    [InlineData("SomeParameter.SomeParameter=0", 0)]
    [InlineData("SomeParameter.SomeParameter=1234", 1234)]
    public void Deserialize_CreatesCorrectObject_ForNestedObjects(string queryString, int expected)
    {
        var result = QueryStringSerializer.Deserialize<SomeGenericClassWithParameter<SomeGenericClassWithParameter<int>>>(queryString);

        Assert.NotNull(result);
        Assert.NotNull(result.SomeParameter);
        Assert.Equal(expected, result.SomeParameter.SomeParameter);
    }

    [Theory]
    [InlineData("SomeParameter.SomeParameter=1", 1)]
    [InlineData("SomeParameter.SomeParameter=-1", -1)]
    [InlineData("SomeParameter.SomeParameter=0", 0)]
    [InlineData("SomeParameter.SomeParameter=1234", 1234)]
    public void Serialize_CreatesCorrectQueryString_ForNestedObjects(string expected, int value)
    {
        var someClass = new SomeGenericClassWithParameter<SomeClassWithIntParameter>(new SomeClassWithIntParameter() { SomeParameter = value });
        var actual = QueryStringSerializer.Serialize(someClass);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("SomeParameter.SomeString=hello,world!&SomeParameter.SomeParameter=1", 1, "hello,world!")]
    public void Serialize_CreatesCorrectQueryString_ForMoreNestedObjects(string expected, int intValue, string stringValue)
    {
        var someClassWithInt = new SomeGenericClassWithParameter<int>(intValue) { SomeString = stringValue };
        var someClass = new SomeGenericClassWithParameter<SomeGenericClassWithParameter<int>>(someClassWithInt);
        var actual = QueryStringSerializer.Serialize(someClass);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("?SomeString=Anything", "Anything")]
    [InlineData("SomeString=Anything", "Anything")]
    public void Deserialize_CreatesCorrectObject_ForRecords(string queryString, string expected)
    {
        var result = QueryStringSerializer.Deserialize<SomeRecord>(queryString);

        Assert.NotNull(result);
        Assert.Equal(expected, result.SomeString);
    }

    [Theory]
    [InlineData("?SomeString=Anything", "Anything")]
    [InlineData("SomeString=Anything", "Anything")]
    public void Deserialize_CreatesCorrectObject_ForStructs(string queryString, string expected)
    {
        var result = QueryStringSerializer.Deserialize<SomeStruct>(queryString);

        Assert.Equal(expected, result.SomeString);
    }

    [Theory]
    [InlineData("?SomeString=Anything", "Anything")]
    [InlineData("SomeString=Anything", "Anything")]
    public void Deserialize_CreatesCorrectObject_ForReadonlyStructs(string queryString, string expected)
    {
        var result = QueryStringSerializer.Deserialize<SomeReadonlyStruct>(queryString);

        Assert.Equal(expected, result.SomeString);
    }

    [Theory]
    [InlineData(SomeEnum.BigBanana, "SomeParameter=BigBanana")]
    public void Serialize_SerializesEnumAsString_WithDefaultOptions(SomeEnum enumValue, string expected)
    {
        var someClass = new SomeGenericClassWithParameter<SomeEnum>(enumValue);
        var actual = QueryStringSerializer.Serialize(someClass);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Serialize_SerializesEnumAsInt_WhenOptionsHaveEnumAsStringAsFalse()
    {
        var options = new QueryStringSerializerOptions { EnumsAsStrings = false };
        var someClass = new SomeGenericClassWithParameter<SomeEnum>(SomeEnum.BigBanana);

        var expected = $"SomeParameter={(int)SomeEnum.BigBanana}";
        var actual = QueryStringSerializer.Serialize(someClass, options);

        Assert.Equal(expected, actual);
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

        Assert.NotNull(result?.SomeParameter);
        Assert.Equal(someClass.SomeParameter.SomeParameter, result.SomeParameter.SomeParameter);
    }
}
