using Backlang.Codeanalysis.Parsing.AST.Declarations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1;

[TestClass]
public class EnumTests : ParserTestBase
{
    [TestMethod]
    public void Enum_With_Int_Values_Should_Pass()
    {
        var src = "enum Colors { White = 0, Red = 1, Green = 2, Blue = 3, Black = 4 }";
        var declaration = ParseAndGetNode<EnumDeclaration>(src);

        Assert.AreEqual(declaration.Name, "Colors");
        Assert.AreEqual(declaration.Members.Count, 5);
    }

    [TestMethod]
    public void Enum_With_Strings_Should_Pass()
    {
        var src = "enum Colors { White = 'white', Red = 'red', Green = 'green', Blue = 'blue', Black = 'black' }";
        var declaration = ParseAndGetNode<EnumDeclaration>(src);

        Assert.AreEqual(declaration.Name, "Colors");
        Assert.AreEqual(declaration.Members.Count, 5);
    }

    [TestMethod]
    public void Enum_Without_Values_Should_Pass()
    {
        var src = "enum Colors { White, Red, Green, Blue, Black }";
        var declaration = ParseAndGetNode<EnumDeclaration>(src);

        Assert.AreEqual(declaration.Name, "Colors");
        Assert.AreEqual(declaration.Members.Count, 5);
    }
}