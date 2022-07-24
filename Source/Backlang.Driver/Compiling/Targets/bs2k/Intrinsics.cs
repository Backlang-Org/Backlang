namespace Backlang.Driver.Compiling.Targets.bs2k;

public static class Intrinsics
{
    public static Register register;

    public static string Copy(string target, char value) => Copy(target, (int)value);

    public static string Copy(string target, int value) => $"copy {target}, {value}";

    public static string Jump(string label) => $"jump {label}";

    public static string Jump(Register register) => $"jump {register}";

    public static string Jump_Eq(string register, string registerOrLabel) => $"jump_eq {register}, {registerOrLabel}";

    public static string Jump_Neq(string register, string registerOrLabel) => $"jump_neq {register}, {registerOrLabel}";

    public static string Jump_Gt(string register, string registerOrLabel) => $"jump_gt {register}, {registerOrLabel}";

    public static string Jump_Ge(string register, string registerOrLabel) => $"jump_ge {register}, {registerOrLabel}";

    public static string Jump_Lt(string register, string registerOrLabel) => $"jump_lt {register}, {registerOrLabel}";

    public static string Jump_Le(string register, string registerOrLabel) => $"jump_le {register}, {registerOrLabel}";

    public static string Push(string register) => $"push {register}";

    public static string Push(int value) => $"copy {value}, R0\npush R0";

    public static string Push(char value) => Push((int)value);

    public static string Pop(string register) => $"pop {register}";

    public static string Ret() => "return";

    public static string NoOp() => "noop";

    public static string Halt() => "halt";

    public static string Add(string lhs, string rhs, string target) => $"add {lhs}, {rhs}, {target}";

    public static string Sub(string lhs, string rhs, string target) => $"sub {lhs}, {rhs}, {target}";

    public static string Mult(string lhs, string rhs, string high, string low) => $"add {lhs}, {rhs}, {high}, {low}";

    public static string Divmod(string lhs, string rhs, string result, string remainder) => $"divmod {lhs}, {rhs}, {result}, {remainder}";

    public static string And(string lhs, string rhs, string target) => $"and {lhs}, {rhs}, {target}";

    public static string Or(string lhs, string rhs, string target) => $"or {lhs}, {rhs}, {target}";

    public static string Comp(string lhs, string rhs, string target) => $"comp {lhs}, {rhs}, {target}";

    public static string Comp_Eq(string lhs, string rhs, string target) => $"comp_eq {lhs}, {rhs}, {target}";

    public static string Comp_Neq(string lhs, string rhs, string target) => $"comp_neq {lhs}, {rhs}, {target}";

    public static string Comp_Gt(string lhs, string rhs, string target) => $"comp_gt {lhs} {rhs}, {target}";

    public static string Comp_Ge(string lhs, string rhs, string target) => $"comp_ge {lhs}, {rhs}, {target}";

    public static string Comp_Lt(string lhs, string rhs, string target) => $"comp_lt {lhs}, {rhs}, {target}";

    public static string Comp_Le(string lhs, string rhs, string target) => $"comp_le {lhs}, {rhs}, {target}";
}