using Mono.Cecil.Cil;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

public static class Intrinsics
{
    public static ILProcessor iLProcessor;

    public static void Ldnull() => iLProcessor.Emit(OpCodes.Ldnull);

    public static void Ldstr(string value) => iLProcessor.Emit(OpCodes.Ldstr, value);

    public static void Ldc_I4(int value) => iLProcessor.Emit(OpCodes.Ldc_I4, value);

    public static void Ldc_I8(long value) => iLProcessor.Emit(OpCodes.Ldc_I8, value);

    public static void Ldc_R4(float value) => iLProcessor.Emit(OpCodes.Ldc_R4, value);

    public static void Ldc_R8(double value) => iLProcessor.Emit(OpCodes.Ldc_R8, value);

    public static void Ldarg(ushort arg) => iLProcessor.Emit(OpCodes.Ldarg, arg);

    public static void Ldarga(ushort arg) => iLProcessor.Emit(OpCodes.Ldarga, arg);

    public static void Ldloc(ushort local) => iLProcessor.Emit(OpCodes.Ldloc, local);

    public static void Ldloca(ushort local) => iLProcessor.Emit(OpCodes.Ldloca, local);
}