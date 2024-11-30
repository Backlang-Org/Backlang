using Backlang.Driver.Core.Instructions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Instruction = Furesoft.Core.CodeDom.Compiler.Instruction;

namespace Backlang.Driver.Compiling.Targets.Dotnet.Emitters;

internal class TypeofEmitter : IEmitter
{
    public void Emit(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor,
        Instruction instruction, BasicBlock block)
    {
        var toip = (TypeOfInstructionPrototype)instruction.Prototype;

        ilProcessor.Emit(OpCodes.Ldtoken, assemblyDefinition.ImportType(toip.Type));
    }
}