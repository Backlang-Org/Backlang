using LeMP;
using Loyc;
using Loyc.Syntax;
using System.Text;

namespace Backlang.Core;

[ContainsMacros]
public static class BuiltInMacros
{
    private static LNodeFactory F = new LNodeFactory(EmptySourceFile.Synthetic);

    [LexicalMacro(@"nameof(id_or_expr)",
        @"Converts the 'key' name component of an expression to a string (e.g. nameof(A.B<C>(D)) == ""B"")")]
    public static LNode @nameof(LNode nameof, IMacroContext context)
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

    [LexicalMacro("concatId(id, id)", "Concats 2 Ids to a new Id (eg. concatId(a, b) == ab)")]
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

    [LexicalMacro("generateId()", "Generates a new Id (eg. generateId() == a0)")]
    public static LNode generateId(LNode generateID, IMacroContext context)
    {
        string alphabet = "abcdefghijklmnopqrstuvwxyz_";

        var sb = new StringBuilder();
        for (int i = 0; i < 3; i++)
        {
            sb.Append(Random.Shared.Next(0, alphabet.Length));
        }

        return F.Id("_" + sb.ToString() + context.IncrementTempCounter());
    }
}