using Backlang.Codeanalysis.Parsing.AST.Statements;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class VariableDeclarationStatement : Statement, IParsePoint<LNode>
{
    public VariableDeclarationStatement(string name, TypeLiteral? type, bool isMutable, Expression? value)
    {
        Name = name;
        Type = type;
        IsMutable = isMutable;
        Value = value;
    }

    public bool IsMutable { get; }
    public string Name { get; }
    public TypeLiteral? Type { get; }
    public Expression? Value { get; }

    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        bool isMutable = false;
        TypeLiteral? type = null;
        Expression? value = null;

        if (iterator.Current.Type == TokenType.Mutable)
        {
            isMutable = true;
            iterator.NextToken();
        }

        var nameToken = iterator.Match(TokenType.Identifier);

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

        return new VariableDeclarationStatement(nameToken.Text, type, isMutable, value);
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}