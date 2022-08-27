using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public sealed class ReturnStatement : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;
        var arguments = LNode.List();

        if (!iterator.IsMatch(TokenType.Semicolon))
        {
            arguments.Add(Expression.Parse(parser));
        }

        iterator.Match(TokenType.Semicolon);

        return SyntaxTree.Factory.Call(CodeSymbols.Return, arguments).WithRange(keywordToken, iterator.Prev);
    }
}