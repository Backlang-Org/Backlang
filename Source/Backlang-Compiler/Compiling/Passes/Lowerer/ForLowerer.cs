using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Declarations;
using Backlang.Codeanalysis.Parsing.AST.Expressions;
using Backlang.Codeanalysis.Parsing.AST.Expressions.Match;
using Backlang.Codeanalysis.Parsing.AST.Statements;
using Backlang.Codeanalysis.Parsing.AST.Statements.Assembler;

namespace Backlang_Compiler.Compiling.Passes.Lowerer;

public class ForLowerer : IVisitor<SyntaxNode>
{
    public SyntaxNode Visit(InvalidNode invalidNode)
    {
        return invalidNode;
    }

    public SyntaxNode Visit(Instruction instruction)
    {
        return instruction;
    }

    public SyntaxNode Visit(VariableDeclarationStatement variableDeclarationStatement)
    {
        return variableDeclarationStatement;
    }

    public SyntaxNode Visit(ForStatement forStatement)
    {
        var identifier = (NameExpression)forStatement.Variable;
        Expression lowerBound = null;
        Expression upperBound = null;

        if (forStatement.Collection is BinaryExpression binary && binary.OperatorToken.Text == "..")
        {
            lowerBound = binary.Left;
            upperBound = binary.Right;
        }

        var init = new VariableDeclarationStatement(identifier.Name, forStatement.Type, true, lowerBound);
        var body = new Block();
        body.Body.Add(init);

        var whileBody = new Block();
        whileBody.Body.AddRange(forStatement.Body.Body);

        whileBody.Body.Add(new AssignmentStatement(identifier.Name, new BinaryExpression(identifier, new Token(TokenType.Plus, "+"), new LiteralNode(1))));

        var whileStatement = new WhileStatement(new BinaryExpression(identifier, new Token(TokenType.LessThan, "<"), upperBound), whileBody);
        body.Body.Add(whileStatement);

        return body;
    }

    public SyntaxNode Visit(WhileStatement whileStatement)
    {
        return whileStatement;
    }

    public SyntaxNode Visit(IfStatement ifStatement)
    {
        return ifStatement;
    }

    public SyntaxNode Visit(TypeAliasDeclaration typeAliasDeclaration)
    {
        return typeAliasDeclaration;
    }

    public SyntaxNode Visit(LiteralNode literal)
    {
        return literal;
    }

    public SyntaxNode Visit(StructDeclaration structDeclaration)
    {
        return structDeclaration;
    }

    public SyntaxNode Visit(AssemblerBlockStatement assemblerBlockStatement)
    {
        return assemblerBlockStatement;
    }

    public SyntaxNode Visit(RegisterDeclaration registerDeclaration)
    {
        return registerDeclaration;
    }

    public SyntaxNode Visit(BitFieldDeclaration bitFieldDeclaration)
    {
        return bitFieldDeclaration;
    }

    public SyntaxNode Visit(ExpressionStatement expressionStatement)
    {
        return expressionStatement;
    }

    public SyntaxNode Visit(CompilationUnit compilationUnit)
    {
        compilationUnit.Body = (Block)compilationUnit.Body.Accept(this);

        return compilationUnit;
    }

    public SyntaxNode Visit(BitFieldMemberDeclaration bitFieldMemberDeclaration)
    {
        return bitFieldMemberDeclaration;
    }

    public SyntaxNode Visit(EnumDeclaration enumDeclaration)
    {
        return enumDeclaration;
    }

    public SyntaxNode Visit(AssignmentStatement assignmentStatement)
    {
        return assignmentStatement;
    }

    public SyntaxNode Visit(BinaryExpression binaryExpression)
    {
        return binaryExpression;
    }

    public SyntaxNode Visit(StructMemberDeclaration structMemberDeclaration)
    {
        return structMemberDeclaration;
    }

    public SyntaxNode Visit(UnaryExpression unaryExpression)
    {
        return unaryExpression;
    }

    public SyntaxNode Visit(FunctionDeclaration functionDeclaration)
    {
        functionDeclaration.Body = (Block)functionDeclaration.Body.Accept(this);

        return functionDeclaration;
    }

    public SyntaxNode Visit(EnumMemberDeclaration enumMemberDeclaration)
    {
        return enumMemberDeclaration;
    }

    public SyntaxNode Visit(GroupExpression groupExpression)
    {
        return groupExpression;
    }

    public SyntaxNode Visit(Block block)
    {
        var blk = new Block();

        foreach (var node in block.Body)
        {
            blk.Body.Add(node.Accept(this));
        }

        return blk;
    }

    public SyntaxNode Visit(InvalidExpr invalidExpr)
    {
        return invalidExpr;
    }

    public SyntaxNode Visit(NameExpression nameExpression)
    {
        return nameExpression;
    }

    public SyntaxNode Visit(CallExpression callExpr)
    {
        return callExpr;
    }

    public SyntaxNode Visit(MatchExpression expression)
    {
        return expression;
    }

    public SyntaxNode Visit(DefaultExpression expression)
    {
        return expression;
    }

    public SyntaxNode Visit(InitializerListExpression expression)
    {
        return expression;
    }

    public SyntaxNode Visit(Expression expression)
    {
        return expression;
    }

    public SyntaxNode Visit(ParameterDeclaration parameterDeclaration)
    {
        return parameterDeclaration;
    }

    public SyntaxNode Visit(TypeLiteral typeLiteral)
    {
        return typeLiteral;
    }

    public SyntaxNode Visit(LabelBlockDefinition labelBlockDefinition)
    {
        return labelBlockDefinition;
    }

    public SyntaxNode Visit(GlobalVariableDeclaration globalVariableDeclaration)
    {
        return globalVariableDeclaration;
    }

    public SyntaxNode Visit(ConstVariableDeclaration constVariableDeclaration)
    {
        return constVariableDeclaration;
    }

    public SyntaxNode Visit(ImplementationDeclaration implementationDeclaration)
    {
        return implementationDeclaration;
    }
}