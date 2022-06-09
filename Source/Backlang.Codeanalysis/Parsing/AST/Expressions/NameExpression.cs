using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public sealed class NameExpression : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var nameToken = iterator.Peek(-1);
        var nameExpression = LNode.Id(nameToken.Text);

        if (iterator.Current.Type == TokenType.OpenSquare)
        {
            iterator.NextToken();

            return SyntaxTree.ArrayInstantiation(nameExpression,
                Expression.ParseList(parser, TokenType.CloseSquare)).WithRange(nameToken, iterator.Peek(-1));
        }
        else if (iterator.Current.Type == TokenType.OpenParen)
        {
            iterator.NextToken();

            var arguments = Expression.ParseList(parser, TokenType.CloseParen);

            return LNode.Call(nameExpression, arguments).WithRange(nameToken, iterator.Peek(-1));
        }

        return nameExpression.WithRange(nameToken);
    }
}