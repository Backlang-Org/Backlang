namespace Backlang.Contracts.TypeSystem;

public class U32Type : DescribedType
{
    public U32Type(IAssembly assembly) : base(new SimpleName("UInt32").Qualify("System"), assembly)
    {
    }
}