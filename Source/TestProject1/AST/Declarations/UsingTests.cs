using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Declarations;

[TestClass]
public class UsingTests : ParserTestBase
{
    [TestMethod]
    public void IntUsing_Should_Pass()
    {
        var src = "using i32 as int;";
        var result = ParseAndGetNode(src);
        var node = result.nodes;

        Assert.AreEqual(0, result.errors.Count);

        Assert.AreEqual("i32", node.Args[0].Name.Name);
        Assert.AreEqual("int", node.Args[1].Name.Name);
    }
}