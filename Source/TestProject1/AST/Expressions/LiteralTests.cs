using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestProject1;

namespace TestProject1.AST.Expressions;

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
    public void TypeLiteral_With_InnerType_Should_Pass()
    {
        var src = "list<list<i32>>";
        var literal = ParseLiteral(src);

        Assert.AreEqual(literal.Typename, "list");
        Assert.AreEqual(literal.Arguments.Count, 1);

        Assert.IsInstanceOfType(literal.Arguments[0], typeof(TypeLiteral));

        var innerTypeLiteral = (TypeLiteral)literal.Arguments[0];
        Assert.AreEqual(innerTypeLiteral.Arguments.Count, 1);

        Assert.AreEqual(innerTypeLiteral.Typename, "list");

        Assert.IsInstanceOfType(innerTypeLiteral.Arguments[0], typeof(TypeLiteral));
        Assert.AreEqual(((TypeLiteral)innerTypeLiteral.Arguments[0]).Typename, "i32");
    }

    [TestMethod]
    public void TypeLiteral_With_Multiple_Dimension_Should_Pass()
    {
        var literal = ParseLiteral("int32[,,]");

        Assert.IsNotNull(literal);
        Assert.AreEqual(literal.Typename, "int32");
        Assert.AreEqual(literal.Dimensions, 3);
    }

    [TestMethod]
    public void TypeLiteral_With_Type_Argument_Should_Pass()
    {
        var literal = ParseLiteral("list<i32>");

        Assert.AreEqual(literal.Typename, "list");
        Assert.AreEqual(literal.Arguments.Count, 1);
        Assert.AreEqual(((TypeLiteral)literal.Arguments[0]).Typename, "i32");
    }

    [TestMethod]
    public void TypeLiteral_With_Type_Arguments_Should_Pass()
    {
        var literal = ParseLiteral("list<i32, bool>");

        Assert.AreEqual(literal.Typename, "list");
        Assert.AreEqual(literal.Arguments.Count, 2);
        Assert.AreEqual(((TypeLiteral)literal.Arguments[0]).Typename, "i32");
        Assert.AreEqual(((TypeLiteral)literal.Arguments[1]).Typename, "bool");
    }

    private static TypeLiteral ParseLiteral(string src)
    {
        var lexer = new Lexer();
        var tokens = lexer.Tokenize(src);

        return TypeLiteral.Parse(new TokenIterator(tokens), new Parser(new SourceDocument("test.txt", src), tokens, lexer.Messages));
    }
}
