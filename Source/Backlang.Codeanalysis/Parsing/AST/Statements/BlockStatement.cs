using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

internal class BlockStatement : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        iterator.Position--;

        return Statement.ParseBlock(parser);
    }
}