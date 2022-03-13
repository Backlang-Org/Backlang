using Address = System.UInt32;
using Instruction = System.UInt64;
using Register = System.Byte;

using Word = System.UInt32;

namespace Backlang_Compiler.Compiling;

public class Emitter
{
    private Stream stream;
    private BinaryWriter writer;

    public Emitter()
    {
        stream = new MemoryStream();
        writer = new BinaryWriter(stream);
    }

    public void Add(Register target, Register source, Word value)
    {
        writer.Write(0x0012_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)source) << 32
            | (Instruction)value);
    }

    public void AddTarget(Register target, Register lhs, Register rhs)
    {
        writer.Write(0x0007_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void And(Register target, Register lhs, Register rhs)
    {
        writer.Write(0x000C_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void Compare(Register target, Register lhs, Register rhs)
    {
        writer.Write(0x0014_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void DivMod(Register target, Register mod, Register lhs, Register rhs)
    {
        writer.Write(0x000B_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)mod) << 32
            | ((Instruction)lhs) << 24
            | ((Instruction)rhs) << 16);
    }

    public void EmitHalt()
    {
        writer.Write(0x0006_0000_0000_0000);
    }

    public void LeftShift(Register target, Register lhs, Register rhs)
    {
        writer.Write(0x0010_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void MoveAddressRegister(Address address, Register register)
    {
        writer.Write(0x0003_0000_0000_0000 | ((Instruction)register) << 40 | (Instruction)address);
    }

    public void MovePointerSource(Register pointer, Register source)
    {
        writer.Write(0x0005_0000_0000_0000 | ((Instruction)pointer) << 40 | ((Instruction)source) << 32);
    }

    public void MoveRegisterAddress(Register register, Address address)
    {
        writer.Write(0x0001_0000_0000_0000 | ((Instruction)register) << 40 | (Instruction)address);
    }

    public void MoveRegisterImmediate(Register register, Word value)
    {
        writer.Write((Instruction)(register) << 40 | value);
    }

    public void MoveTargetPointer(Register target, Register pointer)
    {
        writer.Write(0x0004_0000_0000_0000 | ((Instruction)target) << 40 | ((Instruction)pointer) << 32);
    }

    public void MoveTargetSource(Register target, Register source)
    {
        writer.Write(0x0002_0000_0000_0000 | ((Instruction)target) << 40 | ((Instruction)source) << 32);
    }

    public void Multiply(Register target_high, Register target_low, Register lhs, Register rhs)
    {
        writer.Write(0x000A_0000_0000_0000
            | ((Instruction)target_high) << 40
            | ((Instruction)target_low) << 32
            | ((Instruction)lhs) << 24
            | ((Instruction)rhs) << 16);
    }

    public void Not(Register target, Register source)
    {
        writer.Write(0x000F_0000_0000_0000 | ((Instruction)target) << 40 | ((Instruction)source) << 32);
    }

    public void Or(Register target, Register lhs, Register rhs)
    {
        writer.Write(0x000D_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void PopRegister(Register register)
    {
        writer.Write(0x0016_0000_0000_0000 | ((Instruction)register) << 40);
    }

    public void PushRegister(Register register)
    {
        writer.Write(0x0015_0000_0000_0000 | ((Instruction)register) << 40);
    }

    public void RightShift(Register target, Register lhs, Register rhs)
    {
        writer.Write(0x0011_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void Subtract(Register target, Register source, Word value)
    {
        writer.Write(0x0013_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)source) << 32
            | (Instruction)value);
    }

    public void SubtractTarget(Register target, Register lhs, Register rhs)
    {
        writer.Write(0x0008_0000_0000_0000
        | ((Instruction)target) << 40
        | ((Instruction)lhs) << 32
        | ((Instruction)rhs) << 24);
    }

    public void SubtractWithCarry(
        Register target, Register lhs, Register rhs)
    {
        writer.Write(0x0009_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }

    public void Xor(Register target, Register lhs, Register rhs)
    {
        writer.Write(0x000E_0000_0000_0000
            | ((Instruction)target) << 40
            | ((Instruction)lhs) << 32
            | ((Instruction)rhs) << 24);
    }
}