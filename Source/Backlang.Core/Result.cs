using System.Runtime.CompilerServices;

namespace Backlang.Core
{
    public class Result<T>
    {
        private readonly T? _value;

        public Result(T value)
        {
            _value = value;
        }

        public static implicit operator T(Result<T> value)
        {
            return value._value;
        }

        //Unpacking operator
        [SpecialName]
        public static bool op_Unpacking(Result<T> value)
        {
            return value._value != null;
        }
    }
}