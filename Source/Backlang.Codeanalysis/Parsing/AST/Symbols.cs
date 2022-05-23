using Loyc;

namespace Backlang.Codeanalysis.Parsing.AST;

public static class Symbols
{
    public static readonly Symbol Bitfield = GSymbol.Get("#bitfield");
    public static readonly Symbol Global = GSymbol.Get("#global");
    public static readonly Symbol Implementation = GSymbol.Get("#implementation");
    public static readonly Symbol Match = GSymbol.Get("#match");
    public static readonly Symbol Mutable = GSymbol.Get("#mutable");
    public static readonly Symbol PointerType = GSymbol.Get("#type*");
    public static readonly Symbol Range = GSymbol.Get("'..");
    public static readonly Symbol ToExpand = GSymbol.Get("'to_expand'");
    public static readonly Symbol TypeLiteral = GSymbol.Get("#type");
}