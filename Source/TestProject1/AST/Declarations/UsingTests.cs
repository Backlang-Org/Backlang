using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Declarations;

[TestClass]
public class UsingTests : ParserTestBase
{
    [TestMethod]
    public void IntUsing_Should_Pass()
    {
        var src = "using i32 as int;";
        var node = ParseAndGetNode(src).nodes;

        Assert.AreEqual("i32", node.Args[0].Name.Name);
        Assert.AreEqual("int", node.Args[1].Name.Name);
    }
}