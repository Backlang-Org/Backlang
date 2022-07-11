using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Flow;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

public static class MethodBodyCompiler
{
    public static void Compile(DescribedBodyMethod m, Mono.Cecil.MethodDefinition clrMethod, AssemblyDefinition assemblyDefinition)
    {
        var ilProcessor = clrMethod.Body.GetILProcessor();

        foreach (var item in m.Body.Implementation.NamedInstructions)
        {
            var i = item.Instruction;

            if (i.Prototype is CallPrototype callPrototype)
            {
                EmitCall(assemblyDefinition, ilProcessor, item);
            }
            else if (i.Prototype is LoadPrototype ld)
            {
                var consProto = (ConstantPrototype)item.PreviousInstructionOrNull.Prototype;

                EmitConstant(ilProcessor, consProto);
            }
            else if (i.Prototype is AllocaPrototype allocA)
            {
                EmitVariableDeclaration(clrMethod, assemblyDefinition, ilProcessor, item, allocA);
            }
        }

        if (m.Body.Implementation.EntryPoint.Flow is ReturnFlow rf)
        {
            EmitConstant(ilProcessor, (ConstantPrototype)rf.ReturnValue.Prototype);

            ilProcessor.Emit(OpCodes.Ret);
        }
        else if (m.Body.Implementation.EntryPoint.Flow is UnreachableFlow)
        {
            if (clrMethod.ReturnType.Name == "Void")
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Ret));
            }
            else
            {
                ilProcessor.Emit(OpCodes.Throw);
            }
        }

        //clrMethod.Body.MaxStackSize = 7;
    }

    private static void EmitCall(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, Furesoft.Core.CodeDom.Compiler.NamedInstruction item)
    {
        var load = item.PreviousInstructionOrNull;
        var method = GetPrintMethod(assemblyDefinition, load);

        foreach (var arg in method.Parameters)
        {
            if (arg.ParameterType.FullName == "System.Object")
            {
                ilProcessor.Emit(OpCodes.Box, assemblyDefinition.ImportType(load.ResultType));
            }
        }

        ilProcessor.Append(Instruction.Create(OpCodes.Call,
            assemblyDefinition.MainModule.ImportReference(
                method
                )
            ));
    }

    private static void EmitConstant(ILProcessor ilProcessor, ConstantPrototype consProto)
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

    private static void EmitVariableDeclaration(MethodDefinition clrMethod, AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, Furesoft.Core.CodeDom.Compiler.NamedInstruction item, AllocaPrototype allocA)
    {
        var elementType = assemblyDefinition.ImportType(allocA.ElementType);

        var variable =
            new VariableDefinition(assemblyDefinition.MainModule.ImportReference(elementType));
        clrMethod.Body.Variables.Add(variable);

        var store = item.NextInstructionOrNull?.Prototype;

        if (store is ConstantPrototype sp)
        {
            EmitConstant(ilProcessor, sp);
            ilProcessor.Append(Instruction.Create(OpCodes.Stloc, variable));

            clrMethod.Body.InitLocals = true;
        }
    }

    private static MethodReference GetPrintMethod(AssemblyDefinition assemblyDefinition, Furesoft.Core.CodeDom.Compiler.NamedInstruction load)
    {
        var callPrototype = (CallPrototype)load.NextInstructionOrNull.Prototype;

        var parentType = assemblyDefinition.ImportType(callPrototype.Callee.ParentType).Resolve();

        foreach (var method in parentType.Methods)
        {
            var parameters = method.Parameters;

            if (method.Name == callPrototype.Callee.Name.ToString())
            {
                if (MatchesParameters(parameters, load))
                {
                    return method;
                }
            }
        }

        return null;
    }

    private static bool MatchesParameters(Mono.Collections.Generic.Collection<ParameterDefinition> parameters, Furesoft.Core.CodeDom.Compiler.NamedInstruction load)
    {
        bool matches = false;
        for (int i = 0; i < parameters.Count; i++)
        {
            if (parameters[i].ParameterType.FullName == load.ResultType.FullName.ToString() || parameters[i].ParameterType.FullName == "System.Object")
            {
                matches = (matches || i == 0) && parameters[i].ParameterType.FullName == load.ResultType.FullName.ToString() || parameters[i].ParameterType.FullName == "System.Object";
            }
        }

        return matches;
    }
}