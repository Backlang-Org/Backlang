using Loyc;

namespace Backlang.Codeanalysis.Parsing.AST;

public static class Symbols
{
    public static readonly Symbol Global = GSymbol.Get("Global");
    public static readonly Symbol Mutable = GSymbol.Get("Mutable");
    public static readonly Symbol PointerType = GSymbol.Get("Type*");
    public static readonly Symbol TypeLiteral = GSymbol.Get("Type");
}