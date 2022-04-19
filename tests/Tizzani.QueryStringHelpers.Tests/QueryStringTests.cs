using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tizzani.QueryStringHelpers.Tests;

public class QueryStringTests
{
    [Theory]
    [InlineData(null)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void FromObject_CreatesCorrectParameters_ForIntegers(int? someInt)
    {
        var someClassWithInt = new SomeClassWithParameter<int?>(someInt);
        var key = nameof(someClassWithInt.SomeParameter);

        var queryString = QueryString.FromObject(someClassWithInt);
        var action = () => queryString[key];

        if (someInt.HasValue)
        {
            queryString.ContainsKey(key).Should().BeTrue();
            action.Invoke().Should().Be(someInt?.ToString());
        }
        else
        {
            queryString.ContainsKey(key).Should().BeFalse();
            action.Should().Throw<KeyNotFoundException>();
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\t  ")]
    [InlineData("hello")]
    [InlineData("hello, world!")]
    public void FromObject_CreatesCorrectParameters_ForStrings(string? someString)
    {
        var someClassWithString = new SomeClassWithParameter<string?>(someString);
        var key = nameof(someClassWithString.SomeParameter); 
        
        var queryString = QueryString.FromObject(someClassWithString);

        var action = () => queryString[key];
        
        if (!string.IsNullOrWhiteSpace(someString))
        {
            queryString.ContainsKey(key).Should().BeTrue();
            action.Invoke().Should().Be(someString);
        }
        else
        {
            queryString.ContainsKey(key).Should().BeFalse();
            action.Should().Throw<KeyNotFoundException>();
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData(null, null)]
    [InlineData(null, "  ", "\t\n")]
    [InlineData("hello", "world", "12")]
    [InlineData(null, "hello")]
    public void FromObject_CreatesCorrectParameters_ForCollections(params string[]? someCollection)
    {
        var someClassWithCollection = new SomeClassWithParameter<string[]?>(someCollection);
        var key = nameof(someClassWithCollection.SomeParameter);

        var queryString = QueryString.FromObject(someClassWithCollection);

        var action = () => queryString[key];

        var expectedCollection = someCollection?.Where(c => !string.IsNullOrWhiteSpace(c));

        if (expectedCollection?.Any() == true)
        {
            queryString.ContainsKey(key).Should().BeTrue();
            action.Invoke().Should().Be(string.Join(",", expectedCollection));
        }
        else
        {
            queryString.ContainsKey(key).Should().BeFalse();
            action.Should().Throw<KeyNotFoundException>();
        }
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
