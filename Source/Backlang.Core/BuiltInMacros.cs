using LeMP;
using Loyc.Syntax;

namespace Backlang.Core;

[ContainsMacros]
public static partial class BuiltInMacros
{
    [LexicalMacro("nameof(Identifier);",
            "Replacing the current node with the identifier name", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode nameof(LNode node, IMacroContext context)
    {
        return node;
    }
}