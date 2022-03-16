namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public class InitializerListExpression : Expression, IParsePoint<Expression>
{
    public List<Expression> Elements { get; } = new();

    public static Expression Parse(TokenIterator iterator, Parser parser)
    {
        var node = new InitializerListExpression();

        do
        {
            if (iterator.Current.Type == (TokenType.CloseSquare))
            {
                break;
            }

            node.Elements.Add(Expression.Parse(parser));

            if (iterator.Current.Type != TokenType.CloseSquare)
            {
                iterator.Match(TokenType.Comma);
            }
        } while (iterator.Current.Type != (TokenType.CloseSquare));

        iterator.Match(TokenType.CloseSquare);

        return node;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}