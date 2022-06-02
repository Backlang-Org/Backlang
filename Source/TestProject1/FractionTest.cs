using Backlang.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1;

[Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
public class FractionTest
{
    [TestMethod]
    public void CreateFractionOfDouble()
    {
        var value = 1.2;
        var fraction = new Fraction(value);
        Assert.Equals(new Fraction(12, 10), fraction);
    }
}