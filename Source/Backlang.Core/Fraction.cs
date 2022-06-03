namespace Backlang.Core;

//ToDo: don't divide by zero
public struct Fraction : IComparable<Fraction>
{
    public static Fraction MaxValue = new Fraction(short.MaxValue, 1);
    public static Fraction MinValue = new Fraction(1, short.MaxValue);

    private short _denominator;
    private short _numerator;

    public Fraction(Half value) : this((double)value)
    {
    }

    public Fraction(float value) : this((double)value)
    {
    }

    public Fraction(double value)
    {
        short i = 0;
        while (value % 1 != 0)
        {
            value *= 10;
            i++;
        }
        _numerator = (short)value;
        _denominator = (short)(i * 10);
    }

    public Fraction(short numerator, short denominator)
    {
        _numerator = numerator;
        _denominator = denominator;
    }

    public static Fraction One => new Fraction(1, 1);

    public static Fraction Zero => new Fraction(0, 1);

    public static Fraction Abs(Fraction value)
    {
        throw new NotImplementedException();
    }

    public static Fraction Max(Fraction x, Fraction y)
    {
        throw new NotImplementedException();
    }

    public static Fraction Min(Fraction x, Fraction y)
    {
        throw new NotImplementedException();
    }

    public static Fraction operator -(Fraction value)
    {
        throw new NotImplementedException();
    }

    public static Fraction operator -(Fraction left, Fraction right)
    {
        throw new NotImplementedException();
    }

    public static Fraction operator --(Fraction value)
    {
        throw new NotImplementedException();
    }

    public static bool operator !=(Fraction left, Fraction right)
    {
        return left.CompareTo(right) != 0;
    }

    public static Fraction operator %(Fraction left, Fraction right)
    {
        throw new NotImplementedException();
    }

    public static Fraction operator *(Fraction left, Fraction right)
    {
        throw new NotImplementedException();
    }

    public static Fraction operator /(Fraction left, Fraction right)
    {
        throw new NotImplementedException();
    }

    public static Fraction operator +(Fraction value)
    {
        throw new NotImplementedException();
    }

    public static Fraction operator +(Fraction left, Fraction right)
    {
        throw new NotImplementedException();
    }

    public static Fraction operator ++(Fraction value)
    {
        throw new NotImplementedException();
    }

    public static bool operator <(Fraction left, Fraction right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(Fraction left, Fraction right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator ==(Fraction left, Fraction right)
    {
        return left.CompareTo(right) == 0;
    }

    public static bool operator >(Fraction left, Fraction right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(Fraction left, Fraction right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static Fraction Parse(string value)
    {
        throw new NotImplementedException();
    }

    public int CompareTo(Fraction other)
    {
        return GetHashCode().CompareTo(other.GetHashCode());
    }

    public int CompareTo(object? obj)
    {
        return GetHashCode().CompareTo(obj?.GetHashCode());
    }

    public bool Equals(Fraction other)
    {
        return other.GetHashCode() == GetHashCode();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_numerator, _denominator);
    }

    public override string ToString()
    {
        return $"{_numerator} \\\\ {_denominator}";
    }
}