namespace Backlang.Backends.Bs2k;

//Hint: order in bssembler instructions: source, target
public static class Intrinsics
{
    public static Register register;
    public static Addresses addresses;

    public static Constants constants;

    public static string Label(string label)
    {
        return $"{label}:\n";
    }

    public static string Copy(char value, string target)
    {
        return Copy((int)value, target);
    }

    public static string Copy(char value, Register target)
    {
        return Copy((int)value, target);
    }

    public static string Copy(int value, string target)
    {
        return $"copy {value}, {target}";
    }

    public static string Copy(int value, Register target)
    {
        return $"copy {value}, {target}";
    }

    public static string Jump(string label)
    {
        return $"jump {label}";
    }

    public static string Jump(Register register)
    {
        return $"jump {register}";
    }

    public static string Jump_Eq(Register register, Register addr)
    {
        return $"jump_eq {register}, {addr}";
    }

    public static string Jump_Eq(Register register, string addr)
    {
        return $"jump_eq {register}, {addr}";
    }

    public static string Jump_Neq(Register register, string addr)
    {
        return $"jump_neq {register}, {addr}";
    }

    public static string Jump_Neq(Register register, Register addr)
    {
        return $"jump_neq {register}, {addr}";
    }

    public static string Jump_Gt(Register register, string addr)
    {
        return $"jump_gt {register}, {addr}";
    }

    public static string Jump_Gt(Register register, Register addr)
    {
        return $"jump_gt {register}, {addr}";
    }

    public static string Jump_Ge(Register register, string addr)
    {
        return $"jump_ge {register}, {addr}";
    }

    public static string Jump_Ge(Register register, Register addr)
    {
        return $"jump_ge {register}, {addr}";
    }

    public static string Jump_Lt(Register register, string addr)
    {
        return $"jump_lt {register}, {addr}";
    }

    public static string Jump_Lt(Register register, Register addr)
    {
        return $"jump_lt {register}, {addr}";
    }

    public static string Jump_Le(Register register, string addr)
    {
        return $"jump_le {register}, {addr}";
    }

    public static string Jump_Le(Register register, Register addr)
    {
        return $"jump_le {register}, {addr}";
    }

    public static string Push(Register register)
    {
        return $"push {register}";
    }

    public static string Push(int value)
    {
        return $"copy {value}, R0\npush R0";
    }

    public static string Push(char value)
    {
        return Push((int)value);
    }

    public static string Pop(Register register)
    {
        return $"pop {register}";
    }

    public static string Pop()
    {
        return "pop";
    }

    public static string Ret()
    {
        return "return";
    }

    public static string NoOp()
    {
        return "noop";
    }

    public static string Halt()
    {
        return "halt";
    }

    public static string Add(Register lhs, Register rhs, Register target)
    {
        return $"add {lhs}, {rhs}, {target}";
    }

    public static string Sub(Register lhs, Register rhs, Register target)
    {
        return $"sub {lhs}, {rhs}, {target}";
    }

    public static string Mult(Register lhs, Register rhs, Register high, Register low)
    {
        return $"add {lhs}, {rhs}, {high}, {low}";
    }

    public static string Divmod(Register lhs, Register rhs, Register result, Register remainder)
    {
        return $"divmod {lhs}, {rhs}, {result}, {remainder}";
    }

    public static string And(Register lhs, Register rhs, Register target)
    {
        return $"and {lhs}, {rhs}, {target}";
    }

    public static string Or(Register lhs, Register rhs, Register target)
    {
        return $"or {lhs}, {rhs}, {target}";
    }

    public static string Comp(Register lhs, Register rhs, Register target)
    {
        return $"comp {lhs}, {rhs}, {target}";
    }

    public static string Comp_Eq(Register lhs, Register rhs, Register target)
    {
        return $"comp_eq {lhs}, {rhs}, {target}";
    }

    public static string Comp_Neq(Register lhs, Register rhs, Register target)
    {
        return $"comp_neq {lhs}, {rhs}, {target}";
    }

    public static string Comp_Gt(Register lhs, Register rhs, Register target)
    {
        return $"comp_gt {lhs} {rhs}, {target}";
    }

    public static string Comp_Ge(Register lhs, Register rhs, Register target)
    {
        return $"comp_ge {lhs}, {rhs}, {target}";
    }

    public static string Comp_Lt(Register lhs, Register rhs, Register target)
    {
        return $"comp_lt {lhs}, {rhs}, {target}";
    }

    public static string Comp_Le(Register lhs, Register rhs, Register target)
    {
        return $"comp_le {lhs}, {rhs}, {target}";
    }
}