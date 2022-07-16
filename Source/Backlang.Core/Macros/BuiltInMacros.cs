using LeMP;
using Loyc;
using Loyc.Syntax;
using System.Text;

namespace Backlang.Core.Macros;

[ContainsMacros]
public static partial class BuiltInMacros
{
    private static LNodeFactory F = new LNodeFactory(EmptySourceFile.Synthetic);

    [LexicalMacro(@"nameof(id_or_expr)",
        @"Converts the 'key' name component of an expression to a string (e.g. nameof(A.B<C>(D)) == ""B"")", "nameof", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode @Nameof(LNode nameof, IMacroContext context)
    {
        if (nameof.ArgCount != 1)
            return null;

        var arg = nameof.Args[0];
        if (arg.IsCall)
        {
            if (arg.Name == (Symbol)".")
            {
                if (arg.Args[1].IsCall)
                {
                    return F.Literal(arg.Args[1].Name);
                }

                return arg.Args[1];
            }
            if (arg.Name == (Symbol)"::")
            {
                if (arg.Args[1].IsCall)
                {
                    return F.Literal(arg.Args[1].Name);
                }

                return arg.Args[1];
            }
            else if (arg.Target.IsId)
            {
                return arg.Target;
            }
        }

        return arg;
    }

    [LexicalMacro("concatId(id, id)", "Concats 2 Ids to a new Id (eg. concatId(a, b) == ab)", "concatId", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode concatId(LNode concatID, IMacroContext context)
    {
        if (concatID.ArgCount < 2)
            return null;

        var sb = new StringBuilder();

        foreach (var arg in concatID.Args)
        {
            if (arg.IsId)
            {
                sb.Append(arg.Name.Name);
            }
            else
            {
                context.Sink.Warning(arg, "Argument is not an Id");
            }
        }

        return F.Id(sb.ToString());
    }

    [LexicalMacro("$variableName",
            "Expands a variable (scoped property) assigned by a macro such as `static deconstruct()` or `static tryDeconstruct()`.",
            "'$", Mode = MacroMode.Passive)]
    public static LNode DollarSignVariable(LNode node, IMacroContext context)
    {
        LNode id;
        if (node.ArgCount == 1 && (id = node.Args[0]).IsId && !id.HasPAttrs())
        {
            object value;
            if (context.ScopedProperties.TryGetValue("$" + id.Name.Name, out value))
            {
                if (value is LNode)
                    return ((LNode)value).WithRange(id.Range);
                else
                    context.Sink.Warning(id, "The specified scoped property is not a syntax tree. " +
                        "Use `#getScopedProperty({0})` to insert it as a literal.", id.Name);
            }
            else
            {
                context.Sink.Error(id, "There is no macro property in scope named `{0}`", id.Name);
            }
        }
        return null;
    }

    [LexicalMacro("left <-> right", "Swaps the values of the two variables", "'<->", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode Swap(LNode node, IMacroContext context)
    {
        var left = node.Args[0];
        var right = node.Args[1];
        var temp = GenerateId(null, context);

        return LNode.Call(CodeSymbols.Braces, LNode.List(
            LNode.Call(CodeSymbols.Let, LNode.List(LNode.Missing, LNode.Call(CodeSymbols.Assign, LNode.List(temp, left)))),
            LNode.Call(CodeSymbols.Assign, LNode.List(left, right)),
            LNode.Call(CodeSymbols.Assign, LNode.List(right, temp))
            )).SetStyle(NodeStyle.StatementBlock);
    }

    [LexicalMacro("generateId()", "Generates a new Id (eg. generateId() == a0)", "generateId", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode GenerateId(LNode generateID, IMacroContext context)
    {
        var alphabet = "abcdefghijklmnopqrstuvwxyz_";

        var sb = new StringBuilder();
        for (var i = 0; i < 3; i++)
        {
            sb.Append(Random.Shared.Next(0, alphabet.Length));
        }

        return F.Id("_" + sb.ToString() + context.IncrementTempCounter());
    }
}