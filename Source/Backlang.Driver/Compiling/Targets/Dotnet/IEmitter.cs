using Mono.Cecil;
using Mono.Cecil.Cil;
using Instruction = Furesoft.Core.CodeDom.Compiler.Instruction;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

internal interface IEmitter
{
    void Emit(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor,
        Instruction instruction, BasicBlock block);
}