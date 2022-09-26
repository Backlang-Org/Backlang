using Backlang.Codeanalysis.Core;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class BitFieldDeclaration : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;
        var nameToken = iterator.Match(TokenType.Identifier);

        iterator.Match(TokenType.OpenCurly);

        var members = ParsingHelpers.ParseSeperated<BitFieldMemberDeclaration>(parser, TokenType.CloseCurly);

        return SyntaxTree.Bitfield(nameToken, members).WithRange(keywordToken, iterator.Prev);
    }
}