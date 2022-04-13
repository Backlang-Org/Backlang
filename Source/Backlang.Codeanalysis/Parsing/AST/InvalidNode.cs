using Backlang.Codeanalysis.Parsing.AST;
namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class InvalidNode : SyntaxNode
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
