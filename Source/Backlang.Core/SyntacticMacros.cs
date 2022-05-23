using LeMP;
using Loyc;
using Loyc.Syntax;

namespace Backlang.Core;

[ContainsMacros]
public static class SyntacticMacros
{
    private static LNodeFactory F = new LNodeFactory(EmptySourceFile.Synthetic);

    [LexicalMacro("'/=()", "Convert to left = left / something", Mode = MacroMode.MatchEveryCall)]
    public static LNode DivEquals(LNode @operator, IMacroContext context)
    {
        return ConverToAssignment(@operator, CodeSymbols.Div);
    }

    [LexicalMacro("'-=()", "Convert to left = left - something", Mode = MacroMode.MatchEveryCall)]
    public static LNode MinusEquals(LNode @operator, IMacroContext context)
    {
        return ConverToAssignment(@operator, CodeSymbols.Sub);
    }

    [LexicalMacro("'*=()", "Convert to left = left * something", Mode = MacroMode.MatchEveryCall)]
    public static LNode MulEquals(LNode @operator, IMacroContext context)
    {
        return ConverToAssignment(@operator, CodeSymbols.Mul);
    }

    [LexicalMacro("'+=()", "Convert to left = left + something", Mode = MacroMode.MatchEveryCall)]
    public static LNode PlusEquals(LNode @operator, IMacroContext context)
    {
        return ConverToAssignment(@operator, CodeSymbols.Add);
    }

    private static LNode ConverToAssignment(LNode @operator, Symbol symbol)
    {
        if (@operator.Name != (Symbol)symbol.Name.Insert(symbol.Name.Length, "="))
            return null;

        var arg1 = @operator.Args[0];
        var arg2 = @operator.Args[1];

        return F.Call(CodeSymbols.Assign, arg1, F.Call(symbol, arg1, arg2));
    }
}