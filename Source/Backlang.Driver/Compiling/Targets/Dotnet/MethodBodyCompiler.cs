using Backlang.Driver.Compiling.Targets.Dotnet.Emitters;
using Backlang.Driver.Core.Instructions;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Flow;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Mono.Cecil;
using Mono.Cecil.Cil;

using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;
using Instruction = Mono.Cecil.Cil.Instruction;
using MethodDefinition = Mono.Cecil.MethodDefinition;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

public static class MethodBodyCompiler
{
    private static readonly Dictionary<Type, IEmitter> emitters = new()
    {
        [typeof(CallPrototype)] = new CallEmitter(),
        [typeof(TypeOfInstructionPrototype)] = new TypeofEmitter(),
        [typeof(DynamicCastPrototype)] = new DynamicCastEmitter(),
        [typeof(LoadIndirectPrototype)] = new LoadIndirectEmitter(),
        [typeof(NewObjectPrototype)] = new NewObjectEmitter(),
        [typeof(IntrinsicPrototype)] = new ArithmetikEmitter(),
        [typeof(AllocaArrayPrototype)] = new NewArrayEmitter(),
        [typeof(LoadPrototype)] = new LoadEmitter(),
    };

    public static Dictionary<string, VariableDefinition> Compile(DescribedBodyMethod m, MethodDefinition clrMethod, AssemblyDefinition assemblyDefinition, TypeDefinition parentType)
    {
        var ilProcessor = clrMethod.Body.GetILProcessor();

        Intrinsics.iLProcessor = ilProcessor;

        var variables = new Dictionary<string, VariableDefinition>();

        var labels = new Dictionary<BasicBlockTag, int>();
        var fixups = new List<(int InstructionIndex, BasicBlockTag Target)>();

        foreach (var block in m.Body.Implementation.BasicBlocks)
        {
            CompileBlock(block, assemblyDefinition, ilProcessor, clrMethod, parentType,
                variables, fixups, labels);
        }

        FixJumps(ilProcessor, labels, fixups);

        clrMethod.Body.MaxStackSize = 7;

        return variables;
    }

    public static MethodReference GetMethod(AssemblyDefinition assemblyDefinition, IMethod method)
    {
        var parentType = assemblyDefinition.ImportType(method.ParentType).Resolve();

        foreach (var m in parentType.Methods
            .Where(_ => _.Name == method.Name.ToString()))
        {
            var parameters = m.Parameters;

            if (parameters.Count == method.Parameters.Count)
            {
                if (method.GenericParameters.Any())
                {
                    if (method.IsConstructor)
                    {
                        var dts = (DirectTypeSpecialization)GenericTypeMap.Cache[(method.FullName, method)];
                        var args = dts.GetRecursiveGenericArguments().Select(_ => assemblyDefinition.ImportType(_)).ToArray();
                        var genericType = assemblyDefinition.ImportType(dts);
                        var gctor = genericType.Resolve().Methods.FirstOrDefault(_ => _.Name == method.Name.ToString());

                        var mm = m.MakeHostInstanceGeneric(args);

                        return assemblyDefinition.MainModule.ImportReference(mm);
                    }

                    return m;
                }

                if (MatchesParameters(parameters, method))
                    return assemblyDefinition.MainModule.ImportReference(m);
            }
        }

        return null;
    }

