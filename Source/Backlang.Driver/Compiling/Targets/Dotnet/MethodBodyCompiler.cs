using Furesoft.Core.CodeDom.Compiler.Core.Constants;
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
                var consProto = (ConstantPrototype)item.PreviousInstructionOrNull.Prototype;

                AppendInstruction(ilProcessor, consProto);
            }
        }

        ilProcessor.Append(Instruction.Create(OpCodes.Ret));

        clrMethod.Body.MaxStackSize = 7;
    }

    private static void AppendInstruction(ILProcessor ilProcessor, ConstantPrototype consProto)
    {
        dynamic v = consProto.Value;

        if (consProto.ResultType.Name.ToString() == "String")
        {
            ilProcessor.Append(Instruction.Create(OpCodes.Ldstr, v.Value));
        }
        else if (consProto.ResultType.Name.ToString() == "Byte")
        {
            var ccc = (IntegerConstant)v;
            ilProcessor.Append(Instruction.Create(OpCodes.Ldc_I4, ccc.ToInt8()));
        }
        else if (consProto.ResultType.Name.ToString() == "Int16")
        {
            var ccc = (IntegerConstant)v;
            ilProcessor.Append(Instruction.Create(OpCodes.Ldc_I4, ccc.ToInt16()));
        }
        else if (consProto.ResultType.Name.ToString() == "Int32")
        {
            var ccc = (IntegerConstant)v;
            ilProcessor.Append(Instruction.Create(OpCodes.Ldc_I4, ccc.ToInt32()));
        }
        else if (consProto.ResultType.Name.ToString() == "Int64")
        {
            var ccc = (IntegerConstant)v;
            ilProcessor.Append(Instruction.Create(OpCodes.Ldc_I8, ccc.ToInt64()));
        }
        else if (consProto.ResultType.Name.ToString() == "Float")
        {
            var ccc = (Float32Constant)v;
            ilProcessor.Append(Instruction.Create(OpCodes.Ldc_R4, ccc.Value));
        }
        else if (consProto.ResultType.Name.ToString() == "Double")
        {
            var ccc = (Float64Constant)v;
            ilProcessor.Append(Instruction.Create(OpCodes.Ldc_R8, ccc.Value));
        }
        else if (consProto.ResultType.Name.ToString() == "Boolean")
        {
            ilProcessor.Append(Instruction.Create(!v.IsZero ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));
        }
    }
}