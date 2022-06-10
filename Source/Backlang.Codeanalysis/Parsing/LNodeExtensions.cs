using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing;

public static class LNodeExtensions
{
    public static LNode WithRange(this LNode node, int start, int end)
    {
        return node.WithRange(new SourceRange(null, start, end - start));
    }

    public static LNode WithRange(this LNode node, Token token)
    {
        return node.WithRange(token.Start, token.End);
    }

    public static LNode WithRange(this LNode node, Token startToken, Token endtoken)
    {
        return node.WithRange(startToken.Start, endtoken.End);
    }
}