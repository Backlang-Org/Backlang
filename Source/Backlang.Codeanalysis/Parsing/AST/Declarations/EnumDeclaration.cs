using Backlang.Codeanalysis.Core;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class EnumDeclaration : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;
        var nameToken = iterator.Match(TokenType.Identifier);

        iterator.Match(TokenType.OpenCurly);

        var members = ParsingHelpers.ParseSeperated<EnumMemberDeclaration>(parser, TokenType.CloseCurly);

        return SyntaxTree.Enum(LNode.Id(nameToken.Text), members).WithRange(keywordToken, iterator.Prev);
    }
}