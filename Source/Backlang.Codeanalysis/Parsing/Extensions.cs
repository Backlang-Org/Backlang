using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing;

public static class Extensions
{
    public static LNode WithRange(this LNode node, int start, int length)
    {
        return node.WithRange(new SourceRange(null, start, length));
    }

    public static LNode WithRange(this LNode node, Token token)
    {
        return WithRange(node, token.Start, token.Text.Length);
    }

    public static LNode WithRange(this LNode node, Token startToken, Token endToken)
    {
        return WithRange(node, startToken.Start, endToken.Start - startToken.Start);
    }
}