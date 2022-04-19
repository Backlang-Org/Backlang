using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class ParameterDeclaration : SyntaxNode, IParsePoint<LNode>
{
    public ParameterDeclaration(Token name, TypeLiteral type, Expression? defaultValue)
    {
        Name = name;
        Type = type;
        DefaultValue = defaultValue;
    }

    public LNode? DefaultValue { get; }
    public Token Name { get; }
    public TypeLiteral Type { get; }

    public static SyntaxNode Parse(TokenIterator iterator, Parser parser)
    {
        var name = iterator.Match(TokenType.Identifier);

        iterator.Match(TokenType.Colon);

        var type = TypeLiteral.Parse(iterator, parser);

        LNode? defaultValue = null;

        if (iterator.Current.Type == TokenType.EqualsToken)
        {
            iterator.NextToken();

            defaultValue = Expression.Parse(parser);
        }

        return new ParameterDeclaration(name, type, defaultValue);
    }
}