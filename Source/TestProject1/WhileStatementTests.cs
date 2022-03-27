using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Statements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1;

[TestClass]
public class WhileStatementTests : ParserTestBase
{
    [TestMethod]
    public void While_With_If_Should_Pass()
    {
        var src = "while a < b && c { if !d { 42; } else { 1; } }";
        var tree = ParseAndGetNodeInFunction<WhileStatement>(src);

        Assert.IsInstanceOfType(tree.Condition, typeof(LiteralNode));
    }
}