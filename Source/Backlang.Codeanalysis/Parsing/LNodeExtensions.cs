using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing;

public static class LNodeExtensions
{
    public static LNode WithRange(this LNode node, int start, int end)
    {
        return node.WithRange(new SourceRange(null, start, end - start)).WithAttrs(node.Attrs);
    }

    public static LNode WithRange(this LNode node, Token token)
    {
        return WithRange(node, token.Start, token.End);
    }

    public static LNode WithRange(this LNode node, Token startToken, Token endtoken)
    {
        return WithRange(node, startToken.Start, endtoken.End);
    }
}