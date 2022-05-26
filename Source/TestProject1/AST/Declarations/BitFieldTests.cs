using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Declarations;

[TestClass]
public class BitFieldTests : ParserTestBase
{
    [TestMethod]
    public void Simple_BitField_Should_Pass()
    {
        var src = "bitfield Flags { DivideByZero = 0, IsNull = 1 }";
        var node = ParseAndGetNodes(src);
    }
}