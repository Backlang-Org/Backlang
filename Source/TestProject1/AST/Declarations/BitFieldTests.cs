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

        Assert.AreEqual(node.Name, "Flags");
        Assert.AreEqual(node.Members.Count, 2);

        Assert.AreEqual(node.Members[0].Name, "DivideByZero");
        Assert.AreEqual(((LiteralNode)node.Members[0].Index).Value, 0L);

        Assert.AreEqual(node.Members[1].Name, "IsNull");
        Assert.AreEqual(((LiteralNode)node.Members[1].Index).Value, 1L);
    }
}