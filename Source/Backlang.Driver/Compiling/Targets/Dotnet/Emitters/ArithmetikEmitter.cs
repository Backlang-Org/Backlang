using Furesoft.Core.CodeDom.Compiler.Instructions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Backlang.Driver.Compiling.Targets.Dotnet.Emitters;

internal class ArithmetikEmitter : IEmitter
{
    private readonly ImmutableDictionary<string, OpCode> _stringOPMap = new Dictionary<string, OpCode>()
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
    }.ToImmutableDictionary();

    public void Emit(AssemblyDefinition assemblyDefinition, ILProcessor ilProcessor, Furesoft.Core.CodeDom.Compiler.Instruction instruction, BasicBlock block)
    {
        var arithProtype = (IntrinsicPrototype)instruction.Prototype;

        if (_stringOPMap.TryGetValue(arithProtype.Name, out var opCode))
        {
            ilProcessor.Emit(opCode); return;
        }

        switch (arithProtype.Name)
        {
            case "arith.!=":
                AppendComparison(ilProcessor, OpCodes.Ceq);
                break;

            case "arith.<=":
                AppendComparison(ilProcessor, OpCodes.Cgt);
                break;

            case "arith.>=":
                AppendComparison(ilProcessor, OpCodes.Clt);
                break;
        }
    }

    private static void AppendComparison(ILProcessor ilProcessor, OpCode comparisionOpcode)
    {
        ilProcessor.Emit(comparisionOpcode);
        ilProcessor.Emit(OpCodes.Ldc_I4, 0);
        ilProcessor.Emit(OpCodes.Ceq);
    }
}