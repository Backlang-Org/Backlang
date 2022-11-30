using Furesoft.Core.CodeDom.Compiler.Core.Collections;

namespace Backlang.Driver.Core.Flows;

public class ContinueFlow : BlockFlow
{
    public ContinueFlow(Branch branch)
    {
        Branch = branch;
    }

    public ContinueFlow()
    {

    }

    /// <summary>
    /// Gets the branch that is taken by
    /// this flow.
    /// </summary>
    /// <returns>The jump branch.</returns>
    public Branch Branch { get; private set; }

    /// <inheritdoc/>
    public override IReadOnlyList<Instruction> Instructions => EmptyArray<Instruction>.Value;

    /// <inheritdoc/>
    public override IReadOnlyList<Branch> Branches => new Branch[] { Branch };

    /// <inheritdoc/>
    public override BlockFlow WithInstructions(IReadOnlyList<Instruction> instructions)
    {
        ContractHelpers.Assert(instructions.Count == 0, "Jump flow does not take any instructions.");
        return this;
    }

    /// <inheritdoc/>
    public override BlockFlow WithBranches(IReadOnlyList<Branch> branches)
    {
        ContractHelpers.Assert(branches.Count == 1, "Jump flow takes exactly one branch.");
        var newBranch = branches[0];
        if (object.ReferenceEquals(newBranch, Branch))
        {
            return this;
        }
        else
        {
            return new BreakFlow(newBranch);
        }
    }

    /// <inheritdoc/>
    public override InstructionBuilder GetInstructionBuilder(
        BasicBlockBuilder block,
        int instructionIndex)
    {
        throw new IndexOutOfRangeException();
    }
}