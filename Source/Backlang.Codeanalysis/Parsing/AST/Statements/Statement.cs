using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public static class Statement
{
    public static LNode ParseBlock(Parser parser)
    {
        var openCurlyToken = parser.Iterator.Match(TokenType.OpenCurly);

        var body = new List<LNode>();
        while (!parser.Iterator.IsMatch(TokenType.CloseCurly) && !parser.Iterator.IsMatch(TokenType.EOF))
        {
            body.Add(parser.InvokeStatementParsePoint());
        }

        parser.Iterator.Match(TokenType.CloseCurly);

        return SyntaxTree.Factory.Braces(body).WithStyle(NodeStyle.StatementBlock)
            .WithRange(openCurlyToken, parser.Iterator.Prev);
    }

    public static LNode ParseOneOrBlock(Parser parser)
    {
        if (parser.Iterator.IsMatch(TokenType.OpenCurly))
        {
            return ParseBlock(parser);
        }
        else
        {
            var node = parser.InvokeStatementParsePoint();

            return SyntaxTree.Factory.Braces(node).WithStyle(NodeStyle.StatementBlock).WithRange(node.Range);
        }
    }
}