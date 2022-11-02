using Furesoft.Core.CodeDom.Compiler.Instructions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Backlang.Driver.Compiling.Targets.Dotnet.Emitters;

internal class ArithmetikEmitter : IEmitter
{
    public void Emit(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, Furesoft.Core.CodeDom.Compiler.Instruction instruction, BasicBlock block)
    {
        var arithProtype = (IntrinsicPrototype)instruction.Prototype;

        switch (arithProtype.Name)
        {
            case "arith.+":
                ilProcessor.Emit(OpCodes.Add); break;
            case "arith.-":
                ilProcessor.Emit(OpCodes.Sub); break;
            case "arith.*":
                ilProcessor.Emit(OpCodes.Mul); break;
            case "arith./":
                ilProcessor.Emit(OpCodes.Div); break;
            case "arith.%":
                ilProcessor.Emit(OpCodes.Rem); break;
            case "arith.&":
                ilProcessor.Emit(OpCodes.And); break;
            case "arith.|":
                ilProcessor.Emit(OpCodes.Or); break;
            case "arith.^":
                ilProcessor.Emit(OpCodes.Xor); break;
            case "arith.==":
                ilProcessor.Emit(OpCodes.Ceq); break;
            case "arith.!=":
                ilProcessor.Emit(OpCodes.Ceq);
                ilProcessor.Emit(OpCodes.Ldc_I4, 0);
                ilProcessor.Emit(OpCodes.Ceq);
                break;

            case "arith.<":
                ilProcessor.Emit(OpCodes.Clt); break;
            case "arith.<=":
                ilProcessor.Emit(OpCodes.Cgt);
                ilProcessor.Emit(OpCodes.Ldc_I4, 0);
                ilProcessor.Emit(OpCodes.Ceq);
                break;

            case "arith.>":
                ilProcessor.Emit(OpCodes.Cgt); break;
            case "arith.>=":
                ilProcessor.Emit(OpCodes.Clt);
                ilProcessor.Emit(OpCodes.Ldc_I4, 0);
                ilProcessor.Emit(OpCodes.Ceq);
                break;
        }
    }
}