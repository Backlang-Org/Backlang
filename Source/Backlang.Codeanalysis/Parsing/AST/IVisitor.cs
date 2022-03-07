using Backlang.Codeanalysis.Parsing.AST.Expressions;
using Backlang.Codeanalysis.Parsing.AST.Statements;
using Backlang.Codeanalysis.Parsing.AST;

namespace Backlang.Codeanalysis.Parsing.AST;

public interface IVisitor<T>
{
    T Visit(InvalidNode invalidNode);
    T Visit(VariableDeclarationStatement variableDeclarationStatement);
    T Visit(LiteralNode literal);

    T Visit(ExpressionStatement expressionStatement);

    T Visit(CompilationUnit compilationUnit);

    T Visit(AssignmentStatement assignmentStatement);

    T Visit(BinaryExpression binaryExpression);

    T Visit(UnaryExpression unaryExpression);

    T Visit(GroupExpression groupExpression);

    T Visit(Block block);

    T Visit(InvalidExpr invalidExpr);

    T Visit(NameExpression nameExpression);

    T Visit(CallExpr callExpr);

    T Visit(Expression expression);
}
