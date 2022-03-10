using Backlang.Codeanalysis.Parsing.AST.Statements;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class FunctionDeclaration : Statement
{
    public FunctionDeclaration(Token name, Token returnTypeToken, List<ParameterDeclaration> parameters, Block body)
    {
        Name = name;
        ReturnTypeToken = returnTypeToken;
        Parameters = parameters;
        Body = body;
    }

    public Block Body { get; }
    public Token Name { get; }
    public List<ParameterDeclaration> Parameters { get; }
    public Token ReturnTypeToken { get; }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}