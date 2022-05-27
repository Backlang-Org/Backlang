using LeMP;
using Loyc.Syntax;

namespace Backlang.Core.Macros;

[ContainsMacros]
public static class TestCustomMacros
{
    [LexicalMacro("#matchCode(expr) {}", "match code", "matchCode", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode matchCode(LNode node, IMacroContext context)
    {
        return node.With(LNode.Id("macro executed"), LNode.List(node.Args[0]));
    }

    [LexicalMacro("#someMacro {}", "some Macro", "someMacro", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode someMacro(LNode node, IMacroContext context)
    {
        return LNode.Id("macro executed");
    }
}