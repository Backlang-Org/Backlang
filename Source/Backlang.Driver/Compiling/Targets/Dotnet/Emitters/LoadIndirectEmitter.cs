using Mono.Cecil;
using Mono.Cecil.Cil;
using Instruction = Furesoft.Core.CodeDom.Compiler.Instruction;

namespace Backlang.Driver.Compiling.Targets.Dotnet.Emitters;

internal class LoadIndirectEmitter : IEmitter
{
    public void Emit(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, Instruction instruction,
        BasicBlock block)
    {
        ilProcessor.Emit(OpCodes.Ldind_I4);
    }
}