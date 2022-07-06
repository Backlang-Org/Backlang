using Loyc;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class Modifier
{
    private static Dictionary<TokenType, Symbol> possibleModifiers = new() {
        { TokenType.Static, CodeSymbols.Static },
        { TokenType.Public, CodeSymbols.Public },
        { TokenType.Protected, CodeSymbols.Protected },
        { TokenType.Private, CodeSymbols.Private },
        { TokenType.Operator, CodeSymbols.Operator },
        { TokenType.Abstract, CodeSymbols.Abstract },
        { TokenType.Override, CodeSymbols.Override },
        { TokenType.Extern, CodeSymbols.Extern },
    };

    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        TokenType current = parser.Iterator.Current.Type;
        var mod = LNode.Id(possibleModifiers[current]);
        parser.Iterator.NextToken();
        return mod;
    }

    public static bool TryParse(Parser parser, out LNodeList node)
    {
        var modifiers = new LNodeList();

        while (possibleModifiers.ContainsKey(parser.Iterator.Current.Type))
        {
            var modifier = Parse(parser.Iterator, parser);
            if (modifiers.Contains(modifier))
            {
                parser.AddError($"Modifier '{modifier.Name.Name}' is already applied");

                continue;
            }
            modifiers.Add(modifier);
        }
        node = modifiers;

        return modifiers.Count > 0;
    }
}