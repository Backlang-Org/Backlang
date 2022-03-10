using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Declarations;
using Backlang.Codeanalysis.Parsing.AST.Expressions;
using Backlang.Codeanalysis.Parsing.AST.Statements;

namespace Backlang_Compiler.Compiling.Passes;

public class ConstantFoldingPass : IVisitor<object>
{
    public object Visit(InvalidNode invalidNode)
    {
        throw new NotImplementedException();
    }

    public object Visit(LiteralNode literal)
    {
        return literal.Value;
    }

    public object Visit(ExpressionStatement expressionStatement)
    {
        return expressionStatement.Expression.Accept(this);
    }

    public object Visit(CompilationUnit compilationUnit)
    {
        return compilationUnit.Body.Accept(this);
    }

    public object Visit(AssignmentStatement assignmentStatement)
    {
        return null;
    }

    public object Visit(BinaryExpression binaryExpression)
    {
        var lhs = (dynamic)binaryExpression.Left.Accept(this);
        var rhs = (dynamic)binaryExpression.Right.Accept(this);

        switch (binaryExpression.OperatorToken.Type)
        {
            case TokenType.Plus:
                return lhs + rhs;

            case TokenType.Minus:
                return lhs - rhs;

            case TokenType.Star:
                return lhs * rhs;

            case TokenType.Slash:
                return lhs / rhs;
        }

        return null;
    }

    public object Visit(UnaryExpression unaryExpression)
    {
        switch (unaryExpression.OperatorToken.Type)
        {
            case TokenType.Minus:
                return -((dynamic)unaryExpression.Expression.Accept(this));

            case TokenType.Exclamation:
                return !((dynamic)unaryExpression.Expression.Accept(this));
        }

        return null;
    }

    public object Visit(GroupExpression groupExpression)
    {
        return groupExpression.Inner.Accept(this);
    }

    public object Visit(Block block)
    {
        return block.Body.FirstOrDefault().Accept(this);
    }

    public object Visit(InvalidExpr invalidExpr)
    {
        throw new NotImplementedException();
    }

    public object Visit(NameExpression nameExpression)
    {
        return null;
    }

    public object Visit(CallExpr callExpr)
    {
        return null;
    }

    public object Visit(Expression expression)
    {
        return null;
    }

    public object Visit(VariableDeclarationStatement variableDeclarationStatement)
    {
        throw new NotImplementedException();
    }

    public object Visit(FunctionDeclaration functionDeclaration)
    {
        throw new NotImplementedException();
    }

    public object Visit(ParameterDeclaration parameterDeclaration)
    {
        throw new NotImplementedException();
    }
}