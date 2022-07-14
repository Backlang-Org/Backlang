using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public sealed class NameExpression : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var nameExpression = SyntaxTree.Factory.Id(iterator.Peek(-1).Text);

        if (iterator.Current.Type == TokenType.OpenSquare)
        {
            iterator.NextToken();

            return SyntaxTree.ArrayInstantiation(nameExpression, Expression.ParseList(parser, TokenType.CloseSquare));
        }
        else if (iterator.Current.Type == TokenType.OpenParen)
        {
            iterator.NextToken();

            var arguments = Expression.ParseList(parser, TokenType.CloseParen);

            return SyntaxTree.Factory.Call(nameExpression, arguments);
        }

        return nameExpression;
    }
}