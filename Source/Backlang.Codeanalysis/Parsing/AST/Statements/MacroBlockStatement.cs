using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public sealed class MacroBlockStatement : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var nameToken = iterator.Prev;

        var currPos = iterator.Position - 1;

        var nameExpression = LNode.Id(nameToken.Text);

        if (iterator.Current.Type == TokenType.OpenParen)
        {
            iterator.NextToken();

            var arguments = Expression.ParseList(parser, TokenType.CloseParen);

            if (iterator.Current.Type == TokenType.OpenCurly)
            {
                //custom code block with arguments
                var body = Statement.ParseBlock(parser);
                arguments = arguments.Add(body);

                return LNode.Call(nameExpression, arguments).SetStyle(NodeStyle.StatementBlock)
                    .WithRange(nameToken, iterator.Prev);
            }
            else
            {
                iterator.Position = currPos;

                return ExpressionStatement.Parse(iterator, parser);
            }
        }
        else if (iterator.Current.Type == TokenType.OpenCurly)
        {
            //custom code block without arguments
            var body = Statement.ParseBlock(parser);
            var arguments = LNode.List(LNode.Missing, body);

            return LNode.Call(nameExpression, arguments).SetStyle(NodeStyle.StatementBlock)
                .WithRange(nameToken, iterator.Prev);
        }
        else
        {
            iterator.Position = currPos;

            return ExpressionStatement.Parse(iterator, parser);
        }
    }
}