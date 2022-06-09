using Loyc;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class UsingDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        // using <expression> as <identifier>
        var keywordToken = iterator.Peek(-1);
        var from = Expression.Parse(parser);

        iterator.Match(TokenType.As);

        string to;
        if (iterator.Current.Type == TokenType.Identifier)
        {
            to = iterator.Current.Text;
            iterator.NextToken();
        }
        else
        {
            //error
            parser.Messages.Add(Message.Error(parser.Document,
                $"Expected Identifier, got {iterator.Current.Text}", iterator.Current.Line, iterator.Current.Column));
            return LNode.Missing;
        }

        iterator.Match(TokenType.Semicolon);

        return SyntaxTree.Using(from, LNode.Id((Symbol)to)).WithRange(keywordToken, iterator.Peek(-1));
    }
}