using Furesoft.Core.CodeDom.Compiler.Core;
using System.Collections.Immutable;

namespace Backlang.Driver.Core;

public static class OperatorOverloadingHelpers
{
    private static readonly ImmutableDictionary<string, string> nameMap = new Dictionary<string, string>()
    {
        ["'+"] = "op_Addition",
        ["'/"] = "op_Division",
        ["'-"] = "op_Subtraction",
        ["'*"] = "op_Multiply",
        ["'%"] = "op_Modulus",

        ["'!"] = "op_Not",
    }.ToImmutableDictionary();

    public static bool TryGetOperator(this IType type, string op, out IMethod opMethod, params IType[] args)
    {
        var possibleMethods = type.Methods.Where(_ => _.IsStatic
            && !_.IsConstructor && !_.IsDestructor
            && _.Attributes.GetAll().Any(__ => __?.AttributeType?.Name.ToString() == "SpecialNameAttribute")
            && nameMap.ContainsValue(_.Name.ToString())
            && nameMap[op] == _.Name.ToString()
            && _.Parameters.Count == args.Length
        );

        foreach (var method in possibleMethods)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var param = method.Parameters[i].Type;

                if (arg != param) goto nextMethod;
            }
            opMethod = method;
            return true;

        nextMethod: continue;
        }

        opMethod = null;
        return false;
    }
}