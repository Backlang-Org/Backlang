using Backlang.Codeanalysis.Parsing.AST.Expressions;
using Backlang.Codeanalysis.Parsing.AST.Statements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1;

[TestClass]
public class ExpressionTests : ParserTestBase
{
    [TestMethod]
    public void None_Should_Pass()
    {
        var src = "none;";
        var tree = ParseAndGetNodeInFunction<ExpressionStatement>(src);

        Assert.IsInstanceOfType(tree.Expression, typeof(NoneExpression));
    }

    [TestMethod]
    public void SizeOf_Should_Pass()
    {
        var src = "sizeof<i32>;";
        var tree = ParseAndGetNodeInFunction<ExpressionStatement>(src);
        var expression = (SizeOfExpression)tree.Expression;

        Assert.AreEqual(expression.Type.Typename, "i32");
    }
}