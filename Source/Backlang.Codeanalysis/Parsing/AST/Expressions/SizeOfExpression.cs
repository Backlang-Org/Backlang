namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public class SizeOfExpression : Expression, IParsePoint<Expression>
{
    public TypeLiteral Type { get; set; }

    public static Expression Parse(TokenIterator iterator, Parser parser)
    {
        //sizeof<i32>
        var expr = new SizeOfExpression();

        iterator.Match(TokenType.LessThan);

        expr.Type = TypeLiteral.Parse(iterator, parser);

        iterator.Match(TokenType.GreaterThan);

        return expr;
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}