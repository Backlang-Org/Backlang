using Backlang.Codeanalysis.Parsing.AST.Declarations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestProject1;

namespace TestProject1.AST.Declarations;

[TestClass]
public class TypeAliasTests : ParserTestBase
{
    [TestMethod]
    public void IntAlias_Should_Pass()
    {
        var src = "type int = i32;";
        var node = ParseAndGetNode<TypeAliasDeclaration>(src);

        Assert.AreEqual(node.AliasName, "int");
        Assert.AreEqual(node.ToAlias, "i32");
    }
}
