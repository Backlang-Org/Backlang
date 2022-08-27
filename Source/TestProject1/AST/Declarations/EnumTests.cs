using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Declarations;

[TestClass]
public class EnumTests : ParserTestBase
{
    [TestMethod]
    public void Enum_With_Int_Values_And_SingleLineComment_At_End_Should_Pass()
    {
        var src = "enum Colors { White = 0, Red = 1, Green = 2, Blue = 3, Black = 4 } // something";
        var result = ParseAndGetNodes(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void Enum_With_Int_Values_And_SingleLineComment_In_Middle_Should_Pass()
    {
        var src = "enum Colors { White = 0, Red = 1, Green = 2 //s \n, Blue = 3, Black = 4 } ";
        var result = ParseAndGetNodes(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void Enum_With_Int_Values_Should_Pass()
    {
        var src = "enum Colors { White = 0, Red = 1, Green = 2, Blue = 3, Black = 4 }";
        var result = ParseAndGetNodes(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void Enum_With_Strings_Should_Pass()
    {
        var src = "enum Colors { White = \"white\", Red = \"red\", Green = \"green\", Blue = \"blue\", Black = \"black\" }";
        var result = ParseAndGetNodes(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void Enum_Without_Values_Should_Pass()
    {
        var src = "enum Colors { White, Red, Green, Blue, Black }";
        var result = ParseAndGetNodes(src);

        Assert.AreEqual(0, result.errors.Count);
    }

    [TestMethod]
    public void Enum_Without_Values_With_Trailing_Comma_Should_Fail()
    {
        var src = "enum Colors { White, Red, Green, Blue, Black, }";
        var result = ParseAndGetNodes(src);

        Assert.AreEqual(1, result.errors.Count);
    }
}