using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements.Loops;

public sealed class DoWhileStatement : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;

        var body = Statement.ParseBlock(parser);

        iterator.Match(TokenType.While);

        var cond = Expression.Parse(parser);

        iterator.Match(TokenType.Semicolon);

        return SyntaxTree.DoWhile(body, cond).WithRange(keywordToken, iterator.Prev);
    }
}