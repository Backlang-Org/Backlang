using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public sealed class ForStatement : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        //for x : i32 in 1..12
        //for x in arr
        var keywordToken = iterator.Prev;
        var varExpr = Expression.Parse(parser);

        LNode type = LNode.Missing;

        if (iterator.Current.Type == TokenType.Colon)
        {
            iterator.NextToken();

            type = TypeLiteral.Parse(iterator, parser);
        }

        iterator.Match(TokenType.In);

        var collExpr = Expression.Parse(parser);
        var body = Statement.ParseOneOrBlock(parser);

        return SyntaxTree.For(SyntaxTree.Factory.Tuple(varExpr, type), collExpr, body)
            .WithRange(keywordToken, iterator.Prev);
    }
}