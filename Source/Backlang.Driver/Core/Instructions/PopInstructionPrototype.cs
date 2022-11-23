namespace Backlang.Driver.Core.Instructions;

internal class PopInstructionPrototype : InstructionPrototype
{
    public override IType ResultType => null;

    public override int ParameterCount => 0;

    public override IReadOnlyList<string> CheckConformance(Instruction instance, MethodBody body)
    {
        return null;
    }

    public override InstructionPrototype Map(MemberMapping mapping)
    {
        return null;
    }
}