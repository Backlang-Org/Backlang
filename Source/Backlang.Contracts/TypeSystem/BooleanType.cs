namespace Backlang.Contracts.TypeSystem;

public class BooleanType : DescribedType
{
    public BooleanType(IAssembly assembly) : base(new SimpleName("Boolean").Qualify("System"), assembly)
    {
    }
}