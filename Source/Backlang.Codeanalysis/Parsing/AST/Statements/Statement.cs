namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public abstract class Statement : SyntaxNode
{
    public static Block ParseBlock(Parser parser)
    {
        parser.Iterator.Match(TokenType.OpenCurly);

        var body = new Block();
        while (parser.Iterator.Current.Type != TokenType.CloseCurly)
        {
            body.Body.Add(parser.InvokeStatementParsePoint());
        }

        parser.Iterator.Match(TokenType.CloseCurly);

        return body;
    }
}