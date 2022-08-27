using Backlang.Codeanalysis.Parsing.AST;
using Loyc.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Declarations;

[TestClass]
public class UnitTests : ParserTestBase
{
    [TestMethod]
    public void Declaration_Should_Pass()
    {
        var src = "unit liter;";
        var tree = ParseAndGetNode(src);

        Assert.AreEqual(0, tree.errors.Count);
        Assert.IsTrue(tree.nodes.Calls(Symbols.UnitDecl));
        Assert.AreEqual(tree.nodes.Args[0], LNode.Id("liter"));
    }
}