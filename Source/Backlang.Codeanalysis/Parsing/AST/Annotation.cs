using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class Annotation
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var atToken = iterator.Match(TokenType.At);
        var name = Expression.Parse(parser);
        var args = LNode.List();

        if (iterator.IsMatch(TokenType.OpenParen))
        {
            iterator.NextToken();

            args = Expression.ParseList(parser, TokenType.CloseParen);
        }

        return SyntaxTree.Annotation(SyntaxTree.Factory.Call(name, args)).WithRange(atToken, iterator.Prev);
    }

    public static bool TryParse(Parser parser, out LNodeList node)
    {
        var annotations = new LNodeList();
        var isAnnotation = () => parser.Iterator.IsMatch(TokenType.At) && parser.Iterator.Peek(1).Type == TokenType.Identifier;

        while (isAnnotation())
        {
            annotations.Add(Parse(parser.Iterator, parser));
        }
        node = annotations;

        return annotations.Count > 0;
    }
}