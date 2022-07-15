using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class TypeAliasDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        // using <expression> as <identifier>
        var keywordToken = iterator.Prev;
        var from = Expression.Parse(parser);

        LNode asKeyword = LNode.Missing;
        if (iterator.IsMatch(TokenType.As))
        {
            var asToken = iterator.Match(TokenType.As);
            asKeyword = SyntaxTree.Factory.Id(CodeSymbols.As.Name).WithRange(asToken);
        }

        var toToken = iterator.Match(TokenType.Identifier);
        var to = SyntaxTree.Factory.Id(toToken.Text).WithRange(toToken);

        iterator.Match(TokenType.Semicolon);

        var result = SyntaxTree.Using(from, to).WithRange(keywordToken, iterator.Prev);

        if (asKeyword != LNode.Missing)
        {
            result = result.PlusAttr(asKeyword);
        }

        return result;
    }
}