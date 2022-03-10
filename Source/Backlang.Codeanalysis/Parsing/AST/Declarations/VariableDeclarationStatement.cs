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

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}