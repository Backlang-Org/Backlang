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
        var ptrType = ParseTypeLiteral("int32*");
        var type = ptrType.Args[0];
        var literal = type.Args[0];

        Assert.IsNotNull(ptrType);
        Assert.IsTrue(ptrType.Calls(Symbols.PointerType));
        Assert.IsTrue(type.Calls(Symbols.TypeLiteral));
        Assert.AreEqual("int32", literal.Name);
    }

    [TestMethod]
    public void TypeLiteral_Should_Pass()
    {
        var literalType = ParseTypeLiteral("int32");
        var literal = literalType.Args[0];

        Assert.IsNotNull(literalType);
        Assert.AreEqual("int32", literal.Name);
    }

    [TestMethod]
    public void TypeLiteral_With_1_Dimension_Should_Pass()
    {
        var literal = ParseTypeLiteral("int32[]");

        Assert.IsNotNull(literal);
    }

    [TestMethod]
    public void TypeLiteral_With_2_Dimension_Should_Pass()
    {
        var literal = ParseTypeLiteral("int32[,]");

        Assert.IsNotNull(literal);
    }

    [TestMethod]
    public void TypeLiteral_With_InnerType_Should_Pass()
    {
        var src = "list<list<i32>>";
        var literal = ParseTypeLiteral(src);
    }

    [TestMethod]
    public void TypeLiteral_With_Multiple_Dimension_Should_Pass()
    {
        var literal = ParseTypeLiteral("int32[,,]");
    }

    [TestMethod]
    public void TypeLiteral_With_Type_Argument_Should_Pass()
    {
        var literal = ParseTypeLiteral("list<i32>");
    }

    [TestMethod]
    public void TypeLiteral_With_Type_Arguments_Should_Pass()
    {
        var literal = ParseTypeLiteral("list<i32, bool>");
    }

    private static LNode ParseTypeLiteral(string src)
    {
        var lexer = new Lexer();
        var document = new SourceDocument("test.txt", src);
        var tokens = lexer.Tokenize(document);

        return TypeLiteral.Parse(new TokenIterator(tokens, document), new Parser(document, tokens, lexer.Messages));
    }
}