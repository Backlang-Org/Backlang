namespace Backlang.Contracts.TypeSystem;

public class U64Type : DescribedType
{
    public U64Type(IAssembly assembly) : base(new SimpleName("UInt64").Qualify("System"), assembly)
    {
    }
}