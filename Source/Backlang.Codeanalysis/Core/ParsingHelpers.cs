using Backlang.Codeanalysis.Parsing;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Core;

internal static class ParsingHelpers
{
    public static LNodeList ParseSeperated<T>(
        Parser parser,
        TokenType terminator,
        TokenType seperator = TokenType.Comma, bool consumeTerminator = true)
        where T : IParsePoint
    {
        if (parser.Iterator.IsMatch(terminator))
        {
            parser.Iterator.Match(terminator);
            return LNodeList.Empty;
        }

        var list = new LNodeList();
        do
        {
            list.Add(T.Parse(parser.Iterator, parser));

            if (parser.Iterator.IsMatch(seperator) && parser.Iterator.Peek(1).Type == terminator)
            {
                parser.Iterator.Messages.Add(Message.Error("Trailing comma is forbidden"));
                parser.Iterator.Match(seperator);
            }
        } while (parser.Iterator.ConsumeIfMatch(seperator));

        if (consumeTerminator)
            parser.Iterator.Match(terminator);

        return list;
    }

    public static LNodeList ParseUntil<T>(Parser parser, TokenType terminator)
        where T : IParsePoint
    {
        var members = new LNodeList();
        while (parser.Iterator.Current.Type != terminator)
        {
            members.Add(T.Parse(parser.Iterator, parser));
        }

        parser.Iterator.Match(terminator);

        return members;
    }
}