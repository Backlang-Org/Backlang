namespace Backlang.Contracts.TypeSystem;

public class I64Type : DescribedType
{
    public I64Type(IAssembly assembly) : base(new SimpleName("Int64").Qualify("System"), assembly)
    {
    }
}