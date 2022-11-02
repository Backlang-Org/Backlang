using Furesoft.Core.CodeDom.Compiler.Instructions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Backlang.Driver.Compiling.Targets.Dotnet.Emitters;

internal class ArithmetikEmitter : IEmitter
{
    private readonly Dictionary<string, OpCode> _stringOPMap = new()
    {
        ["arith.+"] = OpCodes.Add,
        ["arith.-"] = OpCodes.Sub,
        ["arith.*"] = OpCodes.Mul,
        ["arith./"] = OpCodes.Div,

        ["arith.%"] = OpCodes.Div,

        ["arith.&"] = OpCodes.And,
        ["arith.|"] = OpCodes.Or,
        ["arith.^"] = OpCodes.Xor,

        ["arith.<"] = OpCodes.Clt,
        ["arith.>"] = OpCodes.Cgt,
        ["arith.=="] = OpCodes.Ceq,
    };

    public void Emit(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, Furesoft.Core.CodeDom.Compiler.Instruction instruction, BasicBlock block)
    {
        var arithProtype = (IntrinsicPrototype)instruction.Prototype;

        if (_stringOPMap.ContainsKey(arithProtype.Name))
        {
            var op = _stringOPMap[arithProtype.Name];
            ilProcessor.Emit(op); return;
        }

        switch (arithProtype.Name)
        {
            case "arith.!=":
                ilProcessor.Emit(OpCodes.Ceq);
                ilProcessor.Emit(OpCodes.Ldc_I4, 0);
                ilProcessor.Emit(OpCodes.Ceq);
                break;

            case "arith.<=":
                ilProcessor.Emit(OpCodes.Cgt);
                ilProcessor.Emit(OpCodes.Ldc_I4, 0);
                ilProcessor.Emit(OpCodes.Ceq);
                break;

            case "arith.>=":
                ilProcessor.Emit(OpCodes.Clt);
                ilProcessor.Emit(OpCodes.Ldc_I4, 0);
                ilProcessor.Emit(OpCodes.Ceq);
                break;
        }
    }
}