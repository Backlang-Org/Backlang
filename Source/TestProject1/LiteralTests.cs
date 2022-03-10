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
    }

    [TestMethod]
    public void TypeLiteral_Should_Pass()
    {
        var literal = ParseLiteral("int32");

        Assert.IsNotNull(literal);
        Assert.AreEqual(literal.Typename, "int32");
    }

    private static TypeLiteral ParseLiteral(string src)
    {
        var lexer = new Lexer();
        var tokens = lexer.Tokenize(src);

        return TypeLiteral.Parse(new TokenIterator(tokens));
    }
}