namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class StructMemberDeclaration : SyntaxNode
{
    public string Name { get; set; }
    public TypeLiteral Type { get; set; }

    public Expression? Value { get; set; }

    public static StructMemberDeclaration Parse(TokenIterator iterator, Parser parser)
    {
        var member = new StructMemberDeclaration();

        if (iterator.Current.Type == TokenType.Identifier)
        {
            member.Name = iterator.Current.Text;

            iterator.NextToken();
        }

        iterator.Match(TokenType.Colon);

        member.Type = TypeLiteral.Parse(iterator, parser);

        if (iterator.Current.Type == TokenType.EqualsToken)
        {
            iterator.NextToken();

            member.Value = Expression.Parse(parser);
        }

        iterator.Match(TokenType.Semicolon);

        return member;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}