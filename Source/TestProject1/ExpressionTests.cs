using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Expressions;
using Backlang.Codeanalysis.Parsing.AST.Statements;
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

        Assert.IsInstanceOfType(expr, typeof(AddressOperationExpression));
        Assert.IsInstanceOfType(((AddressOperationExpression)expr).Expression, typeof(BinaryExpression));
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
}

[TestClass]
public class ExpressionTests : ParserTestBase
{
    [TestMethod]
    public void Empty_InitializerList_Should_Pass()
    {
        var src = "[];";
        var tree = ParseAndGetNodeInFunction<ExpressionStatement>(src);

        Assert.IsInstanceOfType(tree.Expression, typeof(InitializerListExpression));

        var list = (InitializerListExpression)tree.Expression;

        Assert.AreEqual(list.Elements.Count, 0);
    }

    [TestMethod]
    public void InitializerList_With_Element_Should_Pass()
    {
        var src = "[12];";
        var tree = ParseAndGetNodeInFunction<ExpressionStatement>(src);

        Assert.IsInstanceOfType(tree.Expression, typeof(InitializerListExpression));

        var list = (InitializerListExpression)tree.Expression;

        Assert.AreEqual(list.Elements.Count, 1);
        Assert.AreEqual(((LiteralNode)list.Elements[0]).Value, 12L);
    }

    [TestMethod]
    public void InitializerList_With_Elements_Should_Pass()
    {
        var src = "[1, 2];";
        var tree = ParseAndGetNodeInFunction<ExpressionStatement>(src);

        Assert.IsInstanceOfType(tree.Expression, typeof(InitializerListExpression));

        var list = (InitializerListExpression)tree.Expression;

        Assert.AreEqual(list.Elements.Count, 2);
        Assert.AreEqual(((LiteralNode)list.Elements[0]).Value, 1L);
        Assert.AreEqual(((LiteralNode)list.Elements[1]).Value, 2L);
    }

    [TestMethod]
    public void InitializerList_With_InitialierList_Element_Should_Pass()
    {
        var src = "[[]];";
        var tree = ParseAndGetNodeInFunction<ExpressionStatement>(src);

        Assert.IsInstanceOfType(tree.Expression, typeof(InitializerListExpression));

        var list = (InitializerListExpression)tree.Expression;

        Assert.AreEqual(list.Elements.Count, 1);
        Assert.AreEqual(((InitializerListExpression)list.Elements[0]).Elements.Count, 0);
    }

    [TestMethod]
    public void None_Should_Pass()
    {
        var src = "none;";
        var tree = ParseAndGetNodeInFunction<ExpressionStatement>(src);

        Assert.IsInstanceOfType(tree.Expression, typeof(NoneExpression));
    }

    [TestMethod]
    public void SizeOf_Should_Pass()
    {
        var src = "sizeof<i32>;";
        var tree = ParseAndGetNodeInFunction<ExpressionStatement>(src);
        var expression = (SizeOfExpression)tree.Expression;

        Assert.AreEqual(expression.Type.Typename, "i32");
    }
}