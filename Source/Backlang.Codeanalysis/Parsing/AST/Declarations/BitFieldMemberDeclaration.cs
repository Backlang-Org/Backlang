namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class BitFieldMemberDeclaration : SyntaxNode
{
    public Expression Index { get; set; }
    public string Name { get; set; }

    public static BitFieldMemberDeclaration Parse(TokenIterator iterator, Parser parser)
    {
        var member = new BitFieldMemberDeclaration();

        if (iterator.Current.Type == TokenType.Identifier)
        {
            member.Name = iterator.Current.Text;

            iterator.NextToken();
        }
        else
        {
            member.Name = iterator.NextToken().Text;
        }

        iterator.Match(TokenType.EqualsToken);

        member.Index = Expression.Parse(parser);

        return member;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}