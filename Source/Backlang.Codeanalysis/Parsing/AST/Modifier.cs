using Backlang.Codeanalysis.Core;
using Loyc;
using Loyc.Syntax;
using System.Collections.Immutable;

namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class Modifier
{
    private static ImmutableDictionary<TokenType, Symbol> possibleModifiers = new Dictionary<TokenType, Symbol>() {
        { TokenType.Static, CodeSymbols.Static },
        { TokenType.Public, CodeSymbols.Public },
        { TokenType.Protected, CodeSymbols.Protected },
        { TokenType.Private, CodeSymbols.Private },
        { TokenType.Internal, CodeSymbols.Internal },
        { TokenType.Operator, CodeSymbols.Operator },
        { TokenType.Abstract, CodeSymbols.Abstract },
        { TokenType.Override, CodeSymbols.Override },
        { TokenType.Extern, CodeSymbols.Extern },
    }.ToImmutableDictionary();

    public static bool TryParse(Parser parser, out LNodeList node)
    {
        var modifiers = new LNodeList();

        while (possibleModifiers.ContainsKey(parser.Iterator.Current.Type))
        {
            var modifier = ParseSingle(parser.Iterator, parser);
            if (modifiers.Contains(modifier))
            {
                parser.AddError(new(ErrorID.DuplicateModifier, modifier.Name.Name), modifier.Range);

                continue;
            }
            modifiers.Add(modifier);
        }
        node = modifiers;

        return modifiers.Count > 0;
    }

    private static LNode ParseSingle(TokenIterator iterator, Parser parser)
    {
        var currentToken = iterator.Current;
        var mod = SyntaxTree.Factory.Id(possibleModifiers[currentToken.Type]);
        iterator.NextToken();

        return mod.WithRange(currentToken);
    }
}