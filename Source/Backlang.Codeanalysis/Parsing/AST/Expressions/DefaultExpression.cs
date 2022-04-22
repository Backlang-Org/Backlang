using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public sealed class DefaultExpression : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        //default<i32>
        //default
        if (iterator.Current.Type == TokenType.LessThan)
        {
            iterator.NextToken();

            var type = TypeLiteral.Parse(iterator, parser);

            iterator.Match(TokenType.GreaterThan);

            return SyntaxTree.Default(type);
        }

        return SyntaxTree.Default();
    }
}