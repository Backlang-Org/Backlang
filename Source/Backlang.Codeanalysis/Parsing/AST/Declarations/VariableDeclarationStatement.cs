using Backlang.Codeanalysis.Parsing.AST.Statements;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class VariableDeclarationStatement : Statement
{
    public VariableDeclarationStatement(Token nameToken, TypeLiteral? type, Expression? value)
    {
        NameToken = nameToken;
        Type = type;
        Value = value;
    }

    public Token NameToken { get; }
    public TypeLiteral? Type { get; }
    public Expression? Value { get; }

    public static SyntaxNode Parse(TokenIterator iterator, Parser parser)
    {
        iterator.NextToken();

        var nameToken = iterator.Match(TokenType.Identifier);
        TypeLiteral? type = null;
        Expression? value = null;

        if (iterator.Current.Type == TokenType.Colon)
        {
            iterator.NextToken();

            type = TypeLiteral.Parse(iterator);
        }

        if (iterator.Current.Type == TokenType.EqualsToken)
        {
            iterator.NextToken();

            value = Expression.Parse(parser);
        }

        iterator.Match(TokenType.Semicolon);

        return new VariableDeclarationStatement(nameToken, type, value);
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}