using Backlang.Driver.Core.Instructions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Backlang.Driver.Compiling.Targets.Dotnet.Emitters;

internal class TypeofEmitter : IEmitter
{
    public void Emit(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor,
        Furesoft.Core.CodeDom.Compiler.Instruction instruction, BasicBlock block)
    {
        var typePrototype = (TypeOfInstructionPrototype)instruction.Prototype;

        ilProcessor.Emit(OpCodes.Ldtoken, assemblyDefinition.ImportType(typePrototype.Type));
    }
}