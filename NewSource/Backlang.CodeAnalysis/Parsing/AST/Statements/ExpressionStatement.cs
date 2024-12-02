using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public sealed class ExpressionStatement : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var expr = Expression.Parse(parser);

        iterator.Match(TokenType.Semicolon);

        return expr.WithRange(expr.Range.StartIndex, iterator.Prev.End);
    }
}