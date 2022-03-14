namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class EnumDeclaration : SyntaxNode, IParsePoint<SyntaxNode>
{
    public List<EnumMemberDeclaration> Members { get; set; } = new();
    public string Name { get; set; }

    public static SyntaxNode Parse(TokenIterator iterator, Parser parser)
    {
        var declaration = new EnumDeclaration();

        iterator.NextToken();
        var nameToken = iterator.Match(TokenType.Identifier);

        iterator.Match(TokenType.OpenCurly);

        while (iterator.Current.Type != (TokenType.CloseCurly))
        {
            var memberNameToken = iterator.Current;
            var member = new EnumMemberDeclaration();
            member.Name = memberNameToken.Text;

            iterator.NextToken();

            if (iterator.Current.Type == TokenType.EqualsToken)
            {
                iterator.NextToken();

                member.Value = parser.ParsePrimary();
            }

            if (iterator.Current.Type != TokenType.CloseCurly)
            {
                iterator.Match(TokenType.Comma);
            }

            declaration.Members.Add(member);
        }

        iterator.Match(TokenType.CloseCurly);

        declaration.Name = nameToken.Text;

        return declaration;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}