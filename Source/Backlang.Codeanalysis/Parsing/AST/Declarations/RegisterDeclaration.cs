namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class RegisterDeclaration : SyntaxNode, IParsePoint<SyntaxNode>
{
    public Expression Address { get; set; }
    public string Name { get; set; }

    public static SyntaxNode Parse(TokenIterator iterator, Parser parser)
    {
        var node = new RegisterDeclaration();

        node.Name = iterator.Current.Text;

        iterator.NextToken();

        iterator.Match(TokenType.EqualsToken);

        node.Address = Expression.Parse(parser);

        iterator.Match(TokenType.Semicolon);

        return node;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}