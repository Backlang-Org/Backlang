using Backlang_Compiler.TypeSystem;
using Be.IO;
using Newtonsoft.Json;
using System.Reflection;
using Address = System.UInt32;
using Instruction = System.UInt64;
using Register = System.Byte;

using Word = System.UInt32;

namespace Backlang_Compiler.Compiling;

public class Emitter
{
    private OpcodeMap _machineinfo;
    private MemoryStream _stream;
    private BeBinaryWriter _writer;

    public Emitter()
    {
        _stream = new MemoryStream();
        _writer = new BeBinaryWriter(_stream);

        var jsonStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Backlang_Compiler.machineinfo.json");
        var json = new StreamReader(jsonStream).ReadToEnd();

        _machineinfo = JsonConvert.DeserializeObject<OpcodeMap>(json);
    }

    public uint Current => (uint)_stream.Length;
    public byte[] Result => _stream.ToArray();

    public void Add(Register target, Register source, Word value)
    {
        var opcode = GetOpcodeFor("AddTargetSourceImmediate");

        _writer.Write(0x0000_0000_0000_0000
            | ((Instruction)opcode) << 48
            | ((Instruction)target) << 40
            | ((Instruction)source) << 32
            | (Instruction)value);
    }

    public void AddTarget(Register target, Register lhs, Register rhs)
    {
        var opcode = GetOpcodeFor("AddTargetLhsRhs");

        _writer.Write(0x0000_0000_0000_0000
            | ((Instruction)opcode) << 48
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void And(Register target, Register lhs, Register rhs)
    {
        var opcode = GetOpcodeFor("AndTargetLhsRhs");

        _writer.Write(0x0000_0000_0000_0000
            | ((Instruction)opcode) << 48
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void Compare(Register target, Register lhs, Register rhs)
    {
        var opcode = GetOpcodeFor("CompareTargetLhsRhs");

        _writer.Write(0x0000_0000_0000_0000
            | ((Instruction)opcode) << 48
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void DivMod(Register target, Register mod, Register lhs, Register rhs)
    {
        var opcode = GetOpcodeFor("DivmodTargetModLhsRhs");

        _writer.Write(0x0000_0000_0000_0000
            | ((Instruction)opcode) << 48
            | ((Instruction)target) << 40
            | ((Instruction)mod) << 32
            | ((Instruction)lhs) << 24
            | ((Instruction)rhs) << 16);
    }

    public void EmitHalt()
    {
        var opcode = GetOpcodeFor("HaltAndCatchFire");

        _writer.Write(0x0000_0000_0000_0000 | ((Instruction)opcode) << 48);
    }

    public void EmitJump(Address address)
    {
        var opcode = GetOpcodeFor("JumpAddress");

        _writer.Write(0x0000_0000_0000_0000 | (Instruction)opcode << 48 | (((Instruction)address)));
    }

    public void EmitJumpIfEqual(Register register, Address address)
    {
        var opcode = GetOpcodeFor("JumpAddressIfEqual");

        _writer.Write(0x0000_0000_0000_0000 | (Instruction)opcode << 48 | ((Instruction)register) << 40 | (Instruction)address);
    }

    public void EmitJumpRegister(Register register)
    {
        var opcode = GetOpcodeFor("JumpRegister");

        _writer.Write(0x0000_0000_0000_0000
            | (Instruction)opcode << 48
            | ((Instruction)register) << 40);
    }

    public void EmitLiteral(bool value)
    {
        _writer.Write(0x0000_0000_0000_0000
            | ((byte)PrimitiveObjectID.Bool) << 60);
        _writer.Write(0x0000_0000_0000_0000
            | (value ? 1 : 0));
    }

    public void EmitLiteral(byte value)
    {
        _writer.Write(0x0000_0000_0000_0000
            | ((byte)PrimitiveObjectID.I8) << 60);
        _writer.Write(0x0000_0000_0000_0000
            | value);
    }

    public void EmitLiteral(short value)
    {
        _writer.Write(0x0000_0000_0000_0000
            | ((byte)PrimitiveObjectID.I16) << 60);
        _writer.Write(0x0000_0000_0000_0000
            | value);
    }

    public void EmitLiteral(int value)
    {
        _writer.Write(0x0000_0000_0000_0000
            | ((byte)PrimitiveObjectID.I32) << 60);
        _writer.Write(0x0000_0000_0000_0000
            | value);
    }

    public void EmitLiteral(long value)
    {
        _writer.Write(0x0000_0000_0000_0000
            | ((byte)PrimitiveObjectID.I64) << 60);
        _writer.Write(0x0000_0000_0000_0000
            | value);
    }

    public void EmitLiteral(string value)
    {
        _writer.Write(0x0000_0000_0000_0000
            | ((byte)PrimitiveObjectID.String) << 60
            | value.Length << 32);
        _writer.Write(System.Text.Encoding.ASCII.GetBytes(value));
    }

    public void EmitNegate(Register target, Register source)
    {
        Not(target, source);
        Add(target, target, 1);
    }

    public void EmitNoneLiteral()
    {
        _writer.Write(0x0000_0000_0000_0000);
        _writer.Write(0x0000_0000_0000_0000);
    }

    public void EmitNoOp()
    {
        var opcode = GetOpcodeFor("NoOp");

        _writer.Write(0x0000_0000_0000_0000 | ((Instruction)opcode) << 48);
    }

    public void GetKeyState(Register target, Register keycode)
    {
        var opcode = GetOpcodeFor("GetKeyState");

        _writer.Write(0x0000_0000_0000_0000
            | ((Instruction)opcode) << 48
            | ((Instruction)target) << 40
            | ((Instruction)keycode) << 32);
    }

    public short GetOpcodeFor(string name)
    {
        return (short)GetInstructionInfo(name).opcode;
    }

    public void LeftShift(Register target, Register lhs, Register rhs)
    {
        var opcode = GetOpcodeFor("LeftShiftTargetLhsRhs");

        _writer.Write(0x0010_0000_0000_0000
            | ((Instruction)opcode) << 48
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void MoveAddressRegister(Address address, Register register)
    {
        var opcode = GetOpcodeFor("MoveAddressRegister");

        _writer.Write(0x0000_0000_0000_0000 | (Instruction)opcode << 48 | ((Instruction)register) << 40 | (Instruction)address);
    }

    public void MovePointerSource(Register pointer, Register source)
    {
        var opcode = GetOpcodeFor("MovePointerSource");

        _writer.Write(0x0000_0000_0000_0000 | (Instruction)opcode << 48 | ((Instruction)pointer) << 40 | ((Instruction)source) << 32);
    }

    public void MoveRegisterAddress(Register register, Address address)
    {
        var opcode = GetOpcodeFor("MoveRegisterAddress");

        _writer.Write(0x0000_0000_0000_0000 | (Instruction)opcode << 48 | ((Instruction)register) << 40 | (Instruction)address);
    }

    public void MoveRegisterImmediate(Register register, Word value)
    {
        var opcode = GetOpcodeFor("MoveRegisterImmediate");

        _writer.Write(0x0000_0000_0000_0000 | (Instruction)opcode << 48 | (Instruction)(register) << 40 | value);
    }

    public void MoveTargetPointer(Register target, Register pointer)
    {
        var opcode = GetOpcodeFor("MoveTargetPointer");

        _writer.Write(0x0000_0000_0000_0000 | (Instruction)opcode << 48 | ((Instruction)target) << 40 | ((Instruction)pointer) << 32);
    }

    public void MoveTargetSource(Register target, Register source)
    {
        var opcode = GetOpcodeFor("MoveTargetSource");

        _writer.Write(0x0000_0000_0000_0000 | (Instruction)opcode << 48 | ((Instruction)target) << 40 | ((Instruction)source) << 32);
    }

    public void Multiply(Register target_high, Register target_low, Register lhs, Register rhs)
    {
        var opcode = GetOpcodeFor("MultiplyHighLowLhsRhs");

        _writer.Write(0x0000_0000_0000_0000
            | (Instruction)opcode << 48
            | ((Instruction)target_high) << 40
            | ((Instruction)target_low) << 32
            | ((Instruction)lhs) << 24
            | ((Instruction)rhs) << 16);
    }

    public void Not(Register target, Register source)
    {
        var opcode = GetOpcodeFor("NotTargetSource");

        _writer.Write(0x0000_0000_0000_0000 | (Instruction)opcode << 48 | ((Instruction)target) << 40 | ((Instruction)source) << 32);
    }

    public void Or(Register target, Register lhs, Register rhs)
    {
        var opcode = GetOpcodeFor("OrTargetLhsRhs");

        _writer.Write(0x0000_0000_0000_0000
            | (Instruction)opcode << 48
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void PollTime(Register high, Register low)
    {
        var opcode = GetOpcodeFor("PollTime");

        _writer.Write(0x0000_0000_0000_0000
            | (Instruction)opcode << 48
            | ((Instruction)high) << 40
            | ((Instruction)low << 32));
    }

    public void PopRegister(Register register)
    {
        var opcode = GetOpcodeFor("PopRegister");

        _writer.Write(0x0000_0000_0000_0000 | (Instruction)opcode << 48 | ((Instruction)register) << 40);
    }

    public void PushRegister(Register register)
    {
        var opcode = GetOpcodeFor("PushRegister");

        _writer.Write(0x0000_0000_0000_0000 | (Instruction)opcode << 48 | ((Instruction)register) << 40);
    }

    public void RightShift(Register target, Register lhs, Register rhs)
    {
        var opcode = GetOpcodeFor("RightShiftTargetLhsRhs");

        _writer.Write(0x0000_0000_0000_0000
            | (Instruction)opcode << 48
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void Subtract(Register target, Register source, Word value)
    {
        var opcode = GetOpcodeFor("SubtractTargetSourceImmediate");

        _writer.Write(0x0000_0000_0000_0000
            | (Instruction)opcode << 48
            | ((Instruction)target) << 40
            | ((Instruction)source) << 32
            | (Instruction)value);
    }

    public void SubtractTarget(Register target, Register lhs, Register rhs)
    {
        var opcode = GetOpcodeFor("SubtractTargetLhsRhs");

        _writer.Write(0x0000_0000_0000_0000
            | (Instruction)opcode << 48
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void SubtractWithCarry(
        Register target, Register lhs, Register rhs)
    {
        var opcode = GetOpcodeFor("SubtractWithCarryTargetLhsRhs");

        _writer.Write(0x0000_0000_0000_0000
            | (Instruction)opcode << 48
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void Xor(Register target, Register lhs, Register rhs)
    {
        var opcode = GetOpcodeFor("XorTargetLhsRhs");

        _writer.Write(0x0000_0000_0000_0000
            | (Instruction)opcode << 48
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    private InstructionInfo GetInstructionInfo(string name)
    {
        return _machineinfo.opcodes[name];
    }

    public struct InstructionInfo
    {
        public short opcode { get; set; }
        public string opcode_type { get; set; }
        public string[,] registers { get; set; }
    }

    private class OpcodeMap
    {
        public Dictionary<string, InstructionInfo> opcodes { get; set; } = new();
    }
}