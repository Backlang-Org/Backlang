using Furesoft.Core.CodeDom.Compiler.Instructions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Backlang.Driver.Compiling.Targets.Dotnet.Emitters;

internal class CallEmitter : IEmitter
{
    public void Emit(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor,
        Furesoft.Core.CodeDom.Compiler.Instruction instruction, BasicBlock block)
    {
        var callPrototype = (CallPrototype)instruction.Prototype;

        if (IntrinsicHelper.IsIntrinsicType(typeof(Intrinsics), callPrototype))
        {
            IntrinsicHelper.InvokeIntrinsic(typeof(Intrinsics), callPrototype.Callee, instruction, block);
            return;
        }

        var method = MethodBodyCompiler.GetMethod(assemblyDefinition, callPrototype.Callee);

        for (var i = 0; i < method.Parameters.Count; i++)
        {
            var valueType = block.Graph.NamedInstructions
                .Where(_ => instruction.Arguments[i] == _.Tag)
                .Select(_ => _.ResultType).FirstOrDefault();

            var arg = method.Parameters[i];

            //ToDo: move to IR
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
}