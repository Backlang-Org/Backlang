using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class StructDeclaration : IParsePoint<LNode>
{
    public List<StructMemberDeclaration> Members { get; set; } = new();
    public string Name { get; set; }

    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var node = new StructDeclaration();
        node.Name = iterator.Match(TokenType.Identifier).Text;

        iterator.Match(TokenType.OpenCurly);

        while (iterator.Current.Type != TokenType.CloseCurly)
        {
            node.Members.Add(StructMemberDeclaration.Parse(iterator, parser));
        }

        iterator.Match(TokenType.CloseCurly);

        return node;
    }
}