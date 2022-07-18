namespace Backlang.Driver.Compiling.Targets.bs2k;

public static class Intrinsics
{
    public static string Push(string register)
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
}