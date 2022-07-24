using LeMP;
using Loyc;
using Loyc.Syntax;

namespace Backlang.Driver.InternalMacros;

[ContainsMacros]
public static class IntrinsicsMacros
{
    private static string[] intrinsicNames;

    [LexicalMacro("inline(dotnet) {}", "Make Intrinsics Usable", "inline", Mode = MacroMode.MatchIdentifierOrCall)]
    public static LNode InlineBlock(LNode node, IMacroContext context)
    {
        var body = node.Args.Last();
        var selectedTarget = node.Args[0].Name.Name;

        var newBodyArgs = new LNodeList();

        var target = (string)context.ScopedProperties["Target"];

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

        if (intrinsicNames == null)
        {
            intrinsicNames = InitAvailableIntrinsicNames(compContext.CompilationTarget.IntrinsicType);
        }

        var availableConstants = GetAvailableConstants(compContext.CompilationTarget.IntrinsicType);

        for (var i = 0; i < body.Args.Count; i++)
        {
            var calls = body.Args[i];
            if (!intrinsicNames.Contains(calls.Name.Name))
            {
                compContext.AddError(calls, $"{calls.Name.Name} is no intrinsic");
                continue;
            }

            calls = ConvertCall(calls, compContext, availableConstants);

            var newCall = ConvertIntrinsic(calls, compContext.CompilationTarget.IntrinsicType);
            newBodyArgs = newBodyArgs.Add(newCall);
        }

        return LNode.Call((Symbol)"'{}", newBodyArgs).WithStyle(NodeStyle.Operator);
    }

    private static LNode ConvertCall(LNode calls, CompilerContext context, Dictionary<string, object> availableConstants)
    {
        var newArgs = new LNodeList();

        foreach (var arg in calls.Args)
        {
            if (arg.IsId)
            {
                var constantExists = availableConstants.ContainsKey(arg.Name.Name);

                if (!constantExists)
                {
                    context.AddError(arg, $"Constant '{arg.Name.Name}' does not exists");
                    continue;
                }

                var constantValue = availableConstants[arg.Name.Name];
                newArgs.Add(LNode.Call((Symbol)$"#{constantValue.GetType().Name}", LNode.List(LNode.Literal(constantValue))));
            }
            else
            {
                newArgs.Add(arg);
            }
        }

        return calls.WithArgs(newArgs);
    }

    private static Dictionary<string, object> GetAvailableConstants(Type intrinsicType)
    {
        var result = new Dictionary<string, object>();
        foreach (var field in intrinsicType.GetFields())
        {
            if (field.FieldType.IsAssignableTo(typeof(Enum)))
            {
                var names = Enum.GetNames(field.FieldType);
                foreach (var name in names)
                {
                    result.Add(name, Enum.Parse(field.FieldType, name));
                }
            }
        }

        return result;
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

    private static string[] InitAvailableIntrinsicNames(Type intrinsicType)
    {
        return intrinsicType.GetMethods()
            .Where(_ => _.IsStatic)
            .Select(_ => _.Name.ToLower()).Distinct().ToArray();
    }
}