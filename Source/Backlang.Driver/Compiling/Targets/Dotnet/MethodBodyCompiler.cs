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
                var load = item.PreviousInstructionOrNull;
                var constant = (ConstantPrototype)load.PreviousInstructionOrNull.Prototype;
                var method = typeof(Console).GetMethods().FirstOrDefault(_ =>
                   _.Name == "WriteLine" && _.GetParameters().Length == 1 && _.GetParameters()[0].ParameterType.Name == load.ResultType.Name.ToString()
                );

                ilProcessor.Append(Instruction.Create(OpCodes.Call,
                    _assemblyDefinition.MainModule.ImportReference(
                        method
                        )
                    ));
            }
            else if (i.Prototype is LoadPrototype ld)
            {
                var vP = (ConstantPrototype)item.PreviousInstructionOrNull.Prototype;
                dynamic v = vP.Value;

                if (vP.ResultType.Name.ToString() == "String")
                {
                    ilProcessor.Append(Instruction.Create(OpCodes.Ldstr, v.Value));
                }
                else if (vP.ResultType.Name.ToString() == "Boolean")
                {
                    ilProcessor.Append(Instruction.Create(!v.IsZero ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));
                }
            }
        }

        ilProcessor.Append(Instruction.Create(OpCodes.Ret));

        clrMethod.Body.MaxStackSize = 7;
    }
}