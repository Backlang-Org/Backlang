using Mono.Cecil;
using Mono.Cecil.Cil;
using Instruction = Furesoft.Core.CodeDom.Compiler.Instruction;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

public interface IEmitter
{
    void Emit(MethodDefinition clrMethod, AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor,
        Instruction instruction, FlowGraph implementation, BasicBlock block,
        NamedInstruction item, InstructionPrototype prototype);
}