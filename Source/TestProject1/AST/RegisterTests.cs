using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Declarations;
using Backlang.Codeanalysis.Parsing.AST.Expressions;
using Backlang.Codeanalysis.Parsing.AST.Statements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST;

[TestClass]
public class RegisterTests : ParserTestBase
{
    [TestMethod]
    public void DeclareRegister_Should_Pass()
    {
        var src = "register EAX = 1;";
        var tree = ParseAndGetNode<RegisterDeclaration>(src);

        Assert.AreEqual(tree.Name, "EAX");
        Assert.AreEqual(((LiteralNode)tree.Address).Value, 1L);
    }

    [TestMethod]
    public void UseRegister_Should_Pass()
    {
        var src = "#eax;";
        var tree = ParseAndGetNodeInFunction<ExpressionStatement>(src);
        var expression = (UnaryExpression)tree.Expression;

        Assert.AreEqual(expression.OperatorToken.Text, "#");
        Assert.AreEqual(((NameExpression)expression.Expression).Name, "eax");
    }

    [TestMethod]
    public void UseRegister_With_Assignment_Should_Pass()
    {
        var src = "something = #eax;";
        var tree = ParseAndGetNodeInFunction<ExpressionStatement>(src);
        var expression = (BinaryExpression)tree.Expression;

        Assert.AreEqual(expression.OperatorToken.Text, "=");
        Assert.AreEqual(((UnaryExpression)expression.Right).OperatorToken.Text, "#");
    }
}