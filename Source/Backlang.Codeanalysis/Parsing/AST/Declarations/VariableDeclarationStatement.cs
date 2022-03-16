using Backlang.Codeanalysis.Parsing.AST.Statements;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class VariableDeclarationStatement : Statement, IParsePoint<Statement>
{
    public VariableDeclarationStatement(Token nameToken, TypeLiteral? type, bool isMutable, Expression? value)
    {
        NameToken = nameToken;
        Type = type;
        IsMutable = isMutable;
        Value = value;
    }

    public bool IsMutable { get; }
    public Token NameToken { get; }
    public TypeLiteral? Type { get; }
    public Expression? Value { get; }

    public static Statement Parse(TokenIterator iterator, Parser parser)
    {
        bool isMutable = false;
        TypeLiteral? type = null;
        Expression? value = null;
        Token nameToken = null;

        if (iterator.Current.Type == TokenType.Mutable)
        {
            isMutable = true;
            iterator.NextToken();
        }

        nameToken = iterator.Match(TokenType.Identifier);

        if (iterator.Current.Type == TokenType.Colon)
        {
            iterator.NextToken();

            type = TypeLiteral.Parse(iterator, parser);
        }

        if (iterator.Current.Type == TokenType.EqualsToken)
        {
            iterator.NextToken();

            value = Expression.Parse(parser);
        }

        iterator.Match(TokenType.Semicolon);

        return new VariableDeclarationStatement(nameToken, type, isMutable, value);
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}