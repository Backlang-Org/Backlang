using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public sealed class IfStatement : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        // if cond {} else {}

        var cond = Expression.Parse(parser);
        var body = Statement.ParseBlock(parser);
        LNodeList elseBlock = new();

        if (iterator.Current.Type == TokenType.Else)
        {
            iterator.NextToken();

            elseBlock = Statement.ParseBlock(parser);
        }

        return SyntaxTree.If(cond, body, elseBlock);
    }
}