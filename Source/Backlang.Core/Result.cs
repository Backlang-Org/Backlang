namespace Backlang.Core
{
    public class Result<T>
    {
        private readonly T? _value;

        public Result(T value)
        {
            _value = value;
        }

        public static implicit operator bool(Result<T> value)
        {
            return value._value != null;
        }

        public static implicit operator T(Result<T> value)
        {
            return value._value;
        }
    }
}