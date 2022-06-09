using Backlang.Codeanalysis.Parsing.AST.Statements;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class FunctionDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var signature = Signature.Parse(parser);

        return signature
            .PlusArg(LNode.Call(CodeSymbols.Braces,
                Statement.ParseBlock(parser))
            .SetStyle(NodeStyle.StatementBlock))
            .WithRange(signature.Range.StartIndex, iterator.Peek(-1).End);
    }
}