using Backlang.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1;

[TestClass]
public class FractionTest
{
    [TestMethod]
    public void Create_Fraction_Of_Double_Should_Pass()
    {
        var value = 1.2;
        var fraction = new Fraction(value);

        Assert.AreEqual(new Fraction(12, 10), fraction);
    }

    [TestMethod]
    public void Create_Negative_Fraction_Should_Pass()
    {
        var fraction = new Fraction(-2, 3);

        Assert.AreEqual(new Fraction(2, 3, true), fraction);
    }

    [TestMethod]
    public void Fraction_Division_Should_Pass()
    {
        var fraction = new Fraction(1, 4);

        Assert.AreEqual(new Fraction(2, 4), fraction / new Fraction(1, 2));
    }

    [TestMethod]
    public void Fraction_Muliply_Should_Pass()
    {
        var fraction = new Fraction(1, 2);

        Assert.AreEqual(new Fraction(1, 4), fraction * fraction);
    }

    [TestMethod]
    public void Fraction_Parse_Negative_Should_Pass()
    {
        Assert.AreEqual(new Fraction(2, 4, true), Fraction.Parse(@"-2 \\ 4"));
    }

    [TestMethod]
    public void Fraction_Parse_Should_Pass()
    {
        Assert.AreEqual(new Fraction(2, 4), Fraction.Parse(@"2 \\ 4"));
    }

    [TestMethod]
    public void Fraction_Parse_Both_Negative_Should_Pass()
    {
        Assert.AreEqual(new Fraction(2, 4, false), Fraction.Parse(@"-2 \\ -4"));
    }

    [TestMethod]
    public void Fraction_From_Negative_To_Positive_Should_Pass()
    {
        var frac = new Fraction(-1, 2);
        frac++;
        Assert.AreEqual(new Fraction(0, 2, false), frac);
    }

    [TestMethod]
    public void Fraction_From_Positive_To_Negative_Should_Pass()
    {
        var frac = new Fraction(0, 2);
        frac--;
        Assert.AreEqual(new Fraction(-1, 2), frac);
    }

    [TestMethod]
    public void Fraction_Get_Full_Negative_Numerator_Should_Pass()
    {
        var frac = new Fraction(-2, 4);
        Assert.AreEqual(-2, frac.Numerator);
    }

    [TestMethod]
    public void Fraction_Get_Raw_Negative_Numerator_Should_Pass()
    {
        var frac = new Fraction(-2, 4);
        Assert.AreEqual(2, frac.RawNumerator);
    }
}