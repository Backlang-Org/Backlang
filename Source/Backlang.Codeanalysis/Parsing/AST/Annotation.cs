using Loyc;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class Annotation
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        iterator.Match(TokenType.At);
        var nameToken = iterator.Match(TokenType.Identifier);
        var args = LNode.List();

        if (iterator.Current.Type == TokenType.OpenParen)
        {
            iterator.NextToken();

            args = Expression.ParseList(parser, TokenType.CloseParen);

        
        }

        return SyntaxTree.Annotation(LNode.Call((Symbol)nameToken.Text, args));
    }

    public static bool TryParse(Parser parser, out LNode node)
    {
        var isAnnotation = parser.Iterator.IsMatch(TokenType.At) && parser.Iterator.Peek(1).Type == TokenType.Identifier;

        if (isAnnotation)
        {
            node = Parse(parser.Iterator, parser);
        }
        else
        {
            node = LNode.Missing;
        }

        return isAnnotation;
    }
}