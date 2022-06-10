using Backlang.Codeanalysis.Parsing.AST.Statements;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class FunctionDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;
        var signature = Signature.Parse(parser);

        return signature.WithAttrs(signature.Attrs).PlusArg(LNode.Call(CodeSymbols.Braces, Statement.ParseBlock(parser))
            .SetStyle(NodeStyle.StatementBlock));
    }
}