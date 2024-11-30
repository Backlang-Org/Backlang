using Furesoft.Core.CodeDom.Compiler.Instructions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Instruction = Furesoft.Core.CodeDom.Compiler.Instruction;

namespace Backlang.Driver.Compiling.Targets.Dotnet.Emitters;

internal class DynamicCastEmitter : IEmitter
{
    public void Emit(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, Instruction instruction,
        BasicBlock block)
    {
        var dcp = (DynamicCastPrototype)instruction.Prototype;
        var checkType = assemblyDefinition.ImportType(dcp.TargetType.ElementType);

        ilProcessor.Emit(OpCodes.Isinst, checkType);
    }
}