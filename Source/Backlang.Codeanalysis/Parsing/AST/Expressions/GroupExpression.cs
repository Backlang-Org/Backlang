using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public sealed class GroupOrTupleExpression : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var exprs = Expression.ParseList(parser, TokenType.CloseParen);

        if (exprs.Count == 1)
        {
            return SyntaxTree.Factory.InParens(exprs[0]);
        }

        return SyntaxTree.Factory.Tuple(exprs);
    }
}