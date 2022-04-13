namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public sealed class InvalidExpr : Expression
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
