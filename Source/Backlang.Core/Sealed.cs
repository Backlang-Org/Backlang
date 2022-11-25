using System.Runtime.CompilerServices;

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

    public void Freeze()
    {
        IsFreezed = true;
    }

    public void Unfreeze()
    {
        IsFreezed = false;
    }

    //Unpacking operator
    [SpecialName]
    public static T op_Unpacking(Sealed<T> value)
    {
        return value._value;
    }
}
