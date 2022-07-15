using Loyc;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class TypeAliasDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        // using <expression> as <identifier>
        var keywordToken = iterator.Prev;
        var from = Expression.Parse(parser);

        var asToken = iterator.Match(TokenType.As);
        var asKeyword = SyntaxTree.Factory.Id(asToken.Text).WithRange(asToken);

        string to;
        if (iterator.Current.Type == TokenType.Identifier)
        {
            to = iterator.Current.Text;
            iterator.NextToken();
        }
        else
        {
            //error
            parser.AddError($"Expected Identifier, got {iterator.Current.Text}", iterator.Current.Line, iterator.Current.Column);
            return LNode.Missing;
        }

        iterator.Match(TokenType.Semicolon);

        return SyntaxTree.Using(from, SyntaxTree.Factory.Id((Symbol)to)).PlusAttr(asKeyword).WithRange(keywordToken, iterator.Prev);
    }
}