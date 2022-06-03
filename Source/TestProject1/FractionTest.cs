using Backlang.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1;

[TestClass]
public class FractionTest
{
    [TestMethod]
    public void CreateFractionOfDouble()
    {
        var value = 1.2;
        var fraction = new Fraction(value);

        Assert.AreEqual(new Fraction(12, 10), fraction);
    }

    [TestMethod]
    public void CreateNegativeFraction()
    {
        var fraction = new Fraction(-2, 3);

        Assert.AreEqual(new Fraction(2, 3, true), fraction);
    }
}