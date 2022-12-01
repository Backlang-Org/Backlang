using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Backlang.Driver.Compiling.Targets.Dotnet.Emitters;

internal class LoadIndirectEmitter : IEmitter
{
    public void Emit(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, Furesoft.Core.CodeDom.Compiler.Instruction instruction, BasicBlock block)
    {
        ilProcessor.Emit(OpCodes.Ldind_I4);
    }
}