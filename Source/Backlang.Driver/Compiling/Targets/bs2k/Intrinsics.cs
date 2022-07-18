namespace Backlang.Driver.Compiling.Targets.bs2k;

public static class Intrinsics
{
    public static string Push(string register)
    {
        return $"push {register}";
    }

    public static string Push(int value)
    {
        return $"copy R0, {value}\npush R0";
    }
}