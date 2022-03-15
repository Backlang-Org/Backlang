namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class BitFieldDeclaration : SyntaxNode, IParsePoint<SyntaxNode>
{
    public List<BitFieldMemberDeclaration> Members { get; set; } = new();
    public string Name { get; set; }

    public static SyntaxNode Parse(TokenIterator iterator, Parser parser)
    {
        var declaration = new BitFieldDeclaration();

        declaration.Name = iterator.Match(TokenType.Identifier).Text;

        iterator.Match(TokenType.OpenCurly);

        while (iterator.Current.Type != TokenType.CloseCurly)
        {
            var member = BitFieldMemberDeclaration.Parse(iterator, parser);

            if (iterator.Current.Type != TokenType.CloseCurly)
            {
                iterator.Match(TokenType.Comma);
            }

            declaration.Members.Add(member);
        }

        iterator.Match(TokenType.CloseCurly);

        return declaration;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}