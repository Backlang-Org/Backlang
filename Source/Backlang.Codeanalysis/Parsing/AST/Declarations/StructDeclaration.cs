using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class StructDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var name = iterator.Match(TokenType.Identifier).Text;
        var members = new LNodeList();

        iterator.Match(TokenType.OpenCurly);

        while (iterator.Current.Type != TokenType.CloseCurly)
        {
            members.Add(StructMemberDeclaration.Parse(iterator, parser));
        }

        iterator.Match(TokenType.CloseCurly);

        return SyntaxTree.Struct(name, members);
    }
}