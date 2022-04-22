using Loyc;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Expressions.Match;

public sealed class MatchRule
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        if (iterator.Current.Type == TokenType.Underscore) // _ => 0
        {
            iterator.NextToken();

            iterator.Match(TokenType.GoesTo);

            var result = Expression.Parse(parser);

            return SyntaxTree.Factory.Tuple(LNode.Literal((Symbol)"_"), result);
        }
        else if (iterator.Peek(1).Type == TokenType.GoesTo) //12 => 13
        {
            var matcher = Expression.Parse(parser);

            iterator.Match(TokenType.GoesTo);

            var result = Expression.Parse(parser);

            return SyntaxTree.Factory.Tuple(matcher, result);
        }
        else if (iterator.Current.IsOperator()) // > 12 => false
        {
            var operatorToken = iterator.Current;
            iterator.NextToken();

            var condition = Expression.Parse(parser);

            iterator.Match(TokenType.GoesTo);

            var result = Expression.Parse(parser);

            return SyntaxTree.Factory.Tuple(SyntaxTree.Unary((Symbol)operatorToken.Text, condition), result);
        }
        else if (iterator.Current.Type == TokenType.Identifier && iterator.Peek(1).Type == TokenType.Identifier) //i32 num => num + 2
        {
            var type = TypeLiteral.Parse(iterator, parser);
            var name = iterator.NextToken().Text;

            iterator.Match(TokenType.GoesTo);

            var result = Expression.Parse(parser);

            return SyntaxTree.Factory.Tuple(SyntaxTree.Factory.Var(type, name), result);
        }
        else if (iterator.Current.Type == TokenType.Identifier) //i32 => 32
        {
            var type = TypeLiteral.Parse(iterator, parser);

            iterator.Match(TokenType.GoesTo);

            var result = Expression.Parse(parser);

            return SyntaxTree.Factory.Tuple(type, result);
        }

        return LNode.Missing;
    }
}