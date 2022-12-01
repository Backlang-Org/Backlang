using Furesoft.Core.CodeDom.Compiler.Instructions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Backlang.Driver.Compiling.Targets.Dotnet.Emitters;

internal class LoadEmitter : IEmitter
{
    public void Emit(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, Furesoft.Core.CodeDom.Compiler.Instruction instruction, BasicBlock block)
    {
        var valueInstruction = block.Graph.GetInstruction(instruction.Arguments[0]);

        MethodBodyCompiler.EmitConstant(ilProcessor, (ConstantPrototype)valueInstruction.Prototype);
    }
}