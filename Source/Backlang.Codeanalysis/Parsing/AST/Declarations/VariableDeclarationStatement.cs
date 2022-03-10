using Backlang.Codeanalysis.Parsing.AST.Statements;
namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class VariableDeclarationStatement : Statement
{
    public VariableDeclarationStatement(Token nameToken, Token? typeToken, Expression? value)
    {
        NameToken = nameToken;
        TypeToken = typeToken;
        Value = value;
    }

    public Token NameToken { get; }
    public Token? TypeToken { get; }
    public Expression? Value { get; }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
