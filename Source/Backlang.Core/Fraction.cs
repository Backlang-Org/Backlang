namespace Backlang.Core;

//ToDo: don't divide by zero
public struct Fraction : IComparable<Fraction>
{
    public static Fraction MaxValue = new Fraction(ushort.MaxValue, 1);
    public static Fraction MinValue = new Fraction(1, ushort.MaxValue, true);

    private ushort _denominator;
    private bool _negative;
    private ushort _numerator;

    public Fraction(Half value) : this((double)value)
    {
    }

    public Fraction(float value) : this((double)value)
    {
    }

    public Fraction(double value)
    {
        ushort i = 0;
        if (value < 0)
        {
            value *= -1;
            _negative = true;
        }
        else
        {
            _negative = false;
        }
        while (value % 1 != 0)
        {
            value *= 10;
            i++;
        }
        _numerator = (ushort)value;
        _denominator = (ushort)(i * 10);
    }

    public Fraction(ushort numerator, ushort denominator, bool negative = false)
    {
        _numerator = numerator;
        _denominator = denominator;
        _negative = negative;
    }

    public Fraction(short numerator, short denominator)
    {
        if ((numerator < 0 && denominator >= 0) || (denominator < 0 && numerator >= 0))
        {
            // only one of them is negative
            _negative = true;
        }
        else
        {
            _negative = false;
        }
        _numerator = (ushort)Math.Abs(numerator);
        _denominator = (ushort)Math.Abs(denominator);
    }

    public static Fraction One => new Fraction(1, 1);

    public static Fraction Zero => new Fraction(0, 1);

    public static Fraction Abs(Fraction value)
    {
        throw new NotImplementedException();
    }

    public static implicit operator double(Fraction value)
    {
        return value._numerator / value._denominator;
    }

    public static implicit operator float(Fraction value)
    {
        return (float)value._numerator / value._denominator;
    }

    public static implicit operator Fraction(Half value)
    {
        return new Fraction(value);
    }

    public static implicit operator Fraction(float value)
    {
        return new Fraction(value);
    }

    public static implicit operator Fraction(double value)
    {
        return new Fraction(value);
    }

    public static implicit operator Half(Fraction value)
    {
        return (Half)(value._numerator / value._denominator);
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
        return new Fraction((ushort)(left._numerator * right._numerator), (ushort)(left._denominator * right._denominator));
    }

    public static Fraction operator /(Fraction left, Fraction right)
    {
        return new Fraction((ushort)(left._numerator * right._denominator), (ushort)(left._denominator * right._numerator));
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
        return new Fraction((ushort)(value._numerator + 1), value._denominator);
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
        if (!value.Contains(@"\\")) throw new FormatException("Fraction has wrong format");

        var parts = value.Split(@"\\");
        var numerator = short.Parse(parts[0]);
        var denominator = short.Parse(parts[1]);

        return new Fraction(numerator, denominator);
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
        return HashCode.Combine(_negative, _numerator, _denominator);
    }

    public override string ToString()
    {
        return $"{(_negative ? "-" : "")}{_numerator} \\\\ {_denominator}";
    }
}