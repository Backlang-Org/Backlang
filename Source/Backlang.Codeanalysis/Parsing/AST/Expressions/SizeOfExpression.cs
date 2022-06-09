using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public sealed class SizeOfExpression : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        //sizeof<i32>
        var sizeofToken = iterator.Peek(-1);
        iterator.Match(TokenType.LessThan);

        var type = TypeLiteral.Parse(iterator, parser);

        iterator.Match(TokenType.GreaterThan);

        return SyntaxTree.SizeOf(type).WithRange(sizeofToken);
    }
}