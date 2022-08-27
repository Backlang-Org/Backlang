using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class BitFieldDeclaration : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;
        var name = iterator.Match(TokenType.Identifier).Text;

        iterator.Match(TokenType.OpenCurly);

        var members = new LNodeList();
        while (iterator.Current.Type != TokenType.CloseCurly)
        {
            var member = BitFieldMemberDeclaration.Parse(iterator, parser);

            if (iterator.Current.Type != TokenType.CloseCurly)
            {
                iterator.Match(TokenType.Comma);
            }

            members.Add(member);
        }

        iterator.Match(TokenType.CloseCurly);

        return SyntaxTree.Bitfield(name, members).WithRange(keywordToken, iterator.Prev);
    }
}