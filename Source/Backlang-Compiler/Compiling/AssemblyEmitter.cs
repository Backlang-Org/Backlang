using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Expressions;
using Backlang.Codeanalysis.Parsing.AST.Statements.Assembler;

namespace Backlang_Compiler.Compiling;

public class AssemblyEmitter
{
    public byte[] Emit(AssemblerBlockStatement node)
    {
        var emitter = new Emitter();

        foreach (var bodynode in node.Body)
        {
            if (bodynode is Instruction instruction)
            {
                EmitInstruction(instruction, emitter);
            }
        }

        return emitter.Result;
    }

    private static uint ConvertNumber(LiteralNode lit)
    {
        var literalBytes = BitConverter.GetBytes((long)lit.Value);

        var value = BitConverter.ToUInt32(literalBytes);
        return value;
    }

    private void EmitAdd(Emitter emitter, Instruction instruction)
    {
        var first = instruction.Arguments[0];
        var second = instruction.Arguments[1];
        var third = instruction.Arguments[2];

        if (first is RegisterReferenceExpression target && second is RegisterReferenceExpression source && third is Expression imm)
        {
            var targetRegister = GetRegisterIndex(target);
            var sourceRegister = GetRegisterIndex(source);
            var value = EvaluateExpression(imm);

            emitter.Add(targetRegister, sourceRegister, value);
        }
        else if (first is RegisterReferenceExpression target2 && second is RegisterReferenceExpression lhs && third is RegisterReferenceExpression rhs)
        {
            var targetRegister = GetRegisterIndex(target2);
            var lhsRegister = GetRegisterIndex(lhs);
            var rhsRegsiter = GetRegisterIndex(rhs);

            emitter.Add(targetRegister, lhsRegister, rhsRegsiter);
        }
    }

    private void EmitGks(Instruction instruction, Emitter emitter)
    {
        var first = instruction.Arguments[0];
        var second = instruction.Arguments[1];

        if (first is RegisterReferenceExpression target && second is RegisterReferenceExpression keycode)
        {
            var targetRegister = GetRegisterIndex(target);
            var keycodeRegister = GetRegisterIndex(keycode);

            emitter.GetKeyState(targetRegister, keycodeRegister);
        }
    }

    private void EmitInstruction(Instruction instruction, Emitter emitter)
    {
        var opcode = Enum.Parse<OpCode>(instruction.OpCode, true);

        switch (opcode)
        {
            case OpCode.Mov:
                EmitMove(emitter, instruction);
                break;

            case OpCode.Hlt:
                emitter.EmitHalt();
                break;

            case OpCode.Add:
                EmitAdd(emitter, instruction);
                break;

            case OpCode.Subtract:
                break;

            case OpCode.Multiply:
                break;

            case OpCode.Divmod:
                break;

            case OpCode.And:
                break;

            case OpCode.Or:
                break;

            case OpCode.Xor:
                break;

            case OpCode.Not:
                break;

            case OpCode.Lsh:
                break;

            case OpCode.Rsh:
                break;

            case OpCode.Cmp:
                break;

            case OpCode.Push:
                break;

            case OpCode.Pop:
                break;

            case OpCode.Call:
                break;

            case OpCode.Ret:
                break;

            case OpCode.Jmp:
                break;

            case OpCode.Jmpe:
                break;

            case OpCode.Jmpne:
                break;

            case OpCode.Nop:
                emitter.EmitNoOp();
                break;

            case OpCode.Gks:
                EmitGks(instruction, emitter);
                break;

            case OpCode.Poll:
                break;

            default:
                break;
        }
    }

    private void EmitMove(Emitter emitter, Instruction instruction)
    {
        var source = instruction.Arguments[0];
        var target = instruction.Arguments[1];

        if (target is UnaryExpression unary
            && unary.OperatorToken.Text == "&"
            && unary.Expression is AddressOperationExpression addr
            && source is LiteralNode lit)
        {
            var value = ConvertNumber(lit);

            emitter.MoveRegisterImmediate(0, value);
            emitter.MoveAddressRegister(EvaluateExpression(addr.Expression), 0);
        }
        else if (source is LiteralNode lit2 && target is RegisterReferenceExpression reg)
        {
            var value = ConvertNumber(lit2);

            emitter.MoveRegisterImmediate(GetRegisterIndex(reg), value);
        }
        else if (source is UnaryExpression unary2
            && unary2.OperatorToken.Text == "&"
            && unary2.Expression is AddressOperationExpression addr2
            && target is RegisterReferenceExpression reg2)
        {
            //ToDo: Fix
            emitter.MoveAddressRegister(EvaluateExpression(addr2.Expression), GetRegisterIndex(reg2));
        }
    }

    private uint EvaluateExpression(Expression expr)
    {
        if (expr is LiteralNode lit)
        {
            return ConvertNumber(lit);
        }
        else if (expr is BinaryExpression binary)
        {
            var lhs = EvaluateExpression(binary.Left);
            var rhs = EvaluateExpression(binary.Right);

            switch (binary.OperatorToken.Text)
            {
                case "+": return lhs + rhs;
                case "-": return lhs - rhs;
                case "*": return lhs * rhs;
                case "/": return lhs / rhs;
            }
        }

        return 0;
    }

    private byte GetRegisterIndex(RegisterReferenceExpression reg)
    {
        if (reg.RegisterName == "A")
        {
            return 1;
        }

        if (reg.RegisterName == "B")
        {
            return 1;
        }

        return 0;
    }
}