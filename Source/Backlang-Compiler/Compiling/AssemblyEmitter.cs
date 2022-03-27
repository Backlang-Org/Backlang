using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST;
using Backlang.Codeanalysis.Parsing.AST.Expressions;
using Backlang.Codeanalysis.Parsing.AST.Statements.Assembler;

namespace Backlang_Compiler.Compiling;

public class AssemblyEmitter
{
    private Dictionary<string, byte> _registers = new();

    public AssemblyEmitter()
    {
        InitRegisters();
    }

    public byte[] Emit(AssemblerBlockStatement node, out Emitter emitter)
    {
        emitter = new Emitter();

        foreach (var bodynode in node.Body)
        {
            if (bodynode is Instruction instruction)
            {
                EmitInstruction(instruction, emitter);
            }
        }

        return emitter.Result;
    }

    private static uint ConvertAddressNumber(LiteralNode lit)
    {
        var literalBytes = BitConverter.GetBytes((int)Convert.ChangeType(lit.Value, typeof(int)));

        var value = BitConverter.ToUInt32(literalBytes.ToArray());
        return value;
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

        if (first is RegisterReferenceExpression target && second is RegisterReferenceExpression source && third is LiteralNode imm)
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

            emitter.AddTarget(targetRegister, lhsRegister, rhsRegsiter);
        }
    }

    private void EmitAnd(Emitter emitter, Instruction instruction)
    {
        if (instruction.Arguments.Count == 3)
        {
            var targetArgument = instruction.Arguments[0];
            var lhsArgument = instruction.Arguments[1];
            var rhsArgument = instruction.Arguments[2];

            if (targetArgument is RegisterReferenceExpression targetReg && lhsArgument is RegisterReferenceExpression lhsReg
                && rhsArgument is RegisterReferenceExpression rhsReg)
            {
                var targetIndex = GetRegisterIndex(targetReg);
                var lhsIndex = GetRegisterIndex(lhsReg);
                var rhsIndex = GetRegisterIndex(rhsReg);

                emitter.And(targetIndex, lhsIndex, rhsIndex);
            }
        }
    }

    private void EmitCompare(Emitter emitter, Instruction instruction)
    {
        if (instruction.Arguments.Count == 3)
        {
            var targetArgument = instruction.Arguments[0];
            var lhsArgument = instruction.Arguments[1];
            var rhsArgument = instruction.Arguments[2];

            if (targetArgument is RegisterReferenceExpression targetReg && lhsArgument is RegisterReferenceExpression lhsReg
                && rhsArgument is RegisterReferenceExpression rhsReg)
            {
                var targetIndex = GetRegisterIndex(targetReg);
                var lhsIndex = GetRegisterIndex(lhsReg);
                var rhsIndex = GetRegisterIndex(rhsReg);

                emitter.Compare(targetIndex, lhsIndex, rhsIndex);
            }
        }
    }

    private void EmitDivMod(Emitter emitter, Instruction instruction)
    {
        if (instruction.Arguments.Count == 4)
        {
            var targetArgument = instruction.Arguments[0];
            var modArgument = instruction.Arguments[1];
            var lhsArgument = instruction.Arguments[2];
            var rhsArgument = instruction.Arguments[3];

            if (targetArgument is RegisterReferenceExpression targetReg && modArgument is RegisterReferenceExpression modReg
                && lhsArgument is RegisterReferenceExpression lhsReg && rhsArgument is RegisterReferenceExpression rhsReg)
            {
                var targetIndex = GetRegisterIndex(targetReg);
                var modIndex = GetRegisterIndex(modReg);
                var rhsIndex = GetRegisterIndex(rhsReg);
                var lhsIndex = GetRegisterIndex(lhsReg);

                emitter.DivMod(targetIndex, modIndex, lhsIndex, rhsIndex);
            }
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
                EmitSubtract(emitter, instruction);
                break;

            case OpCode.Multiply:
                EmitMultiply(emitter, instruction);
                break;

            case OpCode.Divmod:
                EmitDivMod(emitter, instruction);
                break;

            case OpCode.And:
                EmitAnd(emitter, instruction);
                break;

            case OpCode.Or:
                EmitOr(emitter, instruction);
                break;

            case OpCode.Xor:
                EmitXOr(emitter, instruction);
                break;

            case OpCode.Not:
                EmitNot(emitter, instruction);
                break;

            case OpCode.Lsh:
                EmitLeftShift(emitter, instruction);
                break;

            case OpCode.Rsh:
                EmitRightShift(emitter, instruction);
                break;

            case OpCode.Cmp:
                EmitCompare(emitter, instruction);
                break;

            case OpCode.Push:
                EmitPush(emitter, instruction);
                break;

            case OpCode.Pop:
                EmitPop(emitter, instruction);
                break;

            case OpCode.Call:
                break;

            case OpCode.Ret:
                break;

            case OpCode.Jmp:
                EmitJmp(instruction, emitter);
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

    private void EmitJmp(Instruction instruction, Emitter emitter)
    {
        var arg = instruction.Arguments[0];

        if (arg is RegisterReferenceExpression reg)
        {
            emitter.EmitJumpRegister(GetRegisterIndex(reg));
        }
        else if (arg is Expression addr)
        {
            emitter.EmitJump(EvaluateExpression(addr));
        }
    }

    private void EmitLeftShift(Emitter emitter, Instruction instruction)
    {
        if (instruction.Arguments.Count == 3)
        {
            var targetArgument = instruction.Arguments[0];
            var lhsArgument = instruction.Arguments[1];
            var rhsArgument = instruction.Arguments[2];

            if (targetArgument is RegisterReferenceExpression targetReg && lhsArgument is RegisterReferenceExpression lhsReg
                && rhsArgument is RegisterReferenceExpression rhsReg)
            {
                var targetIndex = GetRegisterIndex(targetReg);
                var lhsIndex = GetRegisterIndex(lhsReg);
                var rhsIndex = GetRegisterIndex(rhsReg);

                emitter.LeftShift(targetIndex, lhsIndex, rhsIndex);
            }
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
        else if (target is UnaryExpression unary2
            && unary2.OperatorToken.Text == "&"
            && unary2.Expression is AddressOperationExpression addr2
            && source is RegisterReferenceExpression reg2)
        {
            if (addr2.Expression is UnaryExpression ptrUnary)
            {
                if (ptrUnary.OperatorToken.Text == "PTR")
                {
                    var ptr = EvaluateExpression(ptrUnary.Expression);
                    emitter.MoveRegisterImmediate(0, ptr);
                    emitter.MovePointerSource(GetRegisterIndex(reg2), 0);
                }
            }
            else
            {
                emitter.MoveAddressRegister(EvaluateExpression(addr2.Expression), GetRegisterIndex(reg2));
            }
        }
        else if (source is UnaryExpression unary3
            && unary3.OperatorToken.Text == "&"
            && unary3.Expression is AddressOperationExpression addr3
            && target is RegisterReferenceExpression reg3)
        {
            if (addr3.Expression is UnaryExpression ptrUnary)
            {
                if (ptrUnary.OperatorToken.Text == "PTR")
                {
                    var ptr = EvaluateExpression(ptrUnary.Expression);
                    emitter.MoveRegisterImmediate(0, ptr);
                    emitter.MovePointerSource(0, GetRegisterIndex(reg3));
                }
            }
            else
            {
                emitter.MoveRegisterAddress(GetRegisterIndex(reg3), EvaluateExpression(addr3.Expression));
            }
        }
    }

    private void EmitMultiply(Emitter emitter, Instruction instruction)
    {
        // high, low, lhs, rhs

        if (instruction.Arguments.Count == 4)
        {
            var highArgument = instruction.Arguments[0];
            var lowArgument = instruction.Arguments[1];
            var lhsArgument = instruction.Arguments[2];
            var rhsArgument = instruction.Arguments[3];

            if (highArgument is RegisterReferenceExpression highReg && lowArgument is RegisterReferenceExpression lowReg
                && lhsArgument is RegisterReferenceExpression lhsReg && rhsArgument is RegisterReferenceExpression rhsReg)
            {
                var highIndex = GetRegisterIndex(highReg);
                var lowIndex = GetRegisterIndex(lowReg);
                var rhsIndex = GetRegisterIndex(rhsReg);
                var lhsIndex = GetRegisterIndex(lhsReg);

                emitter.Multiply(highIndex, lowIndex, lhsIndex, rhsIndex);
            }
        }
    }

    private void EmitNot(Emitter emitter, Instruction instruction)
    {
        if (instruction.Arguments.Count == 2)
        {
            var targetArgument = instruction.Arguments[0];
            var sourceArgument = instruction.Arguments[1];

            if (targetArgument is RegisterReferenceExpression targetReg
                && sourceArgument is RegisterReferenceExpression sourceReg)
            {
                var targetIndex = GetRegisterIndex(targetReg);
                var sourceIndex = GetRegisterIndex(sourceReg);

                emitter.Not(targetIndex, sourceIndex);
            }
        }
    }

    private void EmitOr(Emitter emitter, Instruction instruction)
    {
        if (instruction.Arguments.Count == 3)
        {
            var targetArgument = instruction.Arguments[0];
            var lhsArgument = instruction.Arguments[1];
            var rhsArgument = instruction.Arguments[2];

            if (targetArgument is RegisterReferenceExpression targetReg && lhsArgument is RegisterReferenceExpression lhsReg
                && rhsArgument is RegisterReferenceExpression rhsReg)
            {
                var targetIndex = GetRegisterIndex(targetReg);
                var lhsIndex = GetRegisterIndex(lhsReg);
                var rhsIndex = GetRegisterIndex(rhsReg);

                emitter.Or(targetIndex, lhsIndex, rhsIndex);
            }
        }
    }

    private void EmitPop(Emitter emitter, Instruction instruction)
    {
        if (instruction.Arguments.Count == 1)
        {
            var targetArgument = instruction.Arguments[0];

            if (targetArgument is RegisterReferenceExpression targetReg)
            {
                var targetIndex = GetRegisterIndex(targetReg);

                emitter.PopRegister(targetIndex);
            }
        }
    }

    private void EmitPush(Emitter emitter, Instruction instruction)
    {
        if (instruction.Arguments.Count == 1)
        {
            var targetArgument = instruction.Arguments[0];

            if (targetArgument is RegisterReferenceExpression targetReg)
            {
                var targetIndex = GetRegisterIndex(targetReg);

                emitter.PushRegister(targetIndex);
            }
        }
    }

    private void EmitRightShift(Emitter emitter, Instruction instruction)
    {
        if (instruction.Arguments.Count == 3)
        {
            var targetArgument = instruction.Arguments[0];
            var lhsArgument = instruction.Arguments[1];
            var rhsArgument = instruction.Arguments[2];

            if (targetArgument is RegisterReferenceExpression targetReg && lhsArgument is RegisterReferenceExpression lhsReg
                && rhsArgument is RegisterReferenceExpression rhsReg)
            {
                var targetIndex = GetRegisterIndex(targetReg);
                var lhsIndex = GetRegisterIndex(lhsReg);
                var rhsIndex = GetRegisterIndex(rhsReg);

                emitter.RightShift(targetIndex, lhsIndex, rhsIndex);
            }
        }
    }

    private void EmitSubtract(Emitter emitter, Instruction instruction)
    {
        var first = instruction.Arguments[0];
        var second = instruction.Arguments[1];
        var third = instruction.Arguments[2];

        if (first is RegisterReferenceExpression target && second is RegisterReferenceExpression source && third is Expression imm)
        {
            var targetRegister = GetRegisterIndex(target);
            var sourceRegister = GetRegisterIndex(source);
            var value = EvaluateExpression(imm);

            emitter.Subtract(targetRegister, sourceRegister, value);
        }
        else if (first is RegisterReferenceExpression target2 && second is RegisterReferenceExpression lhs && third is RegisterReferenceExpression rhs)
        {
            var targetRegister = GetRegisterIndex(target2);
            var lhsRegister = GetRegisterIndex(lhs);
            var rhsRegsiter = GetRegisterIndex(rhs);

            emitter.SubtractTarget(targetRegister, lhsRegister, rhsRegsiter);
        }
    }

    private void EmitXOr(Emitter emitter, Instruction instruction)
    {
        if (instruction.Arguments.Count == 3)
        {
            var targetArgument = instruction.Arguments[0];
            var lhsArgument = instruction.Arguments[1];
            var rhsArgument = instruction.Arguments[2];

            if (targetArgument is RegisterReferenceExpression targetReg && lhsArgument is RegisterReferenceExpression lhsReg
                && rhsArgument is RegisterReferenceExpression rhsReg)
            {
                var targetIndex = GetRegisterIndex(targetReg);
                var lhsIndex = GetRegisterIndex(lhsReg);
                var rhsIndex = GetRegisterIndex(rhsReg);

                emitter.Xor(targetIndex, lhsIndex, rhsIndex);
            }
        }
    }

    private uint EvaluateExpression(Expression expr)
    {
        if (expr is LiteralNode lit)
        {
            return ConvertAddressNumber(lit);
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
        if (_registers.ContainsKey(reg.RegisterName))
        {
            return _registers[reg.RegisterName];
        }

        return 0;
    }

    private void InitRegisters()
    {
        const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //A, B, .. Z, AA, AB, AAA, ..
        for (byte i = 0; i < 251; i++)
        {
            _registers.Add(ALPHABET[i % 26] + ((i / 26) == 0 ? "" : ALPHABET[i / 26 - 1].ToString()), i);
        }

        _registers.Add("CCH", 251); // Cycle Count High
        _registers.Add("CCL", 252); // Cycle Count Low
        _registers.Add("FLGS", 253); // Flags
        _registers.Add("IP", 254); // Instruction Pointer
        _registers.Add("SP", 255); // Stack Pointer
    }
}