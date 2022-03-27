namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public class IfStatement : Statement, IParsePoint<Statement>
{
    public IfStatement(Expression condition, Block body, Block? elseBody)
    {
        Condition = condition;
        Body = body;
        ElseBody = elseBody;
    }

    public Block Body { get; set; }
    public Expression Condition { get; set; }
    public Block? ElseBody { get; set; }

    public static Statement Parse(TokenIterator iterator, Parser parser)
    {
        // if cond {} else {}

        var cond = Expression.Parse(parser);
        var body = Statement.ParseBlock(parser);
        Block? elseBlock = null;

        if (iterator.Current.Type == TokenType.Else)
        {
            iterator.NextToken();

            elseBlock = Statement.ParseBlock(parser);
        }

        return new IfStatement(cond, body, elseBlock);
    }

    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}