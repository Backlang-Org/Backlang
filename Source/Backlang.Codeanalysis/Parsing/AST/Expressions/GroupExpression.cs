using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public sealed class GroupExpression : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var openParenToken = iterator.Peek(-1);
        var expr = Expression.Parse(parser);

        iterator.Match(TokenType.CloseParen);

        return SyntaxTree.Factory.InParens(expr).WithRange(openParenToken, iterator.Peek(-1));
    }
}