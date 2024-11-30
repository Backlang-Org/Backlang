namespace Backlang.Contracts.TypeSystem;

public class I16Type : DescribedType
{
    public I16Type(IAssembly assembly) : base(new SimpleName("Int16").Qualify("System"), assembly)
    {
    }
}