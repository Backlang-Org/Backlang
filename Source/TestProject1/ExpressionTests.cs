using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Expressions;
using Backlang.Codeanalysis.Parsing.AST.Statements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1;

[TestClass]
public class ExpressionTests : ParserTestBase
{
    [TestMethod]
    public void Empty_InitializerList_Should_Pass()
    {
        var src = "[];";
        var tree = ParseAndGetNodeInFunction<ExpressionStatement>(src);

        Assert.IsInstanceOfType(tree.Expression, typeof(InitializerListExpression));

        var list = (InitializerListExpression)tree.Expression;

        Assert.AreEqual(list.Elements.Count, 0);
    }

    [TestMethod]
    public void InitializerList_With_Element_Should_Pass()
    {
        var src = "[12];";
        var tree = ParseAndGetNodeInFunction<ExpressionStatement>(src);

        Assert.IsInstanceOfType(tree.Expression, typeof(InitializerListExpression));

        var list = (InitializerListExpression)tree.Expression;

        Assert.AreEqual(list.Elements.Count, 1);
        Assert.AreEqual(((LiteralNode)list.Elements[0]).Value, 12L);
    }

    [TestMethod]
    public void InitializerList_With_Elements_Should_Pass()
    {
        var src = "[1, 2];";
        var tree = ParseAndGetNodeInFunction<ExpressionStatement>(src);

        Assert.IsInstanceOfType(tree.Expression, typeof(InitializerListExpression));

        var list = (InitializerListExpression)tree.Expression;

        Assert.AreEqual(list.Elements.Count, 2);
        Assert.AreEqual(((LiteralNode)list.Elements[0]).Value, 1L);
        Assert.AreEqual(((LiteralNode)list.Elements[1]).Value, 2L);
    }

    [TestMethod]
    public void InitializerList_With_InitialierList_Element_Should_Pass()
    {
        var src = "[[]];";
        var tree = ParseAndGetNodeInFunction<ExpressionStatement>(src);

        Assert.IsInstanceOfType(tree.Expression, typeof(InitializerListExpression));

        var list = (InitializerListExpression)tree.Expression;

        Assert.AreEqual(list.Elements.Count, 1);
        Assert.AreEqual(((InitializerListExpression)list.Elements[0]).Elements.Count, 0);
    }

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