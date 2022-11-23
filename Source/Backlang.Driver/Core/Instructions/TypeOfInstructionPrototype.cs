namespace Backlang.Driver.Core.Instructions;

internal class TypeOfInstructionPrototype : InstructionPrototype
{
    public TypeOfInstructionPrototype(IType type)
    {
        Type = type;
    }

    public IType Type { get; set; }
    public override IType ResultType => Type;

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
