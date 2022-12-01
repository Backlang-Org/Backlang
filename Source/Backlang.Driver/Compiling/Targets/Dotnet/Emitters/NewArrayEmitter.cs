using Furesoft.Core.CodeDom.Compiler.Instructions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Backlang.Driver.Compiling.Targets.Dotnet.Emitters;

internal class NewArrayEmitter : IEmitter
{
    public void Emit(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, Furesoft.Core.CodeDom.Compiler.Instruction instruction, BasicBlock block)
    {
        var prototype = (AllocaArrayPrototype)instruction.Prototype;

        ilProcessor.Emit(OpCodes.Newarr, assemblyDefinition.ImportType(prototype.ElementType));
    }
}