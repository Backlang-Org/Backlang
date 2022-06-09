using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public sealed class ReturnStatement : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Peek(-1);
        var arguments = LNode.List();

        if (!iterator.IsMatch(TokenType.Semicolon))
        {
            arguments.Add(Expression.Parse(parser));
        }

        iterator.Match(TokenType.Semicolon);

        return LNode.Call(CodeSymbols.Return, arguments).WithRange(keywordToken, iterator.Peek(-1));
    }
}