using Backlang.Codeanalysis.Parsing.AST.Declarations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1;

[TestClass]
public class StructTests : ParserTestBase
{
    [TestMethod]
    public void Simple_Struct_Should_Pass()
    {
        var src = "struct Point { X : i32; Y : i32; }";
        var declaration = ParseAndGetNode<StructDeclaration>(src);

        Assert.AreEqual(declaration.Name, "Point");
        Assert.AreEqual(declaration.Members.Count, 2);
    }
}