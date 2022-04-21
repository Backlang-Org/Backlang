using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Declarations;

[TestClass]
public class EnumTests : ParserTestBase
{
    [TestMethod]
    public void Enum_With_Int_Values_And_SingleLineComment_At_End_Should_Pass()
    {
        var src = "enum Colors { White = 0, Red = 1, Green = 2, Blue = 3, Black = 4 } // something";
        var declaration = ParseAndGetNodes(src);

        Assert.AreEqual(declaration.Name, "Colors");
        Assert.AreEqual(declaration.Members.Count, 5);
    }

    [TestMethod]
    public void Enum_With_Int_Values_And_SingleLineComment_In_Middle_Should_Pass()
    {
        var src = "enum Colors { White = 0, Red = 1, Green = 2 //s \n, Blue = 3, Black = 4 } ";
        var declaration = ParseAndGetNodes(src);

        Assert.AreEqual(declaration.Name, "Colors");
        Assert.AreEqual(declaration.Members.Count, 5);
    }

    [TestMethod]
    public void Enum_With_Int_Values_Should_Pass()
    {
        var src = "enum Colors { White = 0, Red = 1, Green = 2, Blue = 3, Black = 4 }";
        var declaration = ParseAndGetNodes(src);

        Assert.AreEqual(declaration.Name, "Colors");
        Assert.AreEqual(declaration.Members.Count, 5);
    }

    [TestMethod]
    public void Enum_With_Strings_Should_Pass()
    {
        var src = "enum Colors { White = 'white', Red = 'red', Green = 'green', Blue = 'blue', Black = 'black' }";
        var declaration = ParseAndGetNodes(src);

        Assert.AreEqual(declaration.Name, "Colors");
        Assert.AreEqual(declaration.Members.Count, 5);
    }

    [TestMethod]
    public void Enum_Without_Values_Should_Pass()
    {
        var src = "enum Colors { White, Red, Green, Blue, Black }";
        var declaration = ParseAndGetNodes(src);

        Assert.AreEqual(declaration.Name, "Colors");
        Assert.AreEqual(declaration.Members.Count, 5);
    }
}