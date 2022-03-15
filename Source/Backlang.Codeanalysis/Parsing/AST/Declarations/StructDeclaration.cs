namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class StructDeclaration : SyntaxNode, IParsePoint<SyntaxNode>
{
    public List<StructMemberDeclaration> Members { get; set; } = new();
    public string Name { get; set; }

    public static SyntaxNode Parse(TokenIterator iterator, Parser parser)
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

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}