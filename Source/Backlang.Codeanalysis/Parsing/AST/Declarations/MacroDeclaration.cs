using Backlang.Codeanalysis.Parsing.AST.Statements;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class MacroDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;
        var signature = Signature.Parse(parser);

        return signature.WithTarget(Symbols.Macro).PlusArg(LNode.Call(CodeSymbols.Braces, Statement.ParseBlock(parser))
            .SetStyle(NodeStyle.StatementBlock)).WithRange(keywordToken, iterator.Prev);
    }
}