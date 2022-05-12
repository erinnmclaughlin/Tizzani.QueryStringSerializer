namespace Tizzani.QueryStringSerializer.Tests.Mocks;

internal class SomeGenericClassWithParameter<T>
{
    public string? SomeString { get; set; }
    public T? SomeParameter { get; set; }

    public SomeGenericClassWithParameter()
    {
    }

    public SomeGenericClassWithParameter(T? value)
    {
        SomeParameter = value;
    }
}

internal class SomeClassWithEnumParameter
{
    public SomeEnum SomeParameter { get; set; }
}

internal class SomeClassWithStringParameter
{
    public string SomeParameter { get; set; } = null!;
}

internal class SomeClassWithIntParameter
{
    public int SomeParameter { get; set; }
}