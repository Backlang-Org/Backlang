using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1;

[TestClass]
public class LiteralTests
{
    [TestMethod]
    public void TypeLiteral_Pointer_Should_Pass()
    {
        var literal = ParseLiteral("int32*");

        Assert.IsNotNull(literal);
        Assert.AreEqual(literal.Typename, "int32");
        Assert.IsTrue(literal.IsPointer);
    }

    [TestMethod]
    public void TypeLiteral_Should_Pass()
    {
        var literal = ParseLiteral("int32");

        Assert.IsNotNull(literal);
        Assert.AreEqual(literal.Typename, "int32");
    }

    [TestMethod]
    public void TypeLiteral_With_1_Dimension_Should_Pass()
    {
        var literal = ParseLiteral("int32[]");

        Assert.IsNotNull(literal);
        Assert.AreEqual(literal.Typename, "int32");
        Assert.AreEqual(literal.Dimensions, 1);
    }

    [TestMethod]
    public void TypeLiteral_With_2_Dimension_Should_Pass()
    {
        var literal = ParseLiteral("int32[,]");

        Assert.IsNotNull(literal);
        Assert.AreEqual(literal.Typename, "int32");
        Assert.AreEqual(literal.Dimensions, 2);
    }

    [TestMethod]
    public void TypeLiteral_With_Multiple_Dimension_Should_Pass()
    {
        var literal = ParseLiteral("int32[,,]");

        Assert.IsNotNull(literal);
        Assert.AreEqual(literal.Typename, "int32");
        Assert.AreEqual(literal.Dimensions, 3);
    }

    private static TypeLiteral ParseLiteral(string src)
    {
        var lexer = new Lexer();
        var tokens = lexer.Tokenize(src);

        return TypeLiteral.Parse(new TokenIterator(tokens));
    }
}