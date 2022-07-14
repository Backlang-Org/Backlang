using Loyc;

namespace Backlang.Codeanalysis.Parsing.AST;

public static class Symbols
{
    public static readonly Symbol Annotation = GSymbol.Get("#annotation");
    public static readonly Symbol Bitfield = GSymbol.Get("#bitfield");
    public static readonly Symbol Float16 = GSymbol.Get("#float16");
    public static readonly Symbol Float32 = GSymbol.Get("#float32");
    public static readonly Symbol Float64 = GSymbol.Get("#float64");
    public static readonly Symbol Global = GSymbol.Get("#global");
    public static readonly Symbol Implementation = GSymbol.Get("#implement");
    public static readonly Symbol Inheritance = GSymbol.Get("#inheritance");
    public static readonly Symbol Macro = GSymbol.Get("#macro");
    public static readonly Symbol Match = GSymbol.Get("#match");
    public static readonly Symbol Mutable = GSymbol.Get("#mutable");
    public static readonly Symbol PointerType = GSymbol.Get("#type*");
    public static readonly Symbol Range = GSymbol.Get("'..");
    public static readonly Symbol ToExpand = GSymbol.Get("'to_expand'");
    public static readonly Symbol TypeLiteral = GSymbol.Get("#type");
    public static readonly Symbol Union = GSymbol.Get("#union");
    public static readonly Symbol Where = GSymbol.Get("#where");
}