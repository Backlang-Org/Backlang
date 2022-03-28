namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public class ForStatement : Statement, IParsePoint<Statement>
{
    public ForStatement(Expression variable, TypeLiteral? type, Expression collection, Block body)
    {
        Variable = variable;
        Type = type;
        Collection = collection;
        Body = body;
    }

    public Block Body { get; set; }
    public Expression Collection { get; set; }
    public TypeLiteral? Type { get; set; }
    public Expression Variable { get; set; }

    public static Statement Parse(TokenIterator iterator, Parser parser)
    {
        //for x : i32 in 1..12
        //for x in arr

        var varExpr = Expression.Parse(parser);

        TypeLiteral? type = null;

        if (iterator.Current.Type == TokenType.Colon)
        {
            iterator.NextToken();

            type = TypeLiteral.Parse(iterator, parser);
        }

        iterator.Match(TokenType.In);

        var collExpr = Expression.Parse(parser);
        var body = Statement.ParseBlock(parser);

        return new ForStatement(varExpr, type, collExpr, body);
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}