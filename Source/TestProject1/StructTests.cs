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

    [TestMethod]
    public void Struct_With_Values_Should_Pass()
    {
        var src = "struct Point { X : i32 = 24; Y : i32 = 42; }";
        var declaration = ParseAndGetNode<StructDeclaration>(src);

        Assert.AreEqual(declaration.Name, "Point");
        Assert.AreEqual(declaration.Members.Count, 2);

        Assert.AreEqual(declaration.Members[0].Name, "X");
        Assert.AreEqual(declaration.Members[1].Name, "Y");
    }
}