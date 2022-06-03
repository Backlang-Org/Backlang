using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class GlobalVariableDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var decl = VariableDeclaration.Parse(iterator, parser);

        return decl.WithAttrs(LNode.Id(Symbols.Global));
    }
}