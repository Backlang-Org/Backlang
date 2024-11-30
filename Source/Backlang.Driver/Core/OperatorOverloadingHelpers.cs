namespace Backlang.Driver.Core;

public static class OperatorOverloadingHelpers
{
    private static readonly ImmutableDictionary<string, string> binMap = new Dictionary<string, string>
    {
        ["'+"] = "op_Addition",
        ["'/"] = "op_Division",
        ["'-"] = "op_Subtraction",
        ["'*"] = "op_Multiply",
        ["'%"] = "op_Modulus",
        ["'&"] = "op_BitwiseAnd",
        ["'|"] = "op_BitwiseOr",
        ["'^"] = "op_ExclusiveOr",
        ["'=="] = "op_Equality",
        ["'!="] = "op_Inequality"
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<string, string> unMap = new Dictionary<string, string>
    {
        ["'!"] = "op_LogicalNot",
        ["'-"] = "op_UnaryNegation",
        ["'~"] = "op_OnesComplement",
        ["'*"] = "op_Deref",
        ["'&"] = "op_AddressOf",
        ["'%"] = "op_Percentage",
        ["'suf?"] = "op_Unpacking",
        ["implicit"] = "op_Implicit",
        ["explicit"] = "op_Explicit"
    }.ToImmutableDictionary();

    public static bool TryGetOperator(this IType type, string op, out IMethod opMethod, params IType[] args)
    {
        var possibleMethods = type.Methods.Where(_ => _.IsStatic
                                                      && !_.IsConstructor && !_.IsDestructor
                                                      && _.Attributes.GetAll().Any(__ =>
                                                          __?.AttributeType?.Name.ToString() == "SpecialNameAttribute")
                                                      && _.Parameters.Count == args.Length
        );

        ImmutableDictionary<string, string> nameMap = null;
        if (args.Length == 1)
        {
            nameMap = unMap;
        }
        else if (args.Length == 2)
        {
            nameMap = binMap;
        }

        possibleMethods =
            possibleMethods.Where(_ => nameMap.ContainsValue(_.Name.ToString()) && nameMap[op] == _.Name.ToString());

        foreach (var method in possibleMethods)
        {
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var param = method.Parameters[i].Type;

                if (arg != param)
                {
                    goto nextMethod;
                }
            }

            opMethod = method;
            return true;

            nextMethod: ;
        }

        opMethod = null;
        return false;
    }
}