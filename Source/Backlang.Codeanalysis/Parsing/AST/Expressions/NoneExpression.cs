using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public sealed class NoneExpression : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        return SyntaxTree.None();
    }
}