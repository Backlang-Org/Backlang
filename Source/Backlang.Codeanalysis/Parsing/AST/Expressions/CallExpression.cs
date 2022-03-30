namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public class CallExpression : Expression
{
    public CallExpression(Expression name, List<Expression> arguments)
    {
        Name = name;
        Arguments = arguments;
    }

    public List<Expression> Arguments { get; }
    public Expression Name { get; }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}