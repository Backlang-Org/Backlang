using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public class ThrowStatement : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;

        LNode arg = LNode.Missing;
        if (!iterator.IsMatch(TokenType.Semicolon))
        {
            arg = Expression.Parse(parser);
        }

        iterator.Match(TokenType.Semicolon);

        return SyntaxTree.Throw(arg).WithRange(keywordToken, iterator.Prev);
    }
}