using LeMP;
using Loyc;
using Loyc.Syntax;

namespace Backlang.Driver.InternalMacros;

[ContainsMacros]
public static class IntrinsicsMacros
{
    private static string[] availableNames;

    [LexicalMacro("inline(dotnet) {}", "Make Intrinsics Usable", "inline", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode InlineBlock(LNode node, IMacroContext context)
    {
        var body = node.Args.Last();
        var newBodyArgs = new LNodeList();

        var target = (string)context.ScopedProperties["Target"];
        var selectedTarget = node.Args[0].Name.Name;

        var compContext = (CompilerContext)context.ScopedProperties["Context"];

        if (selectedTarget == LNode.Missing.Name.Name)
        {
            context.Warn("No target speficied for inline block. Code will never be executed");
        }

        if (target != selectedTarget)
        {
            return LNode.Call((Symbol)"'{}");
        }

        if (!compContext.CompilationTarget.HasIntrinsics)
        {
            context.Warn($"{selectedTarget} has no intrinsics");

            return LNode.Call((Symbol)"'{}");
        }

        if (availableNames == null)
        {
            availableNames = InitAvailableNames(compContext.CompilationTarget.IntrinsicType);
        }

        foreach (var calls in body.Args)
        {
            if (!availableNames.Contains(calls.Name.Name))
            {
                context.Error($"{calls.Name.Name} is no intrinsic");
                continue;
            }

            var newCall = ConvertIntrinsic(calls, compContext.CompilationTarget.IntrinsicType);
            newBodyArgs = newBodyArgs.Add(newCall);
        }

        return LNode.Call((Symbol)"'{}", newBodyArgs).WithStyle(NodeStyle.Operator);
    }

    private static LNode ConvertIntrinsic(LNode call, Type instrinsicType)
    {
        var ns = instrinsicType.Namespace;
        var nsSplitted = ns.Split('.');

        LNode qualifiedName = LNode.Missing;
        for (var i = 0; i < nsSplitted.Length; i++)
        {
            var n = nsSplitted[i];

            if (qualifiedName == LNode.Missing)
            {
                qualifiedName = n.dot(nsSplitted[i + 1]);

                i += 1;
            }
            else
            {
                qualifiedName = qualifiedName.dot(n);
            }
        }

        return qualifiedName.dot(instrinsicType.Name).coloncolon(call);
    }

    private static string[] InitAvailableNames(Type intrinsicType)
    {
        return intrinsicType.GetMethods()
            .Where(_ => _.IsStatic)
            .Select(_ => _.Name.ToLower()).ToArray();
    }
}