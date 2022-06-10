using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

public static class MethodBodyCompiler
{
    public static void Compile(DescribedBodyMethod m, Mono.Cecil.MethodDefinition clrMethod, AssemblyDefinition _assemblyDefinition)
    {
        var ilProcessor = clrMethod.Body.GetILProcessor();

        foreach (var item in m.Body.Implementation.NamedInstructions)
        {
            var i = item.Instruction;

            if (i.Prototype is CallPrototype callPrototype)
            {
                var load = item.PreviousInstructionOrNull;
                var constant = (ConstantPrototype)load.PreviousInstructionOrNull.Prototype;
                var method = GetPrintMethod(load);

                ilProcessor.Append(Instruction.Create(OpCodes.Call,
                    _assemblyDefinition.MainModule.ImportReference(
                        method
                        )
                    ));
            }
            else if (i.Prototype is LoadPrototype ld)
            {
                var consProto = (ConstantPrototype)item.PreviousInstructionOrNull.Prototype;

                AppendConstant(ilProcessor, consProto);
            }
        }

        ilProcessor.Append(Instruction.Create(OpCodes.Ret));

        clrMethod.Body.MaxStackSize = 7;
    }

    private static void AppendConstant(ILProcessor ilProcessor, ConstantPrototype consProto)
    {
        dynamic v = consProto.Value;

        if (v is StringConstant str)
        {
            ilProcessor.Append(Instruction.Create(OpCodes.Ldstr, str.Value));
        }
        else if (v is Float32Constant f32)
        {
            ilProcessor.Append(Instruction.Create(OpCodes.Ldc_R4, f32.Value));
        }
        else if (v is Float64Constant f64)
        {
            ilProcessor.Append(Instruction.Create(OpCodes.Ldc_R8, f64.Value));
        }
        else if (v is IntegerConstant ic)
        {
            switch (ic.Spec.Size)
            {
                case 1:
                    ilProcessor.Append(Instruction.Create(!v.IsZero ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));
                    break;

                case 8:
                    if (ic.Spec.IsSigned)
                    {
                        ilProcessor.Append(Instruction.Create(OpCodes.Ldc_I4, ic.ToInt8()));
                    }
                    else
                    {
                        ilProcessor.Append(Instruction.Create(OpCodes.Ldc_I4, ic.ToUInt8()));
                    }
                    break;

                case 16:
                    if (ic.Spec.IsSigned)
                    {
                        ilProcessor.Append(Instruction.Create(OpCodes.Ldc_I4, ic.ToInt16()));
                    }
                    else
                    {
                        ilProcessor.Append(Instruction.Create(OpCodes.Ldc_I4, ic.ToUInt16()));
                    }
                    break;

                case 32:
                    if (ic.Spec.IsSigned)
                    {
                        ilProcessor.Append(Instruction.Create(OpCodes.Ldc_I4, ic.ToInt32()));
                    }
                    else
                    {
                        ilProcessor.Append(Instruction.Create(OpCodes.Ldc_I4, ic.ToUInt32()));
                    }
                    break;

                case 64:
                    if (ic.Spec.IsSigned)
                    {
                        ilProcessor.Append(Instruction.Create(OpCodes.Ldc_I8, ic.ToInt64()));
                    }
                    else
                    {
                        ilProcessor.Append(Instruction.Create(OpCodes.Ldc_I4, ic.ToUInt64()));
                    }
                    break;

                default:
                    break;
            }
        }
    }

    private static MethodInfo GetPrintMethod(Furesoft.Core.CodeDom.Compiler.NamedInstruction load)
    {
        var callPrototype = (CallPrototype)load.NextInstructionOrNull.Prototype;

        foreach (var method in typeof(Console).GetMethods())
        {
            var parameters = method.GetParameters();

            if (method.Name == callPrototype.Callee.Name.ToString() && parameters.Length == 1)
            {
                if (MatchesParameters(parameters, load))
                {
                    return method;
                }
            }
        }

        return null;
    }

    private static bool MatchesParameters(ParameterInfo[] parameters, Furesoft.Core.CodeDom.Compiler.NamedInstruction load)
    {
        bool matches = false;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].ParameterType.Name == load.ResultType.Name.ToString())
            {
                matches = (matches || i == 0) && parameters[i].ParameterType.Name == load.ResultType.Name.ToString();
            }
        }

        return matches;
    }
}