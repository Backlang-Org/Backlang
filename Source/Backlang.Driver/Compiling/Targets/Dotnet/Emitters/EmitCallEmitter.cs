
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Instruction = Furesoft.Core.CodeDom.Compiler.Instruction;

namespace Backlang.Driver.Compiling.Targets.Dotnet.Emitters;

public class EmitCallEmitter : IEmitter
{
    public void Emit(MethodDefinition clrMethod, AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor,
        Instruction instruction, FlowGraph implementation, BasicBlock block,
        NamedInstruction item, InstructionPrototype prototype)
    {
        var callPrototype = (CallPrototype)prototype;

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
    
    private static bool MatchesParameters(Mono.Collections.Generic.Collection<ParameterDefinition> parameters, IMethod method)
    {
        //ToDo: refactor to improve code
        var methodParams = string.Join(',', method.Parameters.Select(_ => NormalizeTypename(_.Type.FullName.ToString())));
        var monocecilParams = string.Join(',', parameters.Select(_ => _.ParameterType.FullName.ToString()));

        return methodParams.Equals(monocecilParams, StringComparison.Ordinal);
    }

    private static string NormalizeTypename(string str)
    {
        if (str.StartsWith("."))
        {
            return str.Substring(1);
        }

        return str;
    }
    
    public static MethodReference GetMethod(AssemblyDefinition assemblyDefinition, IMethod method)
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
}