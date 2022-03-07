namespace Backlang.Codeanalysis.Parsing.AST;

public abstract class SyntaxNode
{
    public abstract T Accept<T>(IVisitor<T> visitor);
}
