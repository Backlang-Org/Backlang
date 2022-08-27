using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public sealed class ContinueStatement : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;
        iterator.Match(TokenType.Semicolon);

        return LNode.Call(CodeSymbols.Continue).WithRange(keywordToken, iterator.Prev);
    }
}