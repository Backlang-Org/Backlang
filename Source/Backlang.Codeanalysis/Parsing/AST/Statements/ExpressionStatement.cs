namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public class ExpressionStatement : Statement
{
    public ExpressionStatement(Expression expression)
    {
        Expression = expression;
    }

    public Expression Expression { get; set; }

    public static SyntaxNode Parse(TokenIterator iterator, Parser parser)
    {
        var expr = Expression.Parse(parser);

        iterator.Match(TokenType.Semicolon);

        return new ExpressionStatement(expr);
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}