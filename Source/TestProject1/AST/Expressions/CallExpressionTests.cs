using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Expressions;
using Backlang.Codeanalysis.Parsing.AST.Statements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Expressions;

[TestClass]
public class CallExpressionTests : ParserTestBase
{
    [TestMethod]
    public void Call_With_Array_Index_Argument_Should_Pass()
    {
        var src = "print(arr[1]);";
        var tree = ParseAndGetNodeInFunction<ExpressionStatement>(src);
        var node = (CallExpression)tree.Expression;

        Assert.AreEqual(node.Arguments.Count, 1);
        Assert.IsInstanceOfType(node.Name, typeof(NameExpression));
        Assert.AreEqual("print", ((NameExpression)node.Name).Name);
        Assert.IsInstanceOfType(node.Arguments[0], typeof(ArrayAccessExpression));

        var arrAccess = (ArrayAccessExpression)node.Arguments[0];

        Assert.AreEqual(arrAccess.Name.Name, "arr");
        Assert.AreEqual(arrAccess.Indices.Count, 1);
    }

    [TestMethod]
    public void Call_With_One_Argument_Should_Pass()
    {
        var src = "print(true);";
        var tree = ParseAndGetNodeInFunction<ExpressionStatement>(src);
        var node = (CallExpression)tree.Expression;

        Assert.AreEqual(node.Arguments.Count, 1);
        Assert.IsInstanceOfType(node.Name, typeof(NameExpression));
        Assert.AreEqual("print", ((NameExpression)node.Name).Name);
    }

    [TestMethod]
    public void Call_With_Two_Arguments_Should_Pass()
    {
        var src = "print(1, true);";
        var tree = ParseAndGetNodeInFunction<ExpressionStatement>(src);
        var node = (CallExpression)tree.Expression;

        Assert.AreEqual(node.Arguments.Count, 2);
        Assert.IsInstanceOfType(node.Name, typeof(NameExpression));
        Assert.AreEqual("print", ((NameExpression)node.Name).Name);
    }

    [TestMethod]
    public void Call_Without_Arguments_Should_Pass()
    {
        var src = "print();";
        var tree = ParseAndGetNodeInFunction<ExpressionStatement>(src);
        var node = (CallExpression)tree.Expression;

        Assert.AreEqual(node.Arguments.Count, 0);
        Assert.IsInstanceOfType(node.Name, typeof(NameExpression));
        Assert.AreEqual("print", ((NameExpression)node.Name).Name);
    }
}