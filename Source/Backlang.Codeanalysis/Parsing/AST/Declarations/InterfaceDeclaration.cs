using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class InterfaceDeclaration : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;
        var nameToken = iterator.Match(TokenType.Identifier);
        var inheritances = new LNodeList();
        var members = new LNodeList();

        if (iterator.Current.Type == TokenType.Colon)
        {
            do
            {
                iterator.NextToken();
                inheritances.Add(Expression.Parse(parser));
            } while (iterator.Current.Type == TokenType.Comma);
        }

        iterator.Match(TokenType.OpenCurly);

        while (iterator.Current.Type != TokenType.CloseCurly)
        {
            members.Add(TypeFunctionDeclaration.Parse(iterator, parser));
        }

        iterator.Match(TokenType.CloseCurly);

        return SyntaxTree.Interface(nameToken, inheritances, members).WithRange(keywordToken, iterator.Prev);
    }
}