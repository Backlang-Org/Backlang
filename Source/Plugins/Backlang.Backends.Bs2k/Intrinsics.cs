namespace Backlang.Backends.Bs2k;

//Hint: order in bssembler instructions: source, target
public static class Intrinsics
{
    public static Register register;
    public static Addresses addresses;

    public static Constants constants;

    public static string Label(string label) => $"{label}:\n";

    public static string Copy(char value, string target) => Copy((int)value, target);

    public static string Copy(char value, Register target) => Copy((int)value, target);

    public static string Copy(int value, string target) => $"copy {value}, {target}";

    public static string Copy(int value, Register target) => $"copy {value}, {target}";

    public static string Jump(string label) => $"jump {label}";

    public static string Jump(Register register) => $"jump {register}";

    public static string Jump_Eq(Register register, Register addr) => $"jump_eq {register}, {addr}";

    public static string Jump_Eq(Register register, string addr) => $"jump_eq {register}, {addr}";

    public static string Jump_Neq(Register register, string addr) => $"jump_neq {register}, {addr}";

    public static string Jump_Neq(Register register, Register addr) => $"jump_neq {register}, {addr}";

    public static string Jump_Gt(Register register, string addr) => $"jump_gt {register}, {addr}";

    public static string Jump_Gt(Register register, Register addr) => $"jump_gt {register}, {addr}";

    public static string Jump_Ge(Register register, string addr) => $"jump_ge {register}, {addr}";

    public static string Jump_Ge(Register register, Register addr) => $"jump_ge {register}, {addr}";

    public static string Jump_Lt(Register register, string addr) => $"jump_lt {register}, {addr}";

    public static string Jump_Lt(Register register, Register addr) => $"jump_lt {register}, {addr}";

    public static string Jump_Le(Register register, string addr) => $"jump_le {register}, {addr}";

    public static string Jump_Le(Register register, Register addr) => $"jump_le {register}, {addr}";

    public static string Push(Register register) => $"push {register}";

    public static string Push(int value) => $"copy {value}, R0\npush R0";

    public static string Push(char value) => Push((int)value);

    public static string Pop(Register register) => $"pop {register}";

    public static string Pop() => $"pop";

    public static string Ret() => "return";

    public static string NoOp() => "noop";

    public static string Halt() => "halt";

    public static string Add(Register lhs, Register rhs, Register target) => $"add {lhs}, {rhs}, {target}";

    public static string Sub(Register lhs, Register rhs, Register target) => $"sub {lhs}, {rhs}, {target}";

    public static string Mult(Register lhs, Register rhs, Register high, Register low) => $"add {lhs}, {rhs}, {high}, {low}";

    public static string Divmod(Register lhs, Register rhs, Register result, Register remainder) => $"divmod {lhs}, {rhs}, {result}, {remainder}";

    public static string And(Register lhs, Register rhs, Register target) => $"and {lhs}, {rhs}, {target}";

    public static string Or(Register lhs, Register rhs, Register target) => $"or {lhs}, {rhs}, {target}";

    public static string Comp(Register lhs, Register rhs, Register target) => $"comp {lhs}, {rhs}, {target}";

    public static string Comp_Eq(Register lhs, Register rhs, Register target) => $"comp_eq {lhs}, {rhs}, {target}";

    public static string Comp_Neq(Register lhs, Register rhs, Register target) => $"comp_neq {lhs}, {rhs}, {target}";

    public static string Comp_Gt(Register lhs, Register rhs, Register target) => $"comp_gt {lhs} {rhs}, {target}";

    public static string Comp_Ge(Register lhs, Register rhs, Register target) => $"comp_ge {lhs}, {rhs}, {target}";

    public static string Comp_Lt(Register lhs, Register rhs, Register target) => $"comp_lt {lhs}, {rhs}, {target}";

    public static string Comp_Le(Register lhs, Register rhs, Register target) => $"comp_le {lhs}, {rhs}, {target}";
}