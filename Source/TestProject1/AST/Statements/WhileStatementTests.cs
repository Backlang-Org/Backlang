using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Statements;

[TestClass]
public class WhileStatementTests : ParserTestBase
{
    [TestMethod]
    public void While_With_If_Should_Pass()
    {
        var src = "while a < b && c { if !d { 42; } else { 1; } }";
        var tree = ParseAndGetNodesInFunction(src);
    }
}