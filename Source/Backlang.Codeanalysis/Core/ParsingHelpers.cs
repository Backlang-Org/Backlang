using Backlang.Codeanalysis.Parsing;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Core;

internal static class ParsingHelpers
{
    public static LNodeList ParseSeperated(Parser parser, Func<Parser, LNode> callback, TokenType terminator, TokenType seperator = TokenType.Comma)
    {
        if (parser.Iterator.IsMatch(terminator))
        {
            parser.Iterator.Match(terminator);
            return LNodeList.Empty;
        }

        var list = new LNodeList();
        do
        {
            list.Add(callback(parser));
        } while (parser.Iterator.ConsumeIfMatch(seperator));

        parser.Iterator.Match(terminator);
        return list;
    }

    public static LNodeList ParseUntil(Parser parser, TokenType terminator, Func<Parser, LNode> callback)
    {
        var members = new LNodeList();
        while (parser.Iterator.Current.Type != terminator)
        {
            members.Add(callback(parser));
        }

        parser.Iterator.Match(terminator);

        return members;
    }
}