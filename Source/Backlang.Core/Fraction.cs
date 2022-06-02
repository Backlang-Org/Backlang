namespace Backlang.Core;

public struct Fraction
{
    private short _numerator;
    private short _denominator;

    public Fraction(Half value) : this((double)value) { }

    public Fraction(float value) : this((double)value) { }

    public Fraction(double value)
    {
        short i = 0;
        while(value % 1 != 0)
        {
            value *= 10;
            i++;
        }
        _numerator = (short)value;
        _denominator = (short) (i * 10);
    }

    public Fraction(short numerator, short denominator)
    {
        _numerator = numerator;
        _denominator = denominator;
    }
}
