using Backlang.Codeanalysis.Parsing.AST.Statements;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class FunctionDeclaration : Statement
{
    public FunctionDeclaration(Token name, TypeLiteral returnType, List<ParameterDeclaration> parameters, Block body)
    {
        Name = name;
        ReturnType = returnType;
        Parameters = parameters;
        Body = body;
    }

    public Block Body { get; }
    public Token Name { get; }
    public List<ParameterDeclaration> Parameters { get; }
    public TypeLiteral ReturnType { get; }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}