using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST.Expressions;
using Backlang.Codeanalysis.Parsing.AST.Statements.Assembler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace TestProject1;

[TestClass]
public class AssemblerTests
{
    [TestMethod]
    public void AddressOperation_Should_Pass()
    {
        var src = "&[0xFF + 4]";
        var lexer = new Lexer();
        var tokens = lexer.Tokenize(src);

        var expr = Expression.Parse(new Parser(null, tokens, lexer.Messages), AssemblerBlockStatement.ExpressionParsePoints);

        Assert.IsInstanceOfType(expr, typeof(UnaryExpression));

        var unary = (UnaryExpression)expr;

        Assert.IsInstanceOfType(((AddressOperationExpression)unary.Expression).Expression, typeof(BinaryExpression));
    }

    [TestMethod]
    public void Instruction_Mov_Should_Pass()
    {
        var src = "{ mov EAX, 12; }";

        var lexer = new Lexer();
        var tokens = lexer.Tokenize(src);

        var parser = new Parser(null, tokens, lexer.Messages);

        var block = AssemblerBlockStatement.Parse(parser.Iterator, parser);

        var instruciton = ((AssemblerBlockStatement)block).Body.OfType<Instruction>().First();

        Assert.AreEqual(instruciton.OpCode, "mov");
    }

    [TestMethod]
    public void LabelBlock_Mov_Should_Pass()
    {
        var src = "{ loop { mov EAX, 12; jmp $loop; } }";

        var lexer = new Lexer();
        var tokens = lexer.Tokenize(src);

        var parser = new Parser(null, tokens, lexer.Messages);

        var block = AssemblerBlockStatement.Parse(parser.Iterator, parser);

        var instruciton = ((AssemblerBlockStatement)block).Body.OfType<LabelBlockDefinition>().First();

        Assert.AreEqual(instruciton.Name, "loop");
    }

    [TestMethod]
    public void LabelReference_Should_Pass()
    {
        var src = "$eax";
        var lexer = new Lexer();
        var tokens = lexer.Tokenize(src);

        var expr = Expression.Parse(new Parser(null, tokens, lexer.Messages), AssemblerBlockStatement.ExpressionParsePoints);

        Assert.IsInstanceOfType(expr, typeof(LabelReferenceExpression));

        Assert.AreEqual(((LabelReferenceExpression)expr).Label, "eax");
    }

    [TestMethod]
    public void PTROperation_Should_Pass()
    {
        var src = "&[PTR 0xFF + 4]";
        var lexer = new Lexer();
        var tokens = lexer.Tokenize(src);

        var expr = Expression.Parse(new Parser(null, tokens, lexer.Messages), AssemblerBlockStatement.ExpressionParsePoints);

        Assert.IsInstanceOfType(expr, typeof(UnaryExpression));

        var unary = (UnaryExpression)((AddressOperationExpression)((UnaryExpression)expr).Expression).Expression;

        Assert.IsInstanceOfType(unary.Expression, typeof(BinaryExpression));
    }
}