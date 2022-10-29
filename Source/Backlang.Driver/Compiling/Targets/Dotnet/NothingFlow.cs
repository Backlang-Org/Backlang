namespace Backlang.Driver.Compiling.Targets.Dotnet;

internal class NothingFlow : BlockFlow
{
    public override IReadOnlyList<Furesoft.Core.CodeDom.Compiler.Instruction> Instructions => throw new NotImplementedException();

    public override IReadOnlyList<Branch> Branches => throw new NotImplementedException();

    public override InstructionBuilder GetInstructionBuilder(BasicBlockBuilder block, int instructionIndex)
    {
        throw new NotImplementedException();
    }

    public override BlockFlow WithBranches(IReadOnlyList<Branch> branches)
    {
        throw new NotImplementedException();
    }

    public override BlockFlow WithInstructions(IReadOnlyList<Furesoft.Core.CodeDom.Compiler.Instruction> instructions)
    {
        throw new NotImplementedException();
    }
}
