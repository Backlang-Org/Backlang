using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Statements;

[TestClass]
public class IfStatementTests : ParserTestBase
{
    [TestMethod]
    public void If_Else_In_If_Else_Should_Pass()
    {
        var src = "if a < b && c { if !d { 42; } /* else { 1; } */ } else { 3; }";
        var tree = ParseAndGetNodesInFunction(src);
    }

    [TestMethod]
    public void If_Else_Should_Pass()
    {
        var src = "if true { none; } else { 42; }";
        var tree = ParseAndGetNodesInFunction(src);
    }

    [TestMethod]
    public void If_In_If_Else_Should_Pass()
    {
        var src = "if a < b && c { if !d { 42; } else { 1; } }";
        var tree = ParseAndGetNodesInFunction(src);
    }

    [TestMethod]
    public void If_In_If_Should_Pass()
    {
        var src = "if a < b && c { if !d { 42; } }";
        var tree = ParseAndGetNodesInFunction(src);
    }

    [TestMethod]
    public void If_With_Complex_Condition_Without_Else_Should_Pass()
    {
        var src = "if a < b && c { none; }";
        var tree = ParseAndGetNodesInFunction(src);
    }

    [TestMethod]
    public void If_Without_Else_Should_Pass()
    {
        var src = "if true { none; }";
        var tree = ParseAndGetNodesInFunction(src);
    }
}