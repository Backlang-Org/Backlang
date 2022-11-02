using Furesoft.Core.CodeDom.Compiler.Instructions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Backlang.Driver.Compiling.Targets.Dotnet.Emitters;

internal class DynamicCastEmitter : IEmitter
{
    public void Emit(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, Furesoft.Core.CodeDom.Compiler.Instruction instruction, BasicBlock block)
    {
        var dcp = (DynamicCastPrototype)instruction.Prototype;
        var checkType = assemblyDefinition.ImportType(dcp.TargetType.ElementType);

        ilProcessor.Emit(OpCodes.Isinst, checkType);
    }
}