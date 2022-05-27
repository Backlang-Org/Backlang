namespace Backlang.Core
{
    public class Optional<T>
    {
        private readonly T? _value;

        public Optional(T value)
        {
            _value = value;
        }

        public static implicit operator bool(Optional<T> value)
        {
            return value._value != null;
        }

        public static implicit operator T(Optional<T> value)
        {
            return value._value;
        }
    }
}