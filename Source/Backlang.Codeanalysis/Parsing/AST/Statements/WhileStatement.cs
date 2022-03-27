namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public class WhileStatement : Statement, IParsePoint<Statement>
{
    public WhileStatement(Expression condition, Block body)
    {
        Condition = condition;
        Body = body;
    }

    public Block Body { get; set; }
    public Expression Condition { get; set; }

    public static Statement Parse(TokenIterator iterator, Parser parser)
    {
        // while true { 42; }
        var cond = Expression.Parse(parser);
        var body = Statement.ParseBlock(parser);

        return new WhileStatement(cond, body);
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}