using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1;

//ToDo: verschachtelte typenamen erlauben
// list<list<i32>>
// token durch Expression ersetzen

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

    [TestMethod]
    public void TypeLiteral_With_Type_Argument()
    {
        var literal = ParseLiteral("list<i32>");

        Assert.AreEqual(literal.Typename, "list");
        Assert.AreEqual(literal.Arguments.Count, 1);
        Assert.AreEqual(literal.Arguments[0], "i32");
    }

    [TestMethod]
    public void TypeLiteral_With_Type_Arguments()
    {
        var literal = ParseLiteral("list<i32, bool>");

        Assert.AreEqual(literal.Typename, "list");
        Assert.AreEqual(literal.Arguments.Count, 2);
        Assert.AreEqual(literal.Arguments[0], "i32");
        Assert.AreEqual(literal.Arguments[1], "bool");
    }

    private static TypeLiteral ParseLiteral(string src)
    {
        var lexer = new Lexer();
        var tokens = lexer.Tokenize(src);

        return TypeLiteral.Parse(new TokenIterator(tokens));
    }
}