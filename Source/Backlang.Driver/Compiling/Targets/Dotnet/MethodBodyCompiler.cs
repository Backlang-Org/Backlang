using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

public static class MethodBodyCompiler
{
    public static void Compile(DescribedBodyMethod m, Mono.Cecil.MethodDefinition clrMethod, AssemblyDefinition _assemblyDefinition)
    {
        var ilProcessor = clrMethod.Body.GetILProcessor();

        foreach (var item in m.Body.Implementation.NamedInstructions)
        {
            var i = item.Instruction;

            if (i.Prototype is CallPrototype cp)
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Call, _assemblyDefinition.MainModule.ImportReference(typeof(Console).GetMethods().FirstOrDefault(_ => _.GetParameters().Length == 1 && _.GetParameters()[0].ParameterType.Name == "String"))));
            }
            else if (i.Prototype is LoadPrototype ld)
            {
                var vP = (ConstantPrototype)item.PreviousInstructionOrNull.Prototype;
                dynamic v = vP.Value;

                ilProcessor.Append(Instruction.Create(OpCodes.Ldstr, v.Value));
            }
        }

        ilProcessor.Append(Instruction.Create(OpCodes.Ret));

        clrMethod.Body.MaxStackSize = 7;
    }
}