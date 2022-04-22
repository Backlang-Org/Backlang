using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Declarations;

[TestClass]
public class TypeAliasTests : ParserTestBase
{
    [TestMethod]
    public void IntAlias_Should_Pass()
    {
        var src = "type int = i32;";
        var node = ParseAndGetNodes(src);

        Assert.AreEqual(node.AliasName, "int");
        Assert.AreEqual(node.ToAlias, "i32");
    }
}