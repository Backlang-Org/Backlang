using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public abstract class Statement
{
    public static LNodeList ParseBlock(Parser parser)
    {
        parser.Iterator.Match(TokenType.OpenCurly);

        var body = new List<LNode>();
        while (!parser.Iterator.IsMatch(TokenType.CloseCurly))
        {
            body.Add(parser.InvokeStatementParsePoint());
        }

        parser.Iterator.Match(TokenType.CloseCurly);

        return LNode.List(body);
    }

    public static LNodeList ParseOneOrBlock(Parser parser)
    {
        if (parser.Iterator.IsMatch(TokenType.OpenCurly))
        {
            return ParseBlock(parser);
        } else
        {
            // TODO: Make BreakStatement also working, how?
            return LNode.List(ExpressionStatement.Parse(parser.Iterator, parser));
        }
    }
}