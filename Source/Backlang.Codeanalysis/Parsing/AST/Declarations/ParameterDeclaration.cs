namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class ParameterDeclaration : SyntaxNode
{
    public ParameterDeclaration(Token name, Token type, Expression? defaultValue)
    {
        Name = name;
        Type = type;
        DefaultValue = defaultValue;
    }

    public Expression? DefaultValue { get; }
    public Token Name { get; }
    public Token Type { get; }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}