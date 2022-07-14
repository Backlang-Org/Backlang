using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Flow;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

public static class MethodBodyCompiler
{
    public static List<(string name, VariableDefinition definition)> Compile(DescribedBodyMethod m, Mono.Cecil.MethodDefinition clrMethod, AssemblyDefinition assemblyDefinition)
    {
        var ilProcessor = clrMethod.Body.GetILProcessor();
        var variables = new List<(string name, VariableDefinition definition)>();

        foreach (var item in m.Body.Implementation.NamedInstructions)
        {
            var instruction = item.Instruction;

            if (instruction.Prototype is CallPrototype callPrototype)
            {
                EmitCall(assemblyDefinition, ilProcessor, instruction, m.Body.Implementation);
            }
            else if (instruction.Prototype is NewObjectPrototype newObjectPrototype)
            {
                EmitNewObject(assemblyDefinition, ilProcessor, newObjectPrototype);
            }
            else if (instruction.Prototype is LoadPrototype ld)
            {
                var consProto = (ConstantPrototype)item.PreviousInstructionOrNull.Prototype;

                EmitConstant(ilProcessor, consProto);
            }
            else if (instruction.Prototype is AllocaPrototype allocA)
            {
                var variable = EmitVariableDeclaration(clrMethod, assemblyDefinition, ilProcessor, item, allocA);

                variables.Add(variable);
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
                ilProcessor.Emit(OpCodes.Ret);
            }
            else
            {
                ilProcessor.Emit(OpCodes.Throw);
            }
        }

        clrMethod.Body.MaxStackSize = 7;

        return variables;
    }

    private static void EmitNewObject(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, NewObjectPrototype newObjectPrototype)
    {
        var method = GetMethod(assemblyDefinition, newObjectPrototype.Constructor);

        ilProcessor.Emit(OpCodes.Newobj, method);
    }

    private static void EmitCall(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, Furesoft.Core.CodeDom.Compiler.Instruction instruction, Furesoft.Core.CodeDom.Compiler.FlowGraph implementation)
    {
        var callPrototype = (CallPrototype)instruction.Prototype;

        var method = GetMethod(assemblyDefinition, callPrototype.Callee);

        for (var i = 0; i < method.Parameters.Count; i++)
        {
            var valueType = implementation.NamedInstructions
                .Where(_ => instruction.Arguments[i] == _.Tag)
                .Select(_ => _.ResultType).FirstOrDefault();

            var arg = method.Parameters[i];
            if (arg.ParameterType.FullName.ToString() == "System.Object")
            {
                ilProcessor.Emit(OpCodes.Box, assemblyDefinition.ImportType(valueType));
            }
        }

        ilProcessor.Emit(OpCodes.Call,
            assemblyDefinition.MainModule.ImportReference(
                method
                )
            );
    }

    private static void EmitConstant(ILProcessor ilProcessor, ConstantPrototype consProto)
    {
        dynamic v = consProto.Value;

        if (v is StringConstant str)
        {
            ilProcessor.Emit(OpCodes.Ldstr, str.Value);
        }
        else if (v is Float32Constant f32)
        {
            ilProcessor.Emit(OpCodes.Ldc_R4, f32.Value);
        }
        else if (v is Float64Constant f64)
        {
            ilProcessor.Emit(OpCodes.Ldc_R8, f64.Value);
        }
        else if (v is NullConstant)
        {
            ilProcessor.Emit(OpCodes.Ldnull);
        }
        else if (v is IntegerConstant ic)
        {
            switch (ic.Spec.Size)
            {
                case 1:
                    ilProcessor.Emit(!v.IsZero ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                    break;

                case 8:
                    if (ic.Spec.IsSigned)
                    {
                        ilProcessor.Emit(OpCodes.Ldc_I4, ic.ToInt8());
                    }
                    else
                    {
                        ilProcessor.Emit(OpCodes.Ldc_I4, ic.ToUInt8());
                    }
                    break;

                case 16:
                    if (ic.Spec.IsSigned)
                    {
                        ilProcessor.Emit(OpCodes.Ldc_I4, ic.ToInt16());
                    }
                    else
                    {
                        ilProcessor.Emit(OpCodes.Ldc_I4, ic.ToUInt16());
                    }
                    break;

                case 32:
                    if (ic.Spec.IsSigned)
                    {
                        ilProcessor.Emit(OpCodes.Ldc_I4, ic.ToInt32());
                    }
                    else
                    {
                        ilProcessor.Emit(OpCodes.Ldc_I4, ic.ToUInt32());
                    }
                    break;

                case 64:
                    if (ic.Spec.IsSigned)
                    {
                        ilProcessor.Emit(OpCodes.Ldc_I8, ic.ToInt64());
                    }
                    else
                    {
                        ilProcessor.Emit(OpCodes.Ldc_I4, ic.ToUInt64());
                    }
                    break;

                default:
                    break;
            }
        }
    }

    private static (string name, VariableDefinition definition) EmitVariableDeclaration(MethodDefinition clrMethod, AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, Furesoft.Core.CodeDom.Compiler.NamedInstruction item, AllocaPrototype allocA)
    {
        var elementType = assemblyDefinition.ImportType(allocA.ElementType);

        var variable =
            new VariableDefinition(assemblyDefinition.MainModule.ImportReference(elementType));
        clrMethod.Body.Variables.Add(variable);

        var store = item.NextInstructionOrNull?.Prototype;

        if (store is ConstantPrototype sp)
        {
            EmitConstant(ilProcessor, sp);
            ilProcessor.Emit(OpCodes.Stloc, variable);

            clrMethod.Body.InitLocals = true;
        }

        return (item.Block.Parameters[0].Tag.Name, variable);
    }

    private static MethodReference GetMethod(AssemblyDefinition assemblyDefinition, IMethod method)
    {
        var parentType = assemblyDefinition.ImportType(method.ParentType).Resolve();

        foreach (var m in parentType.Methods)
        {
            var parameters = m.Parameters;

            if (m.Name == method.Name.ToString())
            {
                if (parameters.Count == method.Parameters.Count)
                {
                    if (MatchesParameters(parameters, method))
                        return assemblyDefinition.MainModule.ImportReference(m);
                }
            }
        }

        return null;
    }

    private static bool MatchesParameters(Mono.Collections.Generic.Collection<ParameterDefinition> parameters, IMethod method)
    {
        bool matches = false;
        for (int i = 0; i < parameters.Count; i++)
        {
            if (parameters[i].ParameterType.FullName == method.Parameters[i].Type.FullName.ToString())
            {
                matches = (matches || i == 0) && parameters[i].ParameterType.FullName == method.Parameters[i].Type.FullName.ToString();
            }
        }

        return matches;
    }
}