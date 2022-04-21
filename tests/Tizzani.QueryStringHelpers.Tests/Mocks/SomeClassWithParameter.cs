namespace Tizzani.QueryStringHelpers.Tests.Mocks;

internal class SomeClassWithParameter<T>
{
    public T? SomeParameter { get; set; }

    public SomeClassWithParameter()
    {
    }

    public SomeClassWithParameter(T? value)
    {
        SomeParameter = value;
    }
}
