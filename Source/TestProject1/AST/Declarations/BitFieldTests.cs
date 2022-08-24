using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Declarations;

[TestClass]
public class BitFieldTests : ParserTestBase
{
    [TestMethod]
    public void Simple_BitField_Should_Pass()
    {
        var src = "bitfield Flags { DivideByZero = 0, IsNull = 1 }";
        var result = ParseAndGetNode(src);
        var node = result.nodes;

        Assert.AreEqual(0, result.errors.Count);

        Assert.AreEqual("Flags", node.Args[0].Name.Name);

        Assert.AreEqual("DivideByZero", node.Args[1].Args[0].Args[0].Name.Name);
        Assert.AreEqual(0, node.Args[1].Args[0].Args[1].Args[0].Value);

        Assert.AreEqual("IsNull", node.Args[1].Args[1].Args[0].Name.Name);
        Assert.AreEqual(1, node.Args[1].Args[1].Args[1].Args[0].Value);
    }

    [TestMethod]
    public void BitField_Trailling_Comma_Should_Fail()
    {
        var src = "bitfield Flags { DivideByZero = 0, IsNull = 1, }";
        var result = ParseAndGetNode(src);
        var node = result.nodes;

        Assert.AreEqual(1, result.errors.Count);
    }

    [TestMethod]
    public void BitField_With_Expression_Should_Fail()
    {
        var src = "bitfield Flags { DivideByZero = 0 + 1, IsNull = 1 }";
        var result = ParseAndGetNode(src);
        var node = result.nodes;

        Assert.AreEqual(1, result.errors.Count);
    }
}