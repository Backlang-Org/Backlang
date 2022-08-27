using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Statements;

[TestClass]
public class IfStatementTests : ParserTestBase
{
    [TestMethod]
    public void If_Else_In_If_Else_Should_Pass()
    {
        var src = "if a < b && c { if !d { 42; } /* else { 1; } */ } else { 3; }";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void If_Else_Should_Pass()
    {
        var src = "if true { none; } else { 42; }";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void If_In_If_Else_Should_Pass()
    {
        var src = "if a < b && c { if !d { 42; } else { 1; } }";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void If_In_If_Should_Pass()
    {
        var src = "if a < b && c { if !d { 42; } }";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void If_With_Complex_Condition_Without_Else_Should_Pass()
    {
        var src = "if a < b && c { none; }";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void If_Without_Else_Should_Pass()
    {
        var src = "if true { none; }";
        var result = ParseAndGetNodesInFunction(src);

        Assert.AreEqual(0, result.errors.Count);
    }
}