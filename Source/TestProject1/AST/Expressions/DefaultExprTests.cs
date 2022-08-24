using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Expressions;

[TestClass]
public class DefaultExprTests : ParserTestBase
{
    [TestMethod]
    public void Default_With_Type_Should_Pass()
    {
        var src = "default(i32);";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void Default_Without_Type_Should_Pass()
    {
        var src = "default;";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }
}