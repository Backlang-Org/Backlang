using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public sealed class IfStatement : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        // if cond {} else {}
        var keywordToken = iterator.Prev;
        var cond = Expression.Parse(parser);
        var body = Statement.ParseOneOrBlock(parser);
        LNode elseBlock = LNode.Missing;

        if (iterator.Current.Type == TokenType.Else)
        {
            iterator.NextToken();

            elseBlock = Statement.ParseOneOrBlock(parser);
        }

        return SyntaxTree.If(cond, body, elseBlock).WithRange(keywordToken, iterator.Prev);
    }
}