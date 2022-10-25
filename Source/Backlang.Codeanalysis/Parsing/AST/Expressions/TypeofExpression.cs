using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public sealed class TypeofExpression : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        iterator.Match(TokenType.OpenParen);

        var type = TypeLiteral.Parse(iterator, parser);

        iterator.Match(TokenType.CloseParen);

        return SyntaxTree.TypeOfExpression(type);
    }
}