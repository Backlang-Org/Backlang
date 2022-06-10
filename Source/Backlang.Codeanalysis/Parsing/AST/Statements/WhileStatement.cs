using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public sealed class WhileStatement : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        // while true { 42; }
        var keywordToken = iterator.Prev;

        var cond = Expression.Parse(parser);
        var body = Statement.ParseOneOrBlock(parser);

        return SyntaxTree.While(cond, body).WithRange(keywordToken, iterator.Prev);
    }
}