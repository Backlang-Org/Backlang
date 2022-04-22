using Backlang.Codeanalysis.Parsing.AST.Expressions.Match;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Expressions;

[TestClass]
public class MatchTests : ParserTestBase
{
    [TestMethod]
    public void All_Rules_Should_Pass()
    {
        var src = "match input with 12 => 1, i32 => 32, i32 num => num + 2, _ => 3, > 12 => 15;";
        var tree = ParseAndGetNodesInFunction(src);
        var matchExpression = (MatchExpression)tree.Expression;

        Assert.AreEqual(matchExpression.Rules.Count, 5);
    }
}