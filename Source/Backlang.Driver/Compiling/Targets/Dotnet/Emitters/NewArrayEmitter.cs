using Furesoft.Core.CodeDom.Compiler.Instructions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Instruction = Furesoft.Core.CodeDom.Compiler.Instruction;

namespace Backlang.Driver.Compiling.Targets.Dotnet.Emitters;

internal class NewArrayEmitter : IEmitter
{
    public void Emit(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, Instruction instruction,
        BasicBlock block)
    {
        var prototype = (AllocaArrayPrototype)instruction.Prototype;

        ilProcessor.Emit(OpCodes.Newarr, assemblyDefinition.ImportType(prototype.ElementType));
    }
}