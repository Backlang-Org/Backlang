namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class ParameterDeclaration : SyntaxNode, IParsePoint<SyntaxNode>
{
    public ParameterDeclaration(Token name, TypeLiteral type, Expression? defaultValue)
    {
        Name = name;
        Type = type;
        DefaultValue = defaultValue;
    }

    public Expression? DefaultValue { get; }
    public Token Name { get; }
    public TypeLiteral Type { get; }

    public static SyntaxNode Parse(TokenIterator iterator, Parser parser)
    {
        var name = iterator.Match(TokenType.Identifier);

        iterator.Match(TokenType.Colon);

        var type = TypeLiteral.Parse(iterator, parser);

        Expression? defaultValue = null;

        if (iterator.Current.Type == TokenType.EqualsToken)
        {
            iterator.NextToken();

            defaultValue = Expression.Parse(parser);
        }

        return new ParameterDeclaration(name, type, defaultValue);
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}