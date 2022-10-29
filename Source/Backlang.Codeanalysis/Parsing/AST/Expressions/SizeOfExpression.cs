using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public sealed class SizeOfExpression : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        //sizeof(i32)

        iterator.Match(TokenType.OpenParen);

        var type = TypeLiteral.Parse(iterator, parser);

        iterator.Match(TokenType.CloseParen);

        return SyntaxTree.SizeOf(type);
    }
}