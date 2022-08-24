using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Expressions;

[TestClass]
public class TupleTests : ParserTestBase
{
    [TestMethod]
    public void SimpleTuple_Should_Pass()
    {
        var src = "(1, 2);";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void TrailingComma_Should_Fail()
    {
        var src = "(1,2,);";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(1, result.errors.Count);
    }
}