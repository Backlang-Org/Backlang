using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class Annotation
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var atToken = iterator.Match(TokenType.At);
        var call = Expression.Parse(parser);

        return SyntaxTree.Annotation(call).WithRange(atToken, iterator.Prev);
    }

    public static bool TryParse(Parser parser, out LNodeList node)
    {
        var annotations = new LNodeList();
        var isAnnotation = () =>
            parser.Iterator.IsMatch(TokenType.At) && parser.Iterator.Peek(1).Type == TokenType.Identifier;

        while (isAnnotation())
        {
            annotations.Add(Parse(parser.Iterator, parser));
        }

        node = annotations;

        return annotations.Count > 0;
    }
}