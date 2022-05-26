using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST;
using Loyc.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Expressions;

[TestClass]
public class LiteralTests
{
    [TestMethod]
    public void TypeLiteral_Pointer_Should_Pass()
    {
        var literal = ParseLiteral("int32*");

        Assert.IsNotNull(literal);
    }

    [TestMethod]
    public void TypeLiteral_Should_Pass()
    {
        var literal = ParseLiteral("int32");

        Assert.IsNotNull(literal);
    }

    [TestMethod]
    public void TypeLiteral_With_1_Dimension_Should_Pass()
    {
        var literal = ParseLiteral("int32[]");

        Assert.IsNotNull(literal);
    }

    [TestMethod]
    public void TypeLiteral_With_2_Dimension_Should_Pass()
    {
        var literal = ParseLiteral("int32[,]");

        Assert.IsNotNull(literal);
    }

    [TestMethod]
    public void TypeLiteral_With_InnerType_Should_Pass()
    {
        var src = "list<list<i32>>";
        var literal = ParseLiteral(src);
    }

    [TestMethod]
    public void TypeLiteral_With_Multiple_Dimension_Should_Pass()
    {
        var literal = ParseLiteral("int32[,,]");
    }

    [TestMethod]
    public void TypeLiteral_With_Type_Argument_Should_Pass()
    {
        var literal = ParseLiteral("list<i32>");
    }

    [TestMethod]
    public void TypeLiteral_With_Type_Arguments_Should_Pass()
    {
        var literal = ParseLiteral("list<i32, bool>");
    }

    private static LNode ParseLiteral(string src)
    {
        var lexer = new Lexer();
        var document = new SourceDocument("test.txt", src);
        var tokens = lexer.Tokenize(document);

        return TypeLiteral.Parse(new TokenIterator(tokens, document), new Parser(document, tokens, lexer.Messages));
    }
}