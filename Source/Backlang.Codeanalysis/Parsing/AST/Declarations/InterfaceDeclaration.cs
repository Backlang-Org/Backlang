using Backlang.Codeanalysis.Core;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class InterfaceDeclaration : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;
        var nameToken = iterator.Match(TokenType.Identifier);
        var inheritances = new LNodeList();

        if (iterator.ConsumeIfMatch(TokenType.Colon))
        {
            inheritances = Expression.ParseList(parser, TokenType.OpenCurly, false);
        }

        iterator.Match(TokenType.OpenCurly);

        var members = ParsingHelpers.ParseUntil<TypeMemberDeclaration>(parser, TokenType.CloseCurly);

        iterator.Match(TokenType.CloseCurly);

        return SyntaxTree.Interface(nameToken, inheritances, members).WithRange(keywordToken, iterator.Prev);
    }
}