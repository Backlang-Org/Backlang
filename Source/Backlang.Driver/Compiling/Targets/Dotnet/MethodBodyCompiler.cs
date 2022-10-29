using Backlang.Driver.Core.Instructions;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Flow;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Mono.Cecil;
using Mono.Cecil.Cil;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;
using Instruction = Mono.Cecil.Cil.Instruction;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

internal class NothingFlow : BlockFlow
{
    public override IReadOnlyList<Furesoft.Core.CodeDom.Compiler.Instruction> Instructions => throw new NotImplementedException();

    public override IReadOnlyList<Branch> Branches => throw new NotImplementedException();

    public override InstructionBuilder GetInstructionBuilder(BasicBlockBuilder block, int instructionIndex)
    {
        throw new NotImplementedException();
    }

    public override BlockFlow WithBranches(IReadOnlyList<Branch> branches)
    {
        throw new NotImplementedException();
    }

    public override BlockFlow WithInstructions(IReadOnlyList<Furesoft.Core.CodeDom.Compiler.Instruction> instructions)
    {
        throw new NotImplementedException();
    }
}

public static class MethodBodyCompiler
{
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

            if (instruction.Prototype is CallPrototype)
            {
                EmitCall(assemblyDefinition, ilProcessor, instruction, block.Graph, block);
            }
            else if (instruction.Prototype is TypeOfInstructionPrototype toip)
            {
                ilProcessor.Emit(OpCodes.Ldtoken, assemblyDefinition.ImportType(toip.Type));
            }
            else if (instruction.Prototype is LoadLocalAPrototype lda)
            {
                var definition = variables[lda.Parameter.Name.ToString()];
                ilProcessor.Emit(OpCodes.Ldloca_S, definition);
            }
            else if (instruction.Prototype is LoadIndirectPrototype ldi)
            {
                ilProcessor.Emit(OpCodes.Ldind_I4);
            }
            else if (instruction.Prototype is NewObjectPrototype newObjectPrototype)
            {
                EmitNewObject(assemblyDefinition, ilProcessor, newObjectPrototype);
            }
            else if (instruction.Prototype is LoadPrototype)
            {
                var valueInstruction = block.Graph.GetInstruction(instruction.Arguments[0]);
                EmitConstant(assemblyDefinition, ilProcessor, (ConstantPrototype)valueInstruction.Prototype);
            }
            else if (instruction.Prototype is AllocaPrototype allocA)
            {
                var variable = EmitVariableDeclaration(clrMethod, assemblyDefinition, ilProcessor, item, allocA);

                variables.Add(item.Block.Parameters[variables.Count].Tag.Name, variable);
            }
            else if (instruction.Prototype is AllocaArrayPrototype allocArray)
            {
                ilProcessor.Emit(OpCodes.Newarr, assemblyDefinition.ImportType(allocArray.ElementType));
            }
            else if (instruction.Prototype is IntrinsicPrototype arith)
            {
                EmitArithmetic(ilProcessor, arith);
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

        if (block.Flow is ReturnFlow rf)
        {
            if (rf.HasReturnValue)
            {
                EmitConstant(assemblyDefinition, ilProcessor, (ConstantPrototype)rf.ReturnValue.Prototype);
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
            ilProcessor.Emit(
                (ConditionalJumpKind)n.ConditionSelector == ConditionalJumpKind.True ?
                OpCodes.Brtrue : OpCodes.Br, Instruction.Create(OpCodes.Nop)
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

    private static void EmitArithmetic(ILProcessor ilProcessor, IntrinsicPrototype arith)
    {
        switch (arith.Name)
        {
            case "arith.+":
                ilProcessor.Emit(OpCodes.Add); break;
            case "arith.-":
                ilProcessor.Emit(OpCodes.Sub); break;
            case "arith.*":
                ilProcessor.Emit(OpCodes.Mul); break;
            case "arith./":
                ilProcessor.Emit(OpCodes.Div); break;
            case "arith.%":
                ilProcessor.Emit(OpCodes.Rem); break;
            case "arith.&":
                ilProcessor.Emit(OpCodes.And); break;
            case "arith.|":
                ilProcessor.Emit(OpCodes.Or); break;
            case "arith.^":
                ilProcessor.Emit(OpCodes.Xor); break;
            case "arith.==":
                ilProcessor.Emit(OpCodes.Ceq); break;
            case "arith.!=":
                ilProcessor.Emit(OpCodes.Ceq);
                ilProcessor.Emit(OpCodes.Ldc_I4, 0);
                ilProcessor.Emit(OpCodes.Ceq);
                break;

            case "arith.<":
                ilProcessor.Emit(OpCodes.Clt); break;
            case "arith.<=":
                ilProcessor.Emit(OpCodes.Cgt);
                ilProcessor.Emit(OpCodes.Ldc_I4, 0);
                ilProcessor.Emit(OpCodes.Ceq);
                break;

            case "arith.>":
                ilProcessor.Emit(OpCodes.Cgt); break;
            case "arith.>=":
                ilProcessor.Emit(OpCodes.Clt);
                ilProcessor.Emit(OpCodes.Ldc_I4, 0);
                ilProcessor.Emit(OpCodes.Ceq);
                break;
        }
    }

    private static void EmitNewObject(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, NewObjectPrototype newObjectPrototype)
    {
        var method = GetMethod(assemblyDefinition, newObjectPrototype.Constructor);

        ilProcessor.Emit(OpCodes.Newobj, method);
    }

    private static void EmitCall(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, Furesoft.Core.CodeDom.Compiler.Instruction instruction, FlowGraph implementation, BasicBlock block)
    {
        var callPrototype = (CallPrototype)instruction.Prototype;

        if (IntrinsicHelper.IsIntrinsicType(typeof(Intrinsics), callPrototype))
        {
            IntrinsicHelper.InvokeIntrinsic(typeof(Intrinsics), callPrototype.Callee, instruction, block);
            return;
        }

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

    private static void EmitConstant(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, ConstantPrototype consProto)
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

    private static MethodReference GetMethod(AssemblyDefinition assemblyDefinition, IMethod method)
    {
        var parentType = assemblyDefinition.ImportType(method.ParentType).Resolve();

        foreach (var m in parentType.Methods.Where(_ => _.Name == method.Name.ToString()))
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