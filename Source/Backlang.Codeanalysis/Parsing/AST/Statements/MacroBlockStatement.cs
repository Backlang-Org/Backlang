using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public sealed class MacroBlockStatement : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var nameToken = iterator.Prev;

        var currPos = iterator.Position - 1;

        var nameExpression = LNode.Id(nameToken.Text);

        if (iterator.ConsumeIfMatch(TokenType.OpenParen))
        {
            var arguments = Expression.ParseList(parser, TokenType.CloseParen);

            if (iterator.Current.Type == TokenType.OpenCurly)
            {
                //custom code block with arguments
                var body = Statement.ParseBlock(parser);
                arguments = arguments.Add(body);

                return SyntaxTree.Factory.Call(nameExpression, arguments).SetStyle(NodeStyle.StatementBlock)
                    .WithRange(nameToken, iterator.Prev);
            }
            else
            {
                iterator.Match(TokenType.Semicolon);

                return SyntaxTree.Factory.Call(nameExpression, arguments).WithRange(nameToken, iterator.Prev);
            }
        }
        else if (iterator.Current.Type == TokenType.OpenCurly)
        {
            //custom code block without arguments
            var body = Statement.ParseBlock(parser);
            var arguments = LNode.List(LNode.Missing, body);

            return SyntaxTree.Factory.Call(nameExpression, arguments).SetStyle(NodeStyle.StatementBlock)
                .WithRange(nameToken, iterator.Prev);
        }
        else
        {
            iterator.Position = currPos;

            return ExpressionStatement.Parse(iterator, parser);
        }
    }
}