using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public abstract class Statement : SyntaxNode
{
    public static LNodeList ParseBlock(Parser parser)
    {
        parser.Iterator.Match(TokenType.OpenCurly);

        var body = new List<LNode>();
        while (parser.Iterator.Current.Type != TokenType.CloseCurly)
        {
            body.Add(parser.InvokeStatementParsePoint());
        }

        parser.Iterator.Match(TokenType.CloseCurly);

        return LNode.List(body);
    }
}