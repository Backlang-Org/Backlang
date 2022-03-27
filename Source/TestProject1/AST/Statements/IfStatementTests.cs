using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Statements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestProject1;

namespace TestProject1.AST.Statements;

[TestClass]
public class IfStatementTests : ParserTestBase
{
    [TestMethod]
    public void If_Else_In_If_Else_Should_Pass()
    {
        var src = "if a < b && c { if !d { 42; } else { 1; } } else { 3; }";
        var tree = ParseAndGetNodeInFunction<IfStatement>(src);
    }

    [TestMethod]
    public void If_Else_Should_Pass()
    {
        var src = "if true { none; } else { 42; }";
        var tree = ParseAndGetNodeInFunction<IfStatement>(src);

        Assert.IsNotNull(tree.ElseBody);
    }

    [TestMethod]
    public void If_In_If_Else_Should_Pass()
    {
        var src = "if a < b && c { if !d { 42; } else { 1; } }";
        var tree = ParseAndGetNodeInFunction<IfStatement>(src);
    }

    [TestMethod]
    public void If_In_If_Should_Pass()
    {
        var src = "if a < b && c { if !d { 42; } }";
        var tree = ParseAndGetNodeInFunction<IfStatement>(src);
    }

    [TestMethod]
    public void If_With_Complex_Condition_Without_Else_Should_Pass()
    {
        var src = "if a < b && c { none; }";
        var tree = ParseAndGetNodeInFunction<IfStatement>(src);
    }

    [TestMethod]
    public void If_Without_Else_Should_Pass()
    {
        var src = "if true { none; }";
        var tree = ParseAndGetNodeInFunction<IfStatement>(src);

        Assert.IsInstanceOfType(tree.Condition, typeof(LiteralNode));
    }
}
