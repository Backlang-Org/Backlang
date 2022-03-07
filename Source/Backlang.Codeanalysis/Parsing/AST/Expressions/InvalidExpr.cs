namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public class InvalidExpr : Expression
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
