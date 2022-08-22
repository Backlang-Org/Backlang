using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public sealed class GroupOrTupleExpression : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var exprs = new LNodeList();

        while (iterator.Current.Type != TokenType.CloseParen && iterator.Current.Type != TokenType.Comma)
        {
            var parameter = Expression.Parse(parser);

            if (iterator.Current.Type == TokenType.Comma && iterator.Peek(1).Type != TokenType.CloseParen)
            {
                iterator.Match(TokenType.Comma);
            }

            exprs.Add(parameter);
        }

        iterator.Match(TokenType.CloseParen);

        if (exprs.Count == 1)
        {
            return SyntaxTree.Factory.InParens(exprs[0]);
        }

        return SyntaxTree.Factory.Tuple(exprs);
    }
}