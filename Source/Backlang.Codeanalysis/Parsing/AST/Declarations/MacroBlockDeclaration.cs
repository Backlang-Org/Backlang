using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class MacroBlockDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var nametoken = iterator.Current;
        var nameExpression = LNode.Id(nametoken.Text);
        iterator.NextToken();

        if (iterator.Current.Type == TokenType.OpenParen)
        {
            iterator.NextToken();

            var arguments = Expression.ParseList(parser, TokenType.CloseParen);

            if (iterator.Current.Type == TokenType.OpenCurly)
            {
                iterator.NextToken();
                //custom code block with arguments
                var body = parser.InvokeDeclarationParsePoints(TokenType.CloseCurly);
                iterator.Match(TokenType.CloseCurly);

                arguments = arguments.Add(LNode.Call(CodeSymbols.Braces, body));

                return LNode.Call(nameExpression, arguments).SetStyle(NodeStyle.StatementBlock)
                    .SetStyle(NodeStyle.Special).WithRange(nametoken, iterator.Peek(-1));
            }

            return LNode.Call(nameExpression, arguments);
        }
        else if (iterator.Current.Type == TokenType.OpenCurly)
        {
            iterator.NextToken();

            //custom code block without arguments
            var body = parser.InvokeDeclarationParsePoints(TokenType.CloseCurly);
            iterator.Match(TokenType.CloseCurly);

            var arguments = LNode.List(LNode.Missing);
            arguments = arguments.Add(LNode.Call(CodeSymbols.Braces, body));

            return LNode.Call(nameExpression, arguments)
                .SetStyle(NodeStyle.StatementBlock).SetStyle(NodeStyle.Special)
                .WithRange(nametoken, iterator.Peek(-1));
        }

        return LNode.Missing;
    }
}