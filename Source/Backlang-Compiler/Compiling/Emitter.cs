using Backlang_Compiler.TypeSystem;
using Be.IO;
using Address = System.UInt32;
using Instruction = System.UInt64;
using Register = System.Byte;

using Word = System.UInt32;

namespace Backlang_Compiler.Compiling;

public class Emitter
{
    private MemoryStream _stream;
    private BeBinaryWriter _writer;

    public Emitter()
    {
        _stream = new MemoryStream();
        _writer = new BeBinaryWriter(_stream);
    }

    public byte[] Result => _stream.ToArray();

    public void Add(Register target, Register source, Word value)
    {
        _writer.Write(0x0012_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)source) << 32
            | (Instruction)value);
    }

    public void AddTarget(Register target, Register lhs, Register rhs)
    {
        _writer.Write(0x0007_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void And(Register target, Register lhs, Register rhs)
    {
        _writer.Write(0x000C_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void Compare(Register target, Register lhs, Register rhs)
    {
        _writer.Write(0x0014_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void DivMod(Register target, Register mod, Register lhs, Register rhs)
    {
        _writer.Write(0x000B_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)mod) << 32
            | ((Instruction)lhs) << 24
            | ((Instruction)rhs) << 16);
    }

    public void EmitHalt()
    {
        _writer.Write(0x0006_0000_0000_0000);
    }

    public void EmitJump(Register register)
    {
        _writer.Write(0x001A_0000_0000_0000
            | ((Instruction)register) << 40);
    }

    public void EmitJump(Address register)
    {
        _writer.Write(0x0019_0000_0000_0000
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

    public void EmitNoneLiteral()
    {
        _writer.Write(0x0000_0000_0000_0000);
        _writer.Write(0x0000_0000_0000_0000);
    }

    public void EmitNoOp()
    {
        _writer.Write(0x0031_0000_0000_0000);
    }

    public void GetKeyState(Register target, Register keycode)
    {
        _writer.Write(0x0032_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)keycode) << 32);
    }

    public void LeftShift(Register target, Register lhs, Register rhs)
    {
        _writer.Write(0x0010_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void MoveAddressRegister(Address address, Register register)
    {
        _writer.Write(0x0003_0000_0000_0000 | ((Instruction)register) << 40 | (Instruction)address);
    }

    public void MovePointerSource(Register pointer, Register source)
    {
        _writer.Write(0x0005_0000_0000_0000 | ((Instruction)pointer) << 40 | ((Instruction)source) << 32);
    }

    public void MoveRegisterAddress(Register register, Address address)
    {
        _writer.Write(0x0001_0000_0000_0000 | ((Instruction)register) << 40 | (Instruction)address);
    }

    public void MoveRegisterImmediate(Register register, Word value)
    {
        _writer.Write(0x0000_0000_0000_0000 | (Instruction)(register) << 40 | value);
    }

    public void MoveTargetPointer(Register target, Register pointer)
    {
        _writer.Write(0x0004_0000_0000_0000 | ((Instruction)target) << 40 | ((Instruction)pointer) << 32);
    }

    public void MoveTargetSource(Register target, Register source)
    {
        _writer.Write(0x0002_0000_0000_0000 | ((Instruction)target) << 40 | ((Instruction)source) << 32);
    }

    public void Multiply(Register target_high, Register target_low, Register lhs, Register rhs)
    {
        _writer.Write(0x000A_0000_0000_0000
            | ((Instruction)target_high) << 40
            | ((Instruction)target_low) << 32
            | ((Instruction)lhs) << 24
            | ((Instruction)rhs) << 16);
    }

    public void Not(Register target, Register source)
    {
        _writer.Write(0x000F_0000_0000_0000 | ((Instruction)target) << 40 | ((Instruction)source) << 32);
    }

    public void Or(Register target, Register lhs, Register rhs)
    {
        _writer.Write(0x000D_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void PollTime(Register high, Register low)
    {
        _writer.Write(0x0033_0000_0000_0000
            | ((Instruction)high) << 40
            | ((Instruction)low << 32));
    }

    public void PopRegister(Register register)
    {
        _writer.Write(0x0016_0000_0000_0000 | ((Instruction)register) << 40);
    }

    public void PushRegister(Register register)
    {
        _writer.Write(0x0015_0000_0000_0000 | ((Instruction)register) << 40);
    }

    public void RightShift(Register target, Register lhs, Register rhs)
    {
        _writer.Write(0x0011_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void Subtract(Register target, Register source, Word value)
    {
        _writer.Write(0x0013_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)source) << 32
            | (Instruction)value);
    }

    public void SubtractTarget(Register target, Register lhs, Register rhs)
    {
        _writer.Write(0x0008_0000_0000_0000
        | ((Instruction)target) << 40
        | ((Instruction)lhs) << 32
        | ((Instruction)rhs) << 24);
    }

    public void SubtractWithCarry(
        Register target, Register lhs, Register rhs)
    {
        _writer.Write(0x0009_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void Xor(Register target, Register lhs, Register rhs)
    {
        _writer.Write(0x000E_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }
}