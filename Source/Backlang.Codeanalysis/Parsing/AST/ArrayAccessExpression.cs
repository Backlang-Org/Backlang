namespace Backlang.Codeanalysis.Parsing.AST;

public class ArrayAccessExpression : Expression
{
    public List<Expression> Indices { get; set; } = new();
    public NameExpression Name { get; set; }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
