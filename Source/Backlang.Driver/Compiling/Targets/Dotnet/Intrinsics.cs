using Mono.Cecil.Cil;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

public static class Intrinsics
{
    public static ILProcessor iLProcessor;

    public static void Ld_Null()
    {
        iLProcessor.Emit(OpCodes.Ldnull);
    }
}