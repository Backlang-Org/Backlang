using Furesoft.Core.CodeDom.Compiler.Instructions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Instruction = Furesoft.Core.CodeDom.Compiler.Instruction;

namespace Backlang.Driver.Compiling.Targets.Dotnet.Emitters;

internal class NewObjectEmitter : IEmitter
{
    public void Emit(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, Instruction instruction,
        BasicBlock block)
    {
        var newObjectPrototype = (NewObjectPrototype)instruction.Prototype;
        var method = MethodBodyCompiler.GetMethod(assemblyDefinition, newObjectPrototype.Constructor);

        ilProcessor.Emit(OpCodes.Newobj, method);
    }
}