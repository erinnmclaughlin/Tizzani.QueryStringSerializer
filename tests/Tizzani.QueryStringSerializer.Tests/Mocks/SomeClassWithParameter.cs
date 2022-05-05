namespace Tizzani.QueryStringSerializer.Tests.Mocks;

internal class SomeClassWithParameter<T>
{
    public string? SomeString { get; set; }
    public T? SomeParameter { get; set; }

    public SomeClassWithParameter()
    {
    }

    public SomeClassWithParameter(T? value)
    {
        SomeParameter = value;
    }
}
