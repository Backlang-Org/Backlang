namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public class GroupExpression : Expression, IParsePoint<Expression>
{
    public GroupExpression(Expression inner)
    {
        Inner = inner;
    }

    public Expression Inner { get; set; }

    public static Expression Parse(TokenIterator iterator, Parser parser)
    {
        var expr = Expression.Parse(parser);

        iterator.Match(TokenType.CloseParen);

        return new GroupExpression(expr);
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}