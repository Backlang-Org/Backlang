using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements.Loops;

public sealed class WhileStatement : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;

        var cond = Expression.Parse(parser);
        var body = Statement.ParseOneOrBlock(parser);

        return SyntaxTree.While(cond, body).WithRange(keywordToken, iterator.Prev);
    }
}