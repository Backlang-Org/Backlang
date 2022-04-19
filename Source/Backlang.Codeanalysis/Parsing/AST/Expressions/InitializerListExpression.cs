using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public sealed class InitializerListExpression : Expression, IParsePoint<LNode>
{
    public List<Expression> Elements { get; } = new();

    public static LNode Parse(TokenIterator iterator, Parser parser)
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
}