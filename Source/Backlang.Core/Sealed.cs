namespace Backlang.Core;

public struct Sealed<T>
{
    private T _value;

    public bool IsFreezed { get; private set; }

    public static implicit operator Sealed<T>(T value)
    {
        return new Sealed<T> { _value = value, IsFreezed = true };
    }

    public static implicit operator T(Sealed<T> @sealed)
    {
        return @sealed._value;
    }

    public void Set(T value)
    {
        if (IsFreezed)
        {
            throw new InvalidOperationException("Object is freezed");
        }
        _value = value;
    }

    public void Unfreeze()
    {
        IsFreezed = true;
    }
}