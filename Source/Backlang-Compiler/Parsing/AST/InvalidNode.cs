using Backlang_Compiler.Parsing.AST;
namespace Backlang_Compiler.Parsing.AST;

public class InvalidNode : SyntaxNode
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
