namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public sealed class NoneExpression : Expression, IParsePoint<Expression>
{
    public static Expression Parse(TokenIterator iterator, Parser parser)
    {
        return new NoneExpression();
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}