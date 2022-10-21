using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class MacroBlockDeclaration : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var nameExpression = LNode.Id(iterator.Prev.Text).WithRange(iterator.Current);

        var pp = parser.DeclarationParsePoints;

        foreach (var p in parser.StatementParsePoints)
        {
            if (pp.ContainsKey(p.Key)) continue;

            pp.Add(p.Key, p.Value);
        }

        if (iterator.Current.Type == TokenType.OpenParen)
        {
            iterator.NextToken();

            var arguments = Expression.ParseList(parser, TokenType.CloseParen);

            if (iterator.Current.Type == TokenType.OpenCurly)
            {
                iterator.NextToken();
                //custom code block with arguments

                var body = parser.InvokeDeclarationParsePoints(TokenType.CloseCurly, pp);
                iterator.Match(TokenType.CloseCurly);

                arguments = arguments.Add(LNode.Call(CodeSymbols.Braces, body));

                return SyntaxTree.Factory.Call(nameExpression, arguments).SetStyle(NodeStyle.StatementBlock)
                    .SetStyle(NodeStyle.Special).WithRange(nameExpression.Range.StartIndex, iterator.Prev.End);
            }
            else
            {
                parser.Iterator.Match(TokenType.Semicolon);
            }

            return LNode.Call(nameExpression, arguments);
        }
        else if (iterator.Current.Type == TokenType.OpenCurly)
        {
            iterator.NextToken();

            //custom code block without arguments
            var body = parser.InvokeDeclarationParsePoints(TokenType.CloseCurly, pp);
            iterator.Match(TokenType.CloseCurly);

            var arguments = LNode.List(LNode.Missing);
            arguments = arguments.Add(LNode.Call(CodeSymbols.Braces, body));

            return SyntaxTree.Factory.Call(nameExpression, arguments).SetStyle(NodeStyle.StatementBlock)
                .SetStyle(NodeStyle.Special).WithRange(nameExpression.Range.StartIndex, iterator.Prev.End);
        }

        return LNode.Missing;
    }
}