    public static void EmitConstant(ILProcessor ilProcessor, ConstantPrototype consProto)
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
            EmitIntegerConstant(ilProcessor, v, ic);
        }
    }

    private static void FixJumps(ILProcessor ilProcessor, Dictionary<BasicBlockTag, int> labels, List<(int InstructionIndex, BasicBlockTag Target)> fixups)
    {
        foreach (var fixup in fixups)
        {
            var targetLabel = fixup.Target;
            var targetInstructionIndex = labels[targetLabel];
            var targetInstruction = ilProcessor.Body.Instructions[targetInstructionIndex];
            var instructionToFixup = ilProcessor.Body.Instructions[fixup.InstructionIndex];
            instructionToFixup.Operand = targetInstruction;
        }
    }

    private static void CompileBlock(BasicBlock block, AssemblyDefinition assemblyDefinition,
        ILProcessor ilProcessor, MethodDefinition clrMethod, TypeDefinition parentType,
         Dictionary<string, VariableDefinition> variables,
         List<(int InstructionIndex, BasicBlockTag Target)> fixups, Dictionary<BasicBlockTag, int> labels)
    {
        labels.Add(block.Tag, ilProcessor.Body.Instructions.Count);

        foreach (var item in block.NamedInstructions)
        {
            var instruction = item.Instruction;

            var prototypeType = instruction.Prototype.GetType();
            if (emitters.ContainsKey(prototypeType))
            {
                emitters[prototypeType].Emit(assemblyDefinition, ilProcessor, instruction, block);
            }
            else if (instruction.Prototype is LoadLocalAPrototype lda)
            {
                var definition = variables[lda.Parameter.Name.ToString()];
                ilProcessor.Emit(OpCodes.Ldloca_S, definition);
            }
            else if (instruction.Prototype is AllocaPrototype allocA)
            {
                var variable = EmitVariableDeclaration(clrMethod, assemblyDefinition, ilProcessor, item, allocA);

                variables.Add(item.Block.Parameters[variables.Count].Tag.Name, variable);
            }
            else if (instruction.Prototype is LoadArgPrototype larg)
            {
                EmitLoadArg(clrMethod, ilProcessor, parentType, larg);
            }
            else if (instruction.Prototype is LoadLocalPrototype lloc)
            {
                EmitLoadLocal(ilProcessor, lloc, variables);
            }
            else if (instruction.Prototype is LoadFieldPrototype fp)
            {
                EmitLoadField(parentType, ilProcessor, fp);
            }
            else if (instruction.Prototype is StoreFieldPointerPrototype sp)
            {
                EmitStoreField(parentType, ilProcessor, sp);
            }
        }

        EmitBlockFlow(block, ilProcessor, clrMethod, fixups);
    }

    private static void EmitBlockFlow(BasicBlock block, ILProcessor ilProcessor, MethodDefinition clrMethod, List<(int InstructionIndex, BasicBlockTag Target)> fixups)
    {
        if (block.Flow is ReturnFlow rf)
        {
            if (rf.HasReturnValue)
            {
                EmitConstant(ilProcessor, (ConstantPrototype)rf.ReturnValue.Prototype);
            }

            ilProcessor.Emit(OpCodes.Ret);
        }
        else if (block.Flow is JumpFlow node)
        {
            fixups.Add((ilProcessor.Body.Instructions.Count, node.Branch.Target));
            ilProcessor.Emit(OpCodes.Br, Instruction.Create(OpCodes.Nop));
        }
        else if (block.Flow is JumpConditionalFlow n)
        {
            fixups.Add((ilProcessor.Body.Instructions.Count, n.Branch.Target));

            OpCode op = OpCodes.Br;

            var selector = (ConditionalJumpKind)n.ConditionSelector;
            if (selector == ConditionalJumpKind.True)
            {
                op = OpCodes.Brtrue;
            }
            else if (selector == ConditionalJumpKind.Equals)
            {
                op = OpCodes.Beq;
            }
            else if (selector == ConditionalJumpKind.NotEquals)
            {
                op = OpCodes.Bne_Un;
            }

            ilProcessor.Emit(
                op, Instruction.Create(OpCodes.Nop)
            );
        }
        else if (block.Flow is UnreachableFlow)
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
    }

    private static void EmitLoadLocal(ILProcessor ilProcessor, LoadLocalPrototype lloc, Dictionary<string, VariableDefinition> variables)
    {
        var definition = variables[lloc.Parameter.Name.ToString()];
        ilProcessor.Emit(OpCodes.Ldloc, definition);
    }

    private static void EmitStoreField(TypeDefinition parentType, ILProcessor ilProcessor, StoreFieldPointerPrototype fp)
    {
        var field = parentType.Fields.FirstOrDefault(_ => _.Name == fp.Field.Name.ToString());

        ilProcessor.Emit(OpCodes.Stfld, field);
    }

    private static void EmitLoadField(TypeDefinition parentType, ILProcessor ilProcessor, LoadFieldPrototype fp)
    {
        var field = parentType.Fields.FirstOrDefault(_ => _.Name == fp.Field.Name.ToString());

        ilProcessor.Emit(OpCodes.Ldfld, field);
    }

    private static void EmitLoadArg(MethodDefinition clrMethod, ILProcessor ilProcessor, TypeReference parentType, LoadArgPrototype larg)
    {
        var param = clrMethod.Parameters.FirstOrDefault(_ => _.Name == larg.Parameter.Name.ToString());

        if (param != null)
        {
            var index = clrMethod.Parameters.IndexOf(param);

            if (!clrMethod.IsStatic) index++;

            ilProcessor.Emit(OpCodes.Ldarg, index);
        }
        else
        {
            var thisPtr = larg.Parameter.Type.Name.ToString() == parentType.Name.ToString(); //ToDo: fix namespacing

            if (thisPtr)
            {
                ilProcessor.Emit(OpCodes.Ldarg_0);
            }
        }
    }

    private static void EmitIntegerConstant(ILProcessor ilProcessor, dynamic v, IntegerConstant ic)
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

    private static VariableDefinition EmitVariableDeclaration(MethodDefinition clrMethod,
        AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, NamedInstruction item, AllocaPrototype allocA)
    {
        var elementType = assemblyDefinition.ImportType(allocA.ElementType);

        var variable =
            new VariableDefinition(assemblyDefinition.MainModule.ImportReference(elementType));
        clrMethod.Body.Variables.Add(variable);

        var store = item.Instruction.Prototype;

        if (store is AllocaPrototype)
        {
            ilProcessor.Emit(OpCodes.Stloc, variable);

            clrMethod.Body.InitLocals = true;
        }

        return variable;
    }

    private static bool MatchesParameters(Mono.Collections.Generic.Collection<ParameterDefinition> parameters, IMethod method)
    {
        //ToDo: refactor to improve code
        var methodParams = string.Join(',', method.Parameters.Select(_ => NormalizeTypename(_.Type?.FullName.ToString())));
        var monocecilParams = string.Join(',', parameters.Select(_ => _.ParameterType.FullName.ToString()));

        return methodParams.Equals(monocecilParams, StringComparison.Ordinal);
    }

    private static string NormalizeTypename(string str)
    {
        if (!string.IsNullOrEmpty(str) && str.StartsWith("."))
        {
            return str.Substring(1);
        }

        return str;
    }
}