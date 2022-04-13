namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public sealed class DefaultExpression : Expression, IParsePoint<Expression>
{
    public TypeLiteral Type { get; set; }

    public static Expression Parse(TokenIterator iterator, Parser parser)
    {
        //default<i32>
        //default
        var expr = new DefaultExpression();

        if (iterator.Current.Type == TokenType.LessThan)
        {
            iterator.NextToken();

            expr.Type = TypeLiteral.Parse(iterator, parser);

            iterator.Match(TokenType.GreaterThan);
        }

        return expr;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}