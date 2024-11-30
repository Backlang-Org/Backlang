using Furesoft.Core.CodeDom.Compiler.Instructions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Instruction = Furesoft.Core.CodeDom.Compiler.Instruction;

namespace Backlang.Driver.Compiling.Targets.Dotnet.Emitters;

internal class LoadEmitter : IEmitter
{
    public void Emit(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, Instruction instruction,
        BasicBlock block)
    {
        var valueInstruction = block.Graph.GetInstruction(instruction.Arguments[0]);

        MethodBodyCompiler.EmitConstant(ilProcessor, (ConstantPrototype)valueInstruction.Prototype);
    }
}