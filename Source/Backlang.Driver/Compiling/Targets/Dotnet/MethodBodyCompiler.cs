using Furesoft.Core.CodeDom.Compiler;
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
    public static Dictionary<string, VariableDefinition> Compile(DescribedBodyMethod m, Mono.Cecil.MethodDefinition clrMethod, AssemblyDefinition assemblyDefinition, TypeDefinition parentType)
    {
        var ilProcessor = clrMethod.Body.GetILProcessor();

        var variables = new Dictionary<string, VariableDefinition>();
        //var variables = CompileBlock(m.Body.Implementation.EntryPoint, assemblyDefinition, ilProcessor, clrMethod, parentType);

        foreach (var block in m.Body.Implementation.BasicBlocks)
        {
            var blockLocals =
                 CompileBlock(block, assemblyDefinition, ilProcessor, clrMethod, parentType);

            foreach (var local in blockLocals)
            {
                variables.Add(local.Key, local.Value);
            }
        }

        clrMethod.Body.MaxStackSize = 7;

        return variables;
    }

    private static Dictionary<string, VariableDefinition> CompileBlock(BasicBlock block, AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, MethodDefinition clrMethod, TypeDefinition parentType)
    {
        var variables = new Dictionary<string, VariableDefinition>();

        foreach (var item in block.NamedInstructions)
        {
            var instruction = item.Instruction;

            if (instruction.Prototype is CallPrototype callPrototype)
            {
                EmitCall(assemblyDefinition, ilProcessor, instruction, block.Graph);
            }
            else if (instruction.Prototype is NewObjectPrototype newObjectPrototype)
            {
                EmitNewObject(assemblyDefinition, ilProcessor, newObjectPrototype);
            }
            else if (instruction.Prototype is LoadPrototype ld)
            {
                var valueInstruction = block.Graph.GetInstruction(instruction.Arguments[0]);
                EmitConstant(ilProcessor, (ConstantPrototype)valueInstruction.Prototype);
            }
            else if (instruction.Prototype is AllocaPrototype allocA)
            {
                var variable = EmitVariableDeclaration(clrMethod, assemblyDefinition, ilProcessor, item, allocA);

                variables.Add(item.Block.Parameters[variables.Count].Tag.Name, variable);
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
                EmitLoadLocal(clrMethod, ilProcessor, parentType, lloc, variables);
            }
            else if (instruction.Prototype is GetFieldPointerPrototype fp)
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
                EmitConstant(ilProcessor, (ConstantPrototype)rf.ReturnValue.Prototype);
            }

            ilProcessor.Emit(OpCodes.Ret);
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

        return variables;
    }

    private static void EmitLoadLocal(MethodDefinition clrMethod, ILProcessor ilProcessor, TypeDefinition parentType, LoadLocalPrototype lloc, Dictionary<string, VariableDefinition> variables)
    {
        var definition = variables[lloc.Parameter.Name.ToString()];
        ilProcessor.Emit(OpCodes.Ldloc, definition);
    }

    private static void EmitStoreField(TypeDefinition parentType, ILProcessor ilProcessor, StoreFieldPointerPrototype fp)
    {
        var field = parentType.Fields.FirstOrDefault(_ => _.Name == fp.Field.Name.ToString());

        ilProcessor.Emit(OpCodes.Stfld, field);
    }

    private static void EmitLoadField(TypeDefinition parentType, ILProcessor ilProcessor, GetFieldPointerPrototype fp)
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
            ilProcessor.Emit(OpCodes.Ldarg, index + 1);
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
        }
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

    private static VariableDefinition EmitVariableDeclaration(MethodDefinition clrMethod, AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, Furesoft.Core.CodeDom.Compiler.NamedInstruction item, AllocaPrototype allocA)
    {
        var elementType = assemblyDefinition.ImportType(allocA.ElementType);

        var variable =
            new VariableDefinition(assemblyDefinition.MainModule.ImportReference(elementType));
        clrMethod.Body.Variables.Add(variable);

        var store = item.Instruction.Prototype;

        if (store is AllocaPrototype sp)
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
                if (MatchesParameters(parameters, method))
                    return assemblyDefinition.MainModule.ImportReference(m);
            }
        }

        return null;
    }

    private static bool MatchesParameters(Mono.Collections.Generic.Collection<ParameterDefinition> parameters, IMethod method)
    {
        //ToDo: refactor to improve code
        var methodParams = string.Join(',', method.Parameters.Select(_ => _.Type.FullName.ToString()));
        var monocecilParams = string.Join(',', parameters.Select(_ => _.ParameterType.FullName.ToString()));

        return methodParams.Equals(monocecilParams);
    }
